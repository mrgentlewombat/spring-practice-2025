using System;
using System.Threading.Tasks;
using Communication.Http;

namespace SPP.MasterNode.Services.CommandRegistry
{
    public class StatusCommand : ICommand
    {
        private readonly HttpRequestHelper _httpHelper;

        public StatusCommand(HttpRequestHelper httpHelper)
        {
            _httpHelper = httpHelper;
        }

        public async Task ExecuteAsync(Guid commandId)
        {
            await _httpHelper.GetAsync<object>($"worker/status/{commandId}");
        }
    }
}
