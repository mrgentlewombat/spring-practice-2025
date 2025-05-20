using System;
using System.Threading.Tasks;

namespace SPP.MasterNode.Services.CommandRegistry
{
    public interface ICommand
    {
        Task ExecuteAsync(Guid commandId);
    }
}
