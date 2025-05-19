using System;

namespace SPP.Domain.Entities
{
    public class AgentEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CodeName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string Contact { get; set; } = string.Empty;
        public string LastMission { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ClearenceLevel { get; set; } = string.Empty;
        public string ArchiveNote { get; set; } = string.Empty;
        
        public string RegionOfReport { get; set; } = string.Empty;
        public DateOnly DateOfReport { get; set; }

    }
}