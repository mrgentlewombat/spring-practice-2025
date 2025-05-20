namespace SPP.MasterNode.Models
{
    public class UnifiedCommand
    {
        public string CommandId { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; }
        public object Payload { get; set; }
    }
}
