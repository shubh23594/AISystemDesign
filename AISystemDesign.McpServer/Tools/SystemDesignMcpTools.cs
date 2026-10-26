
using System.ComponentModel;
using AISystemDesign.Core;
using ModelContextProtocol.Server;

namespace AISystemDesign.McpServer.Tools
{
    [McpServerToolType]
    public class SystemDesignMcpTools
    {
        [McpServerTool]
        [Description("Calculate average and peak API requests per second.")]
        public static string CalculateSystemCapacity(
            long dailyActiveUsers,
            int requestsPerUserPerDay,
            double peakTrafficMultiplier)
        {
            return SystemDesignTools.CalculateSystemCapacity(
                dailyActiveUsers,
                requestsPerUserPerDay,
                peakTrafficMultiplier);
        }

        [McpServerTool]
        [Description("Estimate storage requirements and replication.")]
        public static string EstimateStorageRequirements(
            long eventsPerDay,
            double averageEventSizeKB,
            int retentionDays,
            int replicationFactor)
        {
            return SystemDesignTools.EstimateStorageRequirements(
                eventsPerDay,
                averageEventSizeKB,
                retentionDays,
                replicationFactor);
        }

        [McpServerTool]
        [Description("Validate a proposed software architecture.")]
        public static string ValidateArchitecture(
            string architectureDescription)
        {
            return SystemDesignTools.ValidateArchitecture(
                architectureDescription);
        }
    }
}
