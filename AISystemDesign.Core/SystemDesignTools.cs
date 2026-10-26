using System.ComponentModel;
using System.Text.RegularExpressions;

namespace AISystemDesign.Core
{
    public static class SystemDesignTools
    {
        // ======================================
        // TOOL 1: SYSTEM CAPACITY CALCULATOR
        // ======================================

        [Description(
            "Calculate daily API requests, average " +
            "requests per second and peak requests " +
            "per second for a system.")]
        public static string CalculateSystemCapacity(
            [Description("Daily active users")]
            long dailyActiveUsers,

            [Description(
                "API requests made by each active user per day")]
            int requestsPerUserPerDay,

            [Description(
                "Peak traffic multiplier, e.g. 5 for 5x")]
            double peakTrafficMultiplier)
        {
            Console.WriteLine(
                "\n[TOOL CALLED] CalculateSystemCapacity");

            if (dailyActiveUsers <= 0 ||
                requestsPerUserPerDay <= 0 ||
                !double.IsFinite(peakTrafficMultiplier) ||
                peakTrafficMultiplier < 1 ||
                peakTrafficMultiplier > 1000)
            {
                return "Invalid system capacity inputs.";
            }

            decimal dailyRequests =
                (decimal)dailyActiveUsers *
                requestsPerUserPerDay;

            decimal averageRps =
                dailyRequests / 86400m;

            decimal peakRps =
                averageRps *
                (decimal)peakTrafficMultiplier;

            return $"""
                SYSTEM CAPACITY ESTIMATION

                Daily Requests: {dailyRequests:N0}
                Average RPS: {averageRps:F2}
                Peak RPS: {peakRps:F2}

                Assumptions:
                - Traffic averaged over 24 hours.
                - Peak RPS derived using the supplied multiplier.
                - Figures are estimates, not load-test results.
                """;
        }

        // ======================================
        // TOOL 2: STORAGE ESTIMATION
        // ======================================

        [Description(
            "Estimate daily storage, retained storage " +
            "and replicated storage for system events.")]
        public static string EstimateStorageRequirements(
            [Description(
                "Number of stored events per day")]
            long eventsPerDay,

            [Description(
                "Average event size in decimal kilobytes")]
            double averageEventSizeKB,

            [Description(
                "Data retention period in days")]
            int retentionDays,

            [Description(
                "Total number of full data copies, " +
                "including the primary copy")]
            int replicationFactor)
        {
            Console.WriteLine(
                "\n[TOOL CALLED] EstimateStorageRequirements");

            if (eventsPerDay <= 0 ||
                eventsPerDay > 1_000_000_000_000L ||
                !double.IsFinite(averageEventSizeKB) ||
                averageEventSizeKB <= 0 ||
                averageEventSizeKB > 1_000_000 ||
                retentionDays <= 0 ||
                retentionDays > 3650 ||
                replicationFactor < 1 ||
                replicationFactor > 20)
            {
                return "Invalid storage estimation inputs.";
            }

            // Decimal KB: 1 KB = 1000 bytes
            decimal dailyBytes =
                eventsPerDay *
                (decimal)averageEventSizeKB *
                1000m;

            decimal dailyStorageMB =
                dailyBytes / 1_000_000m;

            decimal retainedStorageGB =
                dailyBytes *
                retentionDays / 1_000_000_000m;

            decimal replicatedStorageGB =
                retainedStorageGB * replicationFactor;

            return $"""
                STORAGE ESTIMATION

                Events Per Day: {eventsPerDay:N0}
                Average Event Size: {averageEventSizeKB:F2} KB

                Daily Storage: {dailyStorageMB:F2} MB

                Retention: {retentionDays} days
                Storage Before Replication:
                {retainedStorageGB:F2} GB

                Replication Factor: {replicationFactor}
                Total Replicated Storage:
                {replicatedStorageGB:F2} GB

                Assumptions:
                - Decimal units: 1 KB = 1000 bytes.
                - Replication includes the primary copy.
                - Excludes indexes, backups, logs,
                  compression and metadata overhead.
                """;
        }

        // ======================================
        // TOOL 3: ARCHITECTURE VALIDATION
        // ======================================

        [Description(
            "Perform a preliminary rule-based review " +
            "of a proposed software architecture. " +
            "Identify specified, explicitly excluded, " +
            "and unspecified architecture safeguards.")]
        public static string ValidateArchitecture(
            [Description(
                "Exact proposed architecture description " +
                "provided by the user")]
            string architectureDescription)
        {
            Console.WriteLine(
                "\n[TOOL CALLED] ValidateArchitecture");

            if (string.IsNullOrWhiteSpace(
                architectureDescription))
            {
                return "No architecture was provided.";
            }

            var findings = new List<string>();

            // Always examine security and observability
            findings.Add(CheckControl(
                architectureDescription,
                "Authentication / Authorization",
                "authentication", "authorization",
                "JWT", "OAuth", "OIDC", "RBAC"));

            findings.Add(CheckControl(
                architectureDescription,
                "Observability",
                "logging", "logs", "metrics",
                "tracing", "monitoring", "observability"));

            // Booking-specific checks
            if (ContainsAny(
                architectureDescription,
                "booking", "reservation", "payment", "order"))
            {
                findings.Add(CheckControl(
                    architectureDescription,
                    "Booking Idempotency",
                    "idempotency", "idempotent",
                    "deduplication", "idempotency key"));

                findings.Add(CheckControl(
                    architectureDescription,
                    "Transaction / Consistency Strategy",
                    "transaction", "saga",
                    "outbox", "optimistic concurrency"));
            }

            // Caching-specific checks
            if (ContainsAny(
                architectureDescription,
                "Redis", "cache", "caching"))
            {
                findings.Add(CheckControl(
                    architectureDescription,
                    "Cache Expiration",
                    "TTL", "expiration", "expiry",
                    "time to live"));

                findings.Add(CheckControl(
                    architectureDescription,
                    "Cache Invalidation",
                    "invalidation", "invalidate",
                    "cache refresh"));
            }

            // Messaging-specific checks
            if (ContainsAny(
                architectureDescription,
                "Kafka", "RabbitMQ", "queue",
                "messaging", "message broker"))
            {
                findings.Add(CheckControl(
                    architectureDescription,
                    "Message Retry Strategy",
                    "retry", "retries", "backoff"));

                findings.Add(CheckControl(
                    architectureDescription,
                    "Dead Letter Handling",
                    "DLQ", "dead letter", "dead-letter"));

                findings.Add(CheckControl(
                    architectureDescription,
                    "Idempotent Consumers",
                    "idempotent consumer",
                    "consumer idempotency",
                    "message deduplication"));
            }

            return
                "ARCHITECTURE VALIDATION RESULTS\n\n" +
                string.Join("\n", findings) +
                "\n\nInterpretation:\n" +
                "- MENTIONED: Control appears in the description.\n" +
                "- EXPLICITLY MISSING: Text indicates exclusion.\n" +
                "- NOT SPECIFIED: Requires clarification.\n" +
                "\nThis is a basic text-based checklist, " +
                "not a verification of the running system.";
        }

        // ======================================
        // HELPER: DETECT ARCHITECTURE TOPICS
        // ======================================

        private static bool ContainsAny(
            string text,
            params string[] keywords)
        {
            return keywords.Any(keyword =>
                text.Contains(
                    keyword,
                    StringComparison.OrdinalIgnoreCase));
        }

        // ======================================
        // HELPER: REVIEW ONE ARCHITECTURE CONTROL
        // ======================================

        private static string CheckControl(
            string text,
            string controlName,
            params string[] keywords)
        {
            string pattern = @"\b(?:" +
                string.Join(
                    "|",
                    keywords.Select(Regex.Escape)) +
                @")\b";

            MatchCollection matches = Regex.Matches(
                text, pattern, RegexOptions.IgnoreCase);

            if (matches.Count == 0)
            {
                return $"[NOT SPECIFIED] {controlName}";
            }

            foreach (Match match in matches)
            {
                // Look for an explicit negation
                // immediately preceding the term.
                int start = Math.Max(0, match.Index - 60);

                string prefix = text.Substring(
                    start, match.Index - start);

                bool explicitlyMissing = Regex.IsMatch(
                    prefix,
                    @"\b(?:no|without|lack(?:s|ing)?|" +
                    @"not|missing|disabled)\b[^.;:\r\n]{0,45}$",
                    RegexOptions.IgnoreCase);

                if (explicitlyMissing)
                {
                    return
                        $"[EXPLICITLY MISSING] {controlName}";
                }
            }

            return $"[MENTIONED] {controlName}";
        }
    }

}
