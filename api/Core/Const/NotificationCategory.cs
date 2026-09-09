namespace Core.Const
{
    /// <summary>
    /// Notification grouping slugs stored in user_notification.category.
    /// Slugs only — the Vietnamese labels live in the UI so a wording change never needs an API deploy.
    /// </summary>
    public static class NotificationCategory
    {
        /// <summary>Daily domain sync digest pushed by n8n.</summary>
        public const string DomainSync = "domain-sync";

        /// <summary>Pipeline watchdog / sync alerts.</summary>
        public const string PipelineAlert = "pipeline-alert";

        /// <summary>Task workboard events.</summary>
        public const string WorkItem = "work-item";

        /// <summary>Announcements composed by a real sender.</summary>
        public const string Announcement = "announcement";

        /// <summary>Fallback for legacy rows and unrecognised input.</summary>
        public const string General = "general";

        public const int MaxLength = 30;

        private static readonly HashSet<string> Known = new(StringComparer.OrdinalIgnoreCase)
        {
            DomainSync,
            PipelineAlert,
            WorkItem,
            Announcement,
            General
        };

        public static bool IsKnown(string? value) =>
            !string.IsNullOrWhiteSpace(value) && Known.Contains(value.Trim());

        /// <summary>Lower-cases and validates a caller-supplied category, falling back to <see cref="General"/>.</summary>
        public static string Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return General;

            var candidate = value.Trim().ToLowerInvariant();
            return Known.Contains(candidate) ? candidate : General;
        }
    }
}
