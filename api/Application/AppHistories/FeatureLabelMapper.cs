namespace Application.AppHistories
{
    public static class FeatureLabelMapper
    {
        private static readonly Dictionary<string, string> _map = new(StringComparer.OrdinalIgnoreCase)
        {
            { "/api/user", "Quản lý User" },
            { "/api/departments", "Departments" },
            { "/api/positions", "Positions" },
            { "/api/roles", "Roles" },
            { "/api/appconfigs", "App Configs" },
            { "/api/appHistories", "System Logs" },
            { "/api/seafood", "Seafood CRM" },
            { "/api/menu-layout", "Menu" },
        };

        public static string GetLabel(string path)
        {
            if (string.IsNullOrEmpty(path)) return "Unknown";
            foreach (var kv in _map)
                if (path.StartsWith(kv.Key, StringComparison.OrdinalIgnoreCase))
                    return kv.Value;
            var parts = path.TrimStart('/').Split('/');
            return parts.Length >= 2 ? $"{parts[0]}/{parts[1]}" : path;
        }
    }
}
