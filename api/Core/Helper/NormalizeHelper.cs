namespace Core.Helper
{
    /// <summary>
    /// Helper class for normalizing strings (lowercase, remove spaces)
    /// </summary>
    public static class NormalizeHelper
    {
        /// <summary>
        /// Normalize string: lowercase and remove all spaces
        /// Example: "A Pen" -> "apen"
        /// </summary>
        public static string Normalize(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            return input.ToLower().Replace(" ", "");
        }

        /// <summary>
        /// Normalize user name from LastName and FirstName
        /// Example: LastName="A", FirstName="Pen" -> "apen"
        /// </summary>
        public static string NormalizeUserName(string? lastName, string? firstName)
        {
            var fullName = $"{lastName ?? ""} {firstName ?? ""}".Trim();
            return Normalize(fullName);
        }
    }
}
