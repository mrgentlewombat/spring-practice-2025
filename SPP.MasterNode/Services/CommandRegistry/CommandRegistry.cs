using System;
using System.Collections.Generic;
using Communication.Models;

namespace SPP.MasterNode.Services.CommandRegistry
{
    public class CommandRegistry : ICommandRegistry
    {
        private readonly Dictionary<Guid, Command> _commands = new();

        // Add a new command to the registry
        public void Add(Command command)
        {
            _commands[command.ID] = command;
        }

        // Retrieve a command by its ID
        public Command? Get(Guid id)
        {
            return _commands.TryGetValue(id, out var command) ? command : null;
        }

        // Get all commands with a specific status
        public IEnumerable<Command> GetAllByStatus(string status)
        {
            foreach (var command in _commands.Values)
            {
                if (command.Status == status)
                    yield return command;
            }
        }

        // Update an existing command
        public void Update(Command updatedCommand)
        {
            if (_commands.ContainsKey(updatedCommand.ID))
            {
                _commands[updatedCommand.ID] = updatedCommand;
            }
        }
    }
}
