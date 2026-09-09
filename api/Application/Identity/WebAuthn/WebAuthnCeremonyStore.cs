using Microsoft.Extensions.Caching.Memory;
using Volo.Abp.DependencyInjection;

namespace Application.Identity.WebAuthn
{
    /// <summary>
    /// Holds the challenge between the begin and complete halves of a ceremony, and counts failed
    /// assertions per user.
    /// </summary>
    public class WebAuthnCeremonyStore : ISingletonDependency
    {
        private const int MaxFailuresBeforeLockout = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan FailureWindow = TimeSpan.FromMinutes(15);

        private readonly IMemoryCache _cache;

        public WebAuthnCeremonyStore(IMemoryCache cache)
        {
            _cache = cache;
        }

        // ponytail: challenges and failure counts live in process memory. The API runs a single
        // container per environment (docker-compose.yml has no replicas), so this holds. Behind a
        // load balancer, ceremonies would fail at random and the counter would be per-replica —
        // move both to a distributed cache or a table at that point.

        public void SaveChallenge(string ceremonyId, string optionsJson, TimeSpan lifetime) =>
            _cache.Set(ChallengeKey(ceremonyId), optionsJson, lifetime);

        public string? TakeChallenge(string ceremonyId)
        {
            var key = ChallengeKey(ceremonyId);
            if (!_cache.TryGetValue(key, out string? optionsJson))
            {
                return null;
            }

            // Single use: a challenge that survived its ceremony is a replay waiting to happen.
            _cache.Remove(key);
            return optionsJson;
        }

        public bool IsLockedOut(int userId) => _cache.TryGetValue(LockoutKey(userId), out _);

        public void RecordFailure(int userId)
        {
            var key = FailureKey(userId);
            var count = _cache.TryGetValue(key, out int existing) ? existing + 1 : 1;
            _cache.Set(key, count, FailureWindow);

            if (count >= MaxFailuresBeforeLockout)
            {
                _cache.Set(LockoutKey(userId), true, LockoutDuration);
                _cache.Remove(key);
            }
        }

        public void ClearFailures(int userId)
        {
            _cache.Remove(FailureKey(userId));
            _cache.Remove(LockoutKey(userId));
        }

        private static string ChallengeKey(string ceremonyId) => $"webauthn_ceremony:{ceremonyId}";
        private static string FailureKey(int userId) => $"webauthn_failures:{userId}";
        private static string LockoutKey(int userId) => $"webauthn_lockout:{userId}";
    }
}
