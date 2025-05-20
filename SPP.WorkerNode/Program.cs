using System;
using System.Threading;
using System.Threading.Tasks;
using SPP.WorkerNode.Communication;
using SPP.WorkerNode.Services;

namespace SPP.WorkerNode
{
    /// <summary>
    /// Entry point for the Worker Node application.
    /// This is a console application that can receive HTTP requests from CentralApp.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Worker Node - Starting...");            try
            {
                var builder = WebApplication.CreateBuilder(args);
                builder.Logging.ClearProviders();
                builder.Logging.AddConsole();

                // Get configuration from environment or use defaults
                var baseUrl = Environment.GetEnvironmentVariable("WORKER_BASE_URL") ?? "localhost";
                var port = int.TryParse(Environment.GetEnvironmentVariable("WORKER_PORT"), out var p) ? p : 5001;
                
                builder.Services.AddSingleton<CommandProcessor>();
                var app = builder.Build();
                
                var logger = app.Services.GetRequiredService<ILogger<CommandListener>>();
                var processor = app.Services.GetRequiredService<CommandProcessor>();

                Console.WriteLine($"Worker Node listening on: http://{baseUrl}:{port}/api/");
                
                // Create and start the HTTP listener with dependency injection
                using var commandListener = new CommandListener(baseUrl, port, processor, logger);
                commandListener.Start();
                
                Console.WriteLine("Worker Node is running and actively listening for commands.");
                Console.WriteLine("Press Ctrl+C to stop the server.");
                
                // Use ManualResetEvent to keep the application running indefinitely
                var waitHandle = new ManualResetEvent(false);
                
                // Handle Ctrl+C to gracefully shut down
                Console.CancelKeyPress += (sender, e) => {
                    e.Cancel = true; // Prevent immediate termination
                    Console.WriteLine("Shutting down...");
                    commandListener.Stop();
                    waitHandle.Set(); // Signal to allow application to exit
                };
                
                // Wait until signal to terminate
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
