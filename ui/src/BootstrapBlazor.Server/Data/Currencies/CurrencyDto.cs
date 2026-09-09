namespace BootstrapBlazor.Server.Data
{
    /// <summary>
    /// Represents a Data Transfer Object for a currency in the UI project.
    /// </summary>
    public class CurrencyDto
    {
        /// <summary>
        /// Unique identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Currency code (required).
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Currency name (required).
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Currency symbol (optional).
        /// </summary>
        public string? Symbol { get; set; }

        /// <summary>
        /// Whether the currency is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Whether this is the default currency.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Default exchange rate (required).
        /// </summary>
        public double DefaultExchangeRate { get; set; }
    }

    /// <summary>
    /// DTO for creating a new currency - matches API specification
    /// </summary>
    public class CreateCurrencyDto
    {
        /// <summary>
        /// Currency code (required, max 10 chars).
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Currency name (required, max 100 chars).
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Currency symbol (optional, max 10 chars).
        /// </summary>
        public string? Symbol { get; set; }

        /// <summary>
        /// Whether the currency is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Whether this is the default currency.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Default exchange rate (required, min 0.0001).
        /// </summary>
        public double DefaultExchangeRate { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing currency - matches API specification
    /// </summary>
    public class UpdateCurrencyDto
    {
        /// <summary>
        /// Unique identifier of the currency to update (required).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Currency code (required, max 10 chars).
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Currency name (required, max 100 chars).
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Currency symbol (optional, max 10 chars).
        /// </summary>
        public string? Symbol { get; set; }

        /// <summary>
        /// Whether the currency is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Whether this is the default currency.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Default exchange rate (required, min 0.0001).
        /// </summary>
        public double DefaultExchangeRate { get; set; }
    }
}