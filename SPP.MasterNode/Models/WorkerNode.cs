using System;

namespace SPP.MasterNode.Models
{
    public class WorkerNode
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required string Url { get; set; }
        public DateTime LastHeartbeat { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    }
}