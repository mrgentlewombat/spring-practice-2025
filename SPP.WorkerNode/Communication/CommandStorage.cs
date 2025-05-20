using System.Collections.Concurrent;
using SPP.Communication.Models;

namespace SPP.WorkerNode.Communication
{
    public class CommandStorage
    {
        private readonly ConcurrentDictionary<string, CommandInfo> _storage = new();

        public class CommandInfo
        {
            public string CommandId { get; set; } = string.Empty;
            public CommandStatus Status { get; set; }
            public CancellationTokenSource CancellationTokenSource { get; set; } = new();
            public UnifiedCommand Command { get; set; } = null!;
        }

        public enum CommandStatus
        {
            Running,
            Completed,
            Cancelled,
            Failed
        }

        public bool AddCommand(UnifiedCommand command)
        {
            var info = new CommandInfo
            {
                CommandId = command.CommandId,
                Status = CommandStatus.Running,
                Command = command
            };
            return _storage.TryAdd(command.CommandId, info);
        }

        public bool UpdateStatus(string commandId, CommandStatus status)
        {
            if (_storage.TryGetValue(commandId, out var info))
            {
                info.Status = status;
                return true;
            }
            return false;
        }

        public bool TryGetCommand(string commandId, out CommandInfo? commandInfo)
        {
            return _storage.TryGetValue(commandId, out commandInfo);
        }

        public bool CancelCommand(string commandId)
        {
            if (_storage.TryGetValue(commandId, out var info))
            {
                info.CancellationTokenSource.Cancel();
                info.Status = CommandStatus.Cancelled;
                return true;
            }
            return false;
        }

        public IEnumerable<CommandInfo> GetActiveCommands()
        {
            return _storage.Values.Where(c => c.Status == CommandStatus.Running);
        }

        public void Clear()
        {
            foreach (var command in _storage.Values)
            {
                command.CancellationTokenSource.Cancel();
                command.CancellationTokenSource.Dispose();
            }
            _storage.Clear();
        }
    }
}
