using System;
using SPP.Domain.Entities;
using SPP.Domain.Repositories;

namespace SPP.DataProcessing.Services
{
    public class AgentProcessingService
    {
        private readonly IRepository<AgentEntity> _repository;

        public AgentProcessingService(IRepository<AgentEntity> repository)
        {
            _repository = repository;
        }
        public async Task ProcessAndSaveAgentsAsync(List<Agent> agents, string filename)
        {
            var (region, dateOfReport) = ParseFileName(filename);

            var agentEntities = agents.Select(agent => new AgentEntity
            {
                Name = agent.Name,
                CodeName = agent.CodeName,
                Location = agent.Location,
                DateOfBirth = agent.DateOfBirth,
                Contact = agent.Contact,
                LastMission = agent.LastMission,
                Status = agent.Status,
                ClearenceLevel = agent.ClearenceLevel,
                ArchiveNote = agent.ArchiveNote,
                RegionOfReport = region,
                DateOfReport = dateOfReport
            }).ToList();

             foreach (var entity in agentEntities)
            {
                await _repository.AddAsync(entity);
            }

            await _repository.SaveAsync();

        }
        private (string region, DateOnly date) ParseFileName(string filename)
        {
            var name = Path.GetFileNameWithoutExtension(filename); 
            var parts = name.Split('_');

            if (parts.Length < 3)
                throw new FormatException("Invalid file name format");

            var region = parts[0];
            var month = int.Parse(parts[1]);
            var year = int.Parse(parts[2]);
            var date = new DateOnly(year, month, 1);
            return (region, date);
        }

    }
}