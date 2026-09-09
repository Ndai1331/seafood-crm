using BootstrapBlazor.Components;

namespace BootstrapBlazor.Server.Data.SeoRequests;

public static class SeoRequestPartnerPaymentMethods
{
    public const string Bank = "BANK";
    public const string Crypto = "CRYPTO";

    public static bool IsBank(string? value) => Normalize(value) == Bank;
    public static bool IsCrypto(string? value) => Normalize(value) == Crypto;

    /// <summary>True when a ticket carries structured details rather than the legacy text block.</summary>
    public static bool HasStructuredMethod(string? value) => Normalize(value) is Bank or Crypto;

    public static string GetLabel(string? value) => Normalize(value) switch
    {
        Bank => "Chuyển khoản ngân hàng",
        Crypto => "Crypto",
        _ => "-"
    };

    public static List<SelectedItem<string>> BuildOptions() =>
    [
        new(Bank, "Chuyển khoản ngân hàng"),
        new(Crypto, "Crypto")
    ];

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();
}

public static class SeoRequestCryptoNetworks
{
    public const string UsdtTrc20 = "USDT_TRC20";
    public const string UsdtBep20 = "USDT_BEP20";
    public const string UsdtErc20 = "USDT_ERC20";
    public const string UsdcErc20 = "USDC_ERC20";

    public static string GetLabel(string? value) => value?.Trim().ToUpperInvariant() switch
    {
        UsdtTrc20 => "USDT - TRC20",
        UsdtBep20 => "USDT - BEP20",
        UsdtErc20 => "USDT - ERC20",
        UsdcErc20 => "USDC - ERC20",
        _ => value ?? "-"
    };

    public static List<SelectedItem<string>> BuildOptions() =>
    [
        new(UsdtTrc20, "USDT - TRC20"),
        new(UsdtBep20, "USDT - BEP20"),
        new(UsdtErc20, "USDT - ERC20"),
        new(UsdcErc20, "USDC - ERC20")
    ];
}
