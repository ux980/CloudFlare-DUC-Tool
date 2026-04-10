namespace CF_DUC_Tool.src.Core
{

    public class DnsRecordConfig
    {
        public string ProfileName { get; set; } = "Unknown";
        public string ApiToken { get; set; } = "";
        public string ZoneId { get; set; } = "";
        public string RecordName { get; set; } = "";
        public string RecordType { get; set; } = "A"; // A ou AAAA
        public int Ttl { get; set; } = 1;
        public bool Proxied { get; set; } = false;

        public bool IsValid() =>
            !string.IsNullOrWhiteSpace(ApiToken) &&
            !string.IsNullOrWhiteSpace(ZoneId) &&
            !string.IsNullOrWhiteSpace(RecordName);
    }

    public class AppConfig
    {
        public int IntervalMinutes { get; set; } = 5;
        public List<DnsRecordConfig> Records { get; set; } = new();
    }
}