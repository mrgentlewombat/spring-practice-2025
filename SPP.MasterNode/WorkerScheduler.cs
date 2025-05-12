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
        //we wait a bit, so the worker will be for sure available
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine("CentralApp: Scheduler Tick...");

            try
            {
                //check if worker node still active
                var pingResponse = await _communication.PostAsync<CommandResponse>(
                    $"{_workerBaseUrl}/api/command",
                    new CommandRequest { Command = "ping" }
                );
                Console.WriteLine($"Worker Ping Response: {pingResponse?.Message}");

                //check the exact status of the worker node
                var statusResponse = await _communication.GetAsync<StatusResponse>(
                    $"{_workerBaseUrl}/api/status"
                );
                Console.WriteLine($"Worker Status: {statusResponse?.Status}, Progress: {statusResponse?.Progress}%");

                //launch periodic work process on the worker node
                if (statusResponse?.Status == "Idle")
                {
                    var workResponse = await _communication.PostAsync<CommandResponse>(
                        $"{_workerBaseUrl}/api/command",
                        new CommandRequest 
                        { 
                            Command = "start",
                            Data = new { timestamp = DateTime.UtcNow }
                        }
                    );
                    Console.WriteLine($"Work Process Response: {workResponse?.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to communicate with worker node: {ex.Message}");
            }

            // Wait before next check
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
