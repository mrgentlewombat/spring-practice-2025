using System;
using System.Threading;
using System.Threading.Tasks;
using WorkerNodeApp.Communication;
using WorkerNodeApp.Services;

namespace WorkerNodeApp
{
    /// <summary>
    /// Entry point for the Worker Node application.
    /// This is a console application that can receive HTTP requests from CentralApp.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Worker Node - Starting...");

            try
            {
                // Get configuration from command line args or use defaults
                string ipAddress = "127.0.0.1";
                int port = args.Length > 0 ? int.Parse(args[0]) : 5001;
                
                Console.WriteLine($"Worker Node listening on: http://{ipAddress}:{port}/");
                
                // Create command processor to handle incoming commands
                var commandProcessor = new CommandProcessor();
                
                // Create and start the HTTP listener
                using var commandListener = new CommandListener(ipAddress, port, commandProcessor);
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