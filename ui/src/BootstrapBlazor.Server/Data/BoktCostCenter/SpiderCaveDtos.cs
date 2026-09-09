namespace BootstrapBlazor.Server.Data.BoktCostCenter;

public class SpiderCaveStatsDto
{
    public int TotalBoktWallets { get; set; }
    public int ScannedWallets { get; set; }
    public int PendingWallets { get; set; }
    public int ErrorWallets { get; set; }
    public int DiscoveredWallets { get; set; }
    public int TotalTx { get; set; }
    public int TronTx { get; set; }
    public int EvmTx { get; set; }
    public int Journeys { get; set; }
    public int TerminalEndpoints { get; set; }
    public int MultiHopJourneys { get; set; }
    public int Clusters { get; set; }
    public int ConvergenceTargets { get; set; }
}

public class SpiderCaveConvergenceDto
{
    public string TargetAddress { get; set; } = string.Empty;
    public string Chain { get; set; } = string.Empty;
    public int SourceCount { get; set; }
    public decimal TotalValue { get; set; }
    public string? TokenSymbol { get; set; }
    public int MaxHops { get; set; }
    public int RiskScore { get; set; }
    public string? SourceAddresses { get; set; }
    public DateTime? FirstTxDate { get; set; }
    public DateTime? LastTxDate { get; set; }
}

public class SpiderCaveJourneyDto
{
    public long Id { get; set; }
    public string SourceWallet { get; set; } = string.Empty;
    public string SourceChain { get; set; } = string.Empty;
    public string PathAddrs { get; set; } = string.Empty;
    public int Hops { get; set; }
    public string EndAddress { get; set; } = string.Empty;
    public string EndChain { get; set; } = string.Empty;
    public decimal TotalValue { get; set; }
    public string? TokenSymbol { get; set; }
    public bool IsTerminal { get; set; }
    public int BranchCount { get; set; }
}

public class SpiderCaveWalletDto
{
    public string Address { get; set; } = string.Empty;
    public string Chain { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string ScanStatus { get; set; } = string.Empty;
    public int TotalOutTx { get; set; }
    public bool IsConsolidationTarget { get; set; }
    public DateTime? LastScannedAt { get; set; }
}

public class SpiderCaveBoktInfoDto
{
    public string IdentityValue { get; set; } = string.Empty;
    public string IdentityType { get; set; } = string.Empty;
    public string? NhaCungCap { get; set; }
    public string? AccountNameHint { get; set; }
    public string? BankNameHint { get; set; }
    public string? PicCol { get; set; }
    public string? NguoiTao { get; set; }
    public int TicketCount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime? FirstDate { get; set; }
    public DateTime? LastDate { get; set; }
}

public class SpiderCavePicSummaryDto
{
    public string? PicCol { get; set; }
    public int WalletCount { get; set; }
    public int TicketCount { get; set; }
    public decimal TotalVnd { get; set; }
    public string? Nccs { get; set; }
    public List<string> Wallets { get; set; } = [];
}

public class SpiderCaveFilterDto
{
    public string? Chain { get; set; }
    public string? Address { get; set; }
    public int? MinSources { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; } = 50;
}

public class WalletRiskInvestigationCreateRequest
{
    public string StartAddress { get; set; } = string.Empty;
    public string Chain { get; set; } = "ethereum";
}

public class WalletRiskInvestigationDto
{
    public string InvestigationId { get; set; } = string.Empty;
    public string StartAddress { get; set; } = string.Empty;
    public string Chain { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public int ProgressPercent { get; set; }
    public string CurrentStep { get; set; } = string.Empty;
    public int? RiskScore { get; set; }
    public string RiskLevel { get; set; } = "UNKNOWN";
    public string CoverageStatus { get; set; } = "partial";
    public string? Summary { get; set; }
    // Structured twin of Summary. Null for investigations stored before this field existed.
    public WalletRiskReportDto? Report { get; set; }
    public WalletRiskMetricsDto Metrics { get; set; } = new();
    public List<WalletRiskScoreReasonDto> ScoreReasons { get; set; } = [];
    public List<WalletRiskGraphNodeDto> Nodes { get; set; } = [];
    public List<WalletRiskGraphEdgeDto> Edges { get; set; } = [];
    public List<WalletRiskEvidenceDto> Evidence { get; set; } = [];
    public List<WalletRiskWarningDto> Warnings { get; set; } = [];
    public List<WalletRiskProviderStatusDto> ProviderStatuses { get; set; } = [];
    public List<SpiderCaveBoktInfoDto> BoktMatches { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

// Narrative parts of the report. Metrics/state/coverage are rendered from
// WalletRiskInvestigationDto directly so each number has exactly one source.
public class WalletRiskReportDto
{
    public DateTime ReportDate { get; set; }
    public List<WalletRiskReportFlowDto> Flows { get; set; } = [];
    public string Assessment { get; set; } = string.Empty;
    public string? Disclaimer { get; set; }
    public List<string> Recommendations { get; set; } = [];
    public List<string> SourceLinks { get; set; } = [];
}

public class WalletRiskReportFlowDto
{
    public DateTime Timestamp { get; set; }
    public bool Incoming { get; set; }
    public decimal Amount { get; set; }
    public string TokenSymbol { get; set; } = string.Empty;
    public string Counterparty { get; set; } = string.Empty;
    public string TxHash { get; set; } = string.Empty;
}

public class WalletRiskMetricsDto
{
    public int TransactionCount { get; set; }
    public decimal TotalInboundValue { get; set; }
    public decimal TotalOutboundValue { get; set; }
    public decimal? MedianHoldMinutes { get; set; }
    public decimal ForwardedRatio { get; set; }
    public decimal OutputConcentration { get; set; }
    public int InboundSourceCount { get; set; }
    public int OutboundDestinationCount { get; set; }
    public int MatchedSweepCycles { get; set; }
    public int FakeTokenTransferCount { get; set; }
    public bool HasExchangeCounterparty { get; set; }
}

public class WalletRiskScoreReasonDto
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Points { get; set; }
    public string Tier { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> SupportingTxHashes { get; set; } = [];
}

public class WalletRiskGraphNodeDto
{
    public string Id { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Chain { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Hop { get; set; }
    public string Direction { get; set; } = string.Empty;
}

public class WalletRiskGraphEdgeDto
{
    public string Id { get; set; } = string.Empty;
    public string SourceId { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public string? TxHash { get; set; }
    public decimal Amount { get; set; }
    public string TokenSymbol { get; set; } = string.Empty;
    public string Tier { get; set; } = string.Empty;
}

public class WalletRiskEvidenceDto
{
    public string TxHash { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string TokenContract { get; set; } = string.Empty;
    public string TokenSymbol { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string FromAddress { get; set; } = string.Empty;
    public string ToAddress { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public string Tier { get; set; } = string.Empty;
}

public class WalletRiskWarningDto
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> SupportingTxHashes { get; set; } = [];
}

public class WalletRiskProviderStatusDto
{
    public string Provider { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool Truncated { get; set; }
    public string? Message { get; set; }
}

public class SpiderCaveWalletTicketDto
{
    public long RawId { get; set; }
    public string? IdPhieu { get; set; }
    public DateTime? NgayKt { get; set; }
    public string? SheetName { get; set; }
    public int? SourceRow { get; set; }
    public string? NguoiTao { get; set; }
    public string? NhaCungCap { get; set; }
    public string? PicCol { get; set; }
    public string? IdentityType { get; set; }
    public string? NetworkHint { get; set; }
    public string? BankNameHint { get; set; }
    public string? AccountNameHint { get; set; }
    public string? ContextText { get; set; }
    public decimal? AmountValue { get; set; }
    public string? AmountCurrency { get; set; }
    public int? ConfidenceScore { get; set; }
    public string? ParseStatus { get; set; }
}

// Real on-chain transaction scanned from blockchain explorer API (Tronscan/Etherscan) —
// value_display is the true amount, independent of the LLM-parsed ticket amount which can
// be wrong (e.g. picks up an exchange rate instead of the real transferred amount).
public class SpiderCaveWalletTxDto
{
    public string TxHash { get; set; } = string.Empty;
    public string Chain { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string ToAddress { get; set; } = string.Empty;
    public decimal ValueDisplay { get; set; }
    public string? TokenSymbol { get; set; }
    public DateTime BlockTimestamp { get; set; }
    public string Direction { get; set; } = string.Empty;
}

// One row in the executive fraud-risk watchlist — a wallet/bank-account identity ranked by
// combined financial + collusion signal.
public class SpiderCaveWatchlistDto
{
    public string IdentityType { get; set; } = string.Empty;
    public string IdentityValue { get; set; } = string.Empty;
    public int PicCount { get; set; }
    public List<string> PicList { get; set; } = [];
    public int SupplierCount { get; set; }
    public int CreatorCount { get; set; }
    public int TicketCount { get; set; }
    public int OnchainTxCount { get; set; }
    public decimal? TicketToTxRatio { get; set; }
    public string? DominantExpenseType { get; set; }
    public Dictionary<string, int> ExpenseTypeBreakdown { get; set; } = [];
    public decimal TotalVnd { get; set; }
    public decimal TotalUsdt { get; set; }
    public decimal TotalUsd { get; set; }
    public decimal OnchainTotalValue { get; set; }
    public string? OnchainTokenSymbol { get; set; }
    public int MissingCurrencyTickets { get; set; }
    public int PartialTickets { get; set; }
    public int RiskScore { get; set; }
    public string? RiskFlags { get; set; }
    public DateTime? FirstDate { get; set; }
    public DateTime? LastDate { get; set; }
}

public class SpiderCaveWatchlistStatsDto
{
    public int TotalIdentities { get; set; }
    public int HighRiskCount { get; set; }
    public int CollusionSuspectCount { get; set; }
    public decimal TotalExposureUsdt { get; set; }
    public decimal TotalExposureVnd { get; set; }
    public int MissingCurrencyTickets { get; set; }
    public DateTime? SnapshotRefreshedAt { get; set; }
}