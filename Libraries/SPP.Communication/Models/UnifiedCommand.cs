using System.Text.Json.Serialization;

namespace Communication.Models
{
    public class UnifiedCommand
    {
        public string Type { get; set; }
        public object Payload { get; set; }
        [JsonPropertyName("id")]
        public string CommandId { get; set; }
    }

    public class UnifiedCommandResponse
    {
        public bool Success { get; set; }
        public string Status { get; set; }
        public object Result { get; set; }
        public double Progress { get; set; }
        [JsonPropertyName("id")]
        public string CommandId { get; set; }
    }
}
