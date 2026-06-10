using System.Text.Json.Serialization;

namespace SmartProd_Mobile_Front_end.Models
{
    public class DashboardResponse
    {
        [JsonPropertyName("activeOps")]
        public int ActiveOps { get; set; }

        [JsonPropertyName("totalOps")]
        public int TotalOps { get; set; }

        [JsonPropertyName("efficiency")]
        public string Efficiency { get; set; } = "";

        [JsonPropertyName("efficiencyTrend")]
        public string EfficiencyTrend { get; set; } = "";

        [JsonPropertyName("throughput")]
        public string Throughput { get; set; } = "";

        [JsonPropertyName("oeeTarget")]
        public double OeeTarget { get; set; }

        [JsonPropertyName("alertsCount")]
        public int AlertsCount { get; set; }

        [JsonPropertyName("alertMessage")]
        public string AlertMessage { get; set; } = "";

        [JsonPropertyName("currentShift")]
        public string CurrentShift { get; set; } = "";

        [JsonPropertyName("shiftProgress")]
        public string ShiftProgress { get; set; } = "";

        [JsonPropertyName("lineEfficiency")]
        public string LineEfficiency { get; set; } = "";

        [JsonPropertyName("latency")]
        public string Latency { get; set; } = "";

        [JsonPropertyName("traceability")]
        public string Traceability { get; set; } = "";

        [JsonPropertyName("oeeMeta")]
        public string OeeMeta { get; set; } = "";

        [JsonPropertyName("oeeGap")]
        public string OeeGap { get; set; } = "";

        [JsonPropertyName("history")]
        public List<DashboardHistoryRecord>? History { get; set; }
    }

    public class DashboardHistoryRecord
    {
        [JsonPropertyName("date")]
        public string Date { get; set; } = "";

        [JsonPropertyName("throughput")]
        public string Throughput { get; set; } = "";

        [JsonPropertyName("status")]
        public string Status { get; set; } = "";
    }
}
