using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using SPP.WorkerNode.Communication;
using SPP.WorkerNode.Services;

namespace SPP.WorkerNode
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Worker Node - Starting...");

            try
            {
                var builder = WebApplication.CreateBuilder(args);
                builder.Logging.ClearProviders();
                builder.Logging.AddConsole();

                var baseUrl = Environment.GetEnvironmentVariable("WORKER_BASE_URL") ?? "localhost";
                var port = int.TryParse(Environment.GetEnvironmentVariable("WORKER_PORT"), out var p) ? p : 5001;

                builder.Services.AddSingleton<CommandProcessor>();
                var app = builder.Build();

                var logger = app.Services.GetRequiredService<ILogger<CommandListener>>();
                var processor = app.Services.GetRequiredService<CommandProcessor>();

                Console.WriteLine($"Worker Node listening on: http://{baseUrl}:{port}/api/");

                using var commandListener = new CommandListener(baseUrl, port, processor, logger);
                commandListener.Start();

                Console.WriteLine("Worker Node is running and actively listening for commands.");

                // 🔄 Autoregister to Master Node
                try
                {
                    using var client = new HttpClient();
                    var registerPayload = new
                    {
                        url = $"http://{baseUrl}:{port}/api/agent/process"
                    };

                    var masterUrl = "http://localhost:5000/api/worker/register";
                    var response = client.PostAsJsonAsync(masterUrl, registerPayload).Result;

                    if (response.IsSuccessStatusCode)
                        Console.WriteLine("✅ Successfully registered with Master Node.");
                    else
                        Console.WriteLine($"❌ Failed to register with Master Node. Status: {response.StatusCode}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error registering with Master Node: {ex.Message}");
                }

                Console.WriteLine("Press Ctrl+C to stop the server.");

                var waitHandle = new ManualResetEvent(false);

                Console.CancelKeyPress += (sender, e) => {
                    e.Cancel = true;
                    Console.WriteLine("Shutting down...");
                    commandListener.Stop();
                    waitHandle.Set();
                };

                waitHandle.WaitOne();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine("Worker Node shutting down...");
        }
    }
}
