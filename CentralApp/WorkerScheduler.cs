using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

public class WorkerScheduler : BackgroundService
{
    private readonly HttpClient _httpClient = new HttpClient();
    private const string WorkerBaseUrl = "http://worker-node-app:8080";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    //we wait a bit, so the worker will be for sure available
    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

    while (!stoppingToken.IsCancellationRequested)
    {
        Console.WriteLine("CentralApp: Scheduler Tick...");

        try
            {
                //check the exact status of the worker node
            var health = await _httpClient.GetStringAsync($"{WorkerBaseUrl}/health");
            Console.WriteLine($"Worker Health: {health}");
                //check if worker node still active
            var status = await _httpClient.GetStringAsync($"{WorkerBaseUrl}/status");
            Console.WriteLine($"Worker Status: {status}");
                //launch periodic work process on the worker node
            var response = await _httpClient.PostAsync($"{WorkerBaseUrl}/work", null);
            Console.WriteLine("Triggered work on worker node.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to reach worker node: {ex.Message}");
        }

        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
    }
}

}
