using System;
using System.Collections.Generic;
using SPP.Communication.Models;


namespace SPP.MasterNode.Services.CommandRegistry
{
    public interface ICommandRegistry
    {
        void Add(Command command);
        Command? Get(Guid id);
        IEnumerable<Command> GetAllByStatus(string status);
        void Update(Command updatedCommand);
    }
}
