using System;
using System.Collections.Generic;

namespace SPP.MasterNode.Services.CommandRegistry
{
    public class CommandRegistry : ICommandRegistry
    {
        private readonly Dictionary<string, ICommand> _commands = new();
        private readonly Dictionary<Guid, string> _statuses = new();

        public void Register(string commandName, ICommand command)
        {
            _commands[commandName] = command;
        }

        public ICommand? GetCommand(string commandName)
        {
            return _commands.TryGetValue(commandName, out var command) ? command : null;
        }

        public void SetCompleted(Guid commandId)
        {
            _statuses[commandId] = "Completed";
            Console.WriteLine($"Command {commandId} marked as Completed.");
        }

        public void SetFailed(Guid commandId, string reason)
        {
            _statuses[commandId] = $"Failed: {reason}";
            Console.WriteLine($"Command {commandId} marked as Failed. Reason: {reason}");
        }

        public string? GetStatus(Guid commandId)
        {
            return _statuses.TryGetValue(commandId, out var status) ? status : null;
        }
    }
}
