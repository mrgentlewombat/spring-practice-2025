using System;

namespace SPP.MasterNode.Services.CommandRegistry
{
    public interface ICommandRegistry
    {
        void Register(string commandName, ICommand command);
        ICommand? GetCommand(string commandName);
    }
}
