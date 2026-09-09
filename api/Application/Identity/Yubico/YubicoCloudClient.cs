using System.Security.Cryptography;
using System.Text;
using Contract.Identity.WebAuthn;
using Contract.Identity.Yubico;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace Application.Identity.Yubico
{
    /// <summary>
    /// Asks Yubico's validation service whether an OTP is genuine and unused.
    ///
    /// Verified against the real service on 2026-08-23 with a company key: a fresh OTP answered OK,
    /// the same OTP a second time answered REPLAYED_OTP, and a fabricated one answered BAD_OTP. That
    /// middle answer is the reason this replaced a local implementation — remembering which OTPs
    /// have been spent is the hard half of the protocol, and Yubico already does it across every
    /// server in the pool.
    ///
    /// What it does NOT answer is whose key produced the OTP. That check stays ours; see
    /// YubicoOtpVerifier.
    /// </summary>
    public class YubicoCloudClient : ITransientDependency
    {
        private const string DefaultEndpoint = "https://api.yubico.com/wsapi/2.0/verify";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IWebAuthnSettingService _settingService;
        private readonly ILogger<YubicoCloudClient> _logger;

        public YubicoCloudClient(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IWebAuthnSettingService settingService,
            ILogger<YubicoCloudClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _settingService = settingService;
            _logger = logger;
        }

        public async Task<YubicoCloudResult> VerifyAsync(string otp, CancellationToken cancellationToken = default)
        {
            // Database first, environment second — see GetYubicoCredentialsAsync. Variables die
            // with the container; the row survives it, which is what makes this repairable from
            // the settings screen instead of from a shell.
            var (clientId, secret) = await _settingService.GetYubicoCredentialsAsync();

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(secret))
            {
                _logger.LogError(
                    "Yubico credentials are missing from both webauthn_settings and configuration.");
                return new YubicoCloudResult(YubicoCloudStatus.NotConfigured);
            }

            // A fresh nonce per request. The service echoes it back, and comparing it is what stops
            // a captured reply from being handed to us in place of the answer to this request.
            var nonce = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();

            var parameters = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["id"] = clientId,
                ["nonce"] = nonce,
                ["otp"] = otp
            };
            parameters["h"] = YubicoSignature.Sign(parameters, secret);

            var url = _configuration["Yubico:ApiUrl"] ?? DefaultEndpoint;
            var query = string.Join('&', parameters.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));

            string body;
            try
            {
                var client = _httpClientFactory.CreateClient();
                // Short on purpose: this sits in the middle of a login. A validation service having
                // a bad day must not become a login screen that hangs.
                client.Timeout = TimeSpan.FromSeconds(5);
                body = await client.GetStringAsync($"{url}?{query}", cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not reach the Yubico validation service.");
                return new YubicoCloudResult(YubicoCloudStatus.Unreachable);
            }

            return Interpret(body, secret, otp, nonce);
        }

        private YubicoCloudResult Interpret(string body, string secret, string sentOtp, string sentNonce)
        {
            var fields = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (var line in body.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = line.Trim();
                var split = trimmed.IndexOf('=');
                if (split > 0)
                {
                    fields[trimmed[..split]] = trimmed[(split + 1)..];
                }
            }

            if (!fields.TryGetValue("status", out var status))
            {
                _logger.LogWarning("Yubico response carried no status field.");
                return new YubicoCloudResult(YubicoCloudStatus.Unreachable);
            }

            // Every check below must pass before the status is believed. Trusting "status=OK" on its
            // own would mean trusting anyone who can answer faster than Yubico.
            if (fields.TryGetValue("h", out var signature))
            {
                var expected = new SortedDictionary<string, string>(fields, StringComparer.Ordinal);
                expected.Remove("h");
                if (!YubicoSignature.Matches(expected, secret, signature))
                {
                    _logger.LogError("Yubico response signature did not verify — check Yubico:SecretKey.");
                    return new YubicoCloudResult(YubicoCloudStatus.BadResponseSignature);
                }
            }
            else if (status == "OK")
            {
                // An unsigned OK is exactly what a forged reply looks like.
                _logger.LogError("Yubico returned OK without a signature; refusing it.");
                return new YubicoCloudResult(YubicoCloudStatus.BadResponseSignature);
            }

            if (status == "OK")
            {
                if (!string.Equals(fields.GetValueOrDefault("otp"), sentOtp, StringComparison.Ordinal)
                    || !string.Equals(fields.GetValueOrDefault("nonce"), sentNonce, StringComparison.Ordinal))
                {
                    _logger.LogError("Yubico response echoed a different otp/nonce than was sent.");
                    return new YubicoCloudResult(YubicoCloudStatus.MismatchedEcho);
                }
            }

            return new YubicoCloudResult(Parse(status));
        }

        private static YubicoCloudStatus Parse(string status) => status switch
        {
            "OK" => YubicoCloudStatus.Ok,
            "BAD_OTP" => YubicoCloudStatus.BadOtp,
            "REPLAYED_OTP" => YubicoCloudStatus.ReplayedOtp,
            "REPLAYED_REQUEST" => YubicoCloudStatus.ReplayedRequest,
            "BAD_SIGNATURE" => YubicoCloudStatus.BadRequestSignature,
            "NO_SUCH_CLIENT" => YubicoCloudStatus.NoSuchClient,
            "OPERATION_NOT_ALLOWED" => YubicoCloudStatus.OperationNotAllowed,
            "MISSING_PARAMETER" => YubicoCloudStatus.MissingParameter,
            "NOT_ENOUGH_ANSWERS" => YubicoCloudStatus.NotEnoughAnswers,
            _ => YubicoCloudStatus.BackendError
        };
    }

    /// <summary>
    /// HMAC-SHA1 over the request and response fields, exactly as Yubico specifies: drop `h`, sort
    /// the pairs by key, join them as key=value with `&amp;`, then sign with the base64-decoded API
    /// key. Kept separate from the HTTP call so it can be tested without a network.
    /// </summary>
    public static class YubicoSignature
    {
        public static string Sign(IEnumerable<KeyValuePair<string, string>> parameters, string base64Secret)
        {
            var ordered = parameters
                .Where(p => p.Key != "h")
                .OrderBy(p => p.Key, StringComparer.Ordinal)
                .Select(p => $"{p.Key}={p.Value}");

            var message = string.Join('&', ordered);
            using var hmac = new HMACSHA1(Convert.FromBase64String(base64Secret));
            return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(message)));
        }

        public static bool Matches(
            IEnumerable<KeyValuePair<string, string>> parameters, string base64Secret, string presented)
        {
            string expected;
            try
            {
                expected = Sign(parameters, base64Secret);
            }
            catch (FormatException)
            {
                return false;   // secret is not valid base64 — a configuration fault, not a match
            }

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(presented ?? string.Empty));
        }
    }
}
