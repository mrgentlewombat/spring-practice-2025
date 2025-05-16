namespace SPP.DataProcessing.Models;
public class Agent
{
    public int AgentID { get; set; }
    public required string Name { get; set; }
    public required string CodeName { get; set; }
    public required string Location { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public required string Contact { get; set; }
    public required string LastMission { get; set; }
    public required string Status { get; set; }
    public required string ClearenceLevel { get; set; }
    public required string ArchiveNote { get; set; }
}