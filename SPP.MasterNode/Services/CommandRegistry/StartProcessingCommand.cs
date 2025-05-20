using System;
using System.Threading.Tasks;
using Communication.Http;

namespace SPP.MasterNode.Services.CommandRegistry
{
    public class StartProcessingCommand : ICommand
    {
        private readonly HttpRequestHelper _httpHelper;

        public StartProcessingCommand(HttpRequestHelper httpHelper)
        {
            _httpHelper = httpHelper;
        }

        public async Task ExecuteAsync(Guid commandId)
        {
            await _httpHelper.PostAsync<object>($"worker/start/{commandId}", null);
        }
    }
}
