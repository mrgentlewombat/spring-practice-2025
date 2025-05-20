using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Communication.Services;
using Communication.Models;

using Communication.Contracts;

public class WorkerScheduler : BackgroundService
{
    private readonly ICommunication _communication;
    private readonly string _workerBaseUrl;

    public WorkerScheduler(ICommunication communication, string workerBaseUrl)
    {
        _communication = communication;
        _workerBaseUrl = workerBaseUrl;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

    while (!stoppingToken.IsCancellationRequested)
    {
        Console.WriteLine("CentralApp: Scheduler Tick...");

        try
        {
            var command = new UnifiedCommand
            {
                Type = "startprocessing",
                Payload = "read-file"
            };

            var response = await _communication.PostAsync<UnifiedCommandResponse>(
                _workerBaseUrl, // ex: http://localhost:5001/api/agent/process
                command
            );

            Console.WriteLine($"Worker response: Success={response?.Success}, Status={response?.Status}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to communicate with worker node: {ex.Message}");
        }

        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
    }
}

}
