using System;

namespace SPP.Communication.Models
{
    public class Command
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string WorkerNode { get; set; }
    }
}
