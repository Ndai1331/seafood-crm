namespace WebApi.Filters
{
    /// <summary>So khoá bí mật. Dùng chung để không có hai cách so lệch nhau giữa các filter.</summary>
    internal static class ApiKeyComparer
    {
        /// <summary>Comparison whose duration does not depend on how many characters match.</summary>
        public static bool FixedTimeEquals(string a, string b)
        {
            if (a.Length != b.Length) return false;
            var diff = 0;
            for (var i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
