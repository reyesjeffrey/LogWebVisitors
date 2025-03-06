using Newtonsoft.Json;
using System;

namespace VisitorTracker.Models
{
    public class VisitorLog
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString(); // Generate unique ID

        [JsonProperty("date")]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [JsonProperty("pageVisited")]
        public string PageVisited { get; set; } = string.Empty;

        [JsonProperty("ipAddress")]
        public string IpAddress { get; set; } = string.Empty;

        [JsonProperty("device")]
        public string Device { get; set; } = string.Empty;

        [JsonProperty("browser")]
        public string Browser { get; set; } = string.Empty;

        [JsonProperty("referrer")]
        public string Referrer { get; set; } = string.Empty;
    }
}
