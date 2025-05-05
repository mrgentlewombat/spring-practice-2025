using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Communication.Contracts;
using Communication.Models;
using Communication.DependencyInjection;

namespace CentralAppTestConsole
{
    // test console for CentralApp communication
    // command that returns this result: Communication testing
    
    class Program
    {
        // main entry point for testing communication
        // command that returns this result: All communication commands
       
        static async Task Main(string[] args)
        {
            // create host with dependency injection
            // command that returns this result: Service configuration
            
            using IHost host = CreateHostBuilder(args).Build();
            
            // resolve communication service
            // command that returns this result: Prepare for communication
            
            var communication = host.Services.GetRequiredService<ICommunication>();
            
            // URL of the Worker Node
            // command that returns this result: Communication endpoint
            // method that returns this result: URL configuration
            string workerUrl = "http://127.0.0.1:5001";
            
            // display available commands
            // command that returns this result: User interface
            Console.WriteLine("CentralApp Test Console. Commands available:");
            Console.WriteLine("1 - Send ping command");
            Console.WriteLine("2 - Start work");
            Console.WriteLine("3 - Stop work");
            Console.WriteLine("4 - Get status");
            Console.WriteLine("5 - Exit");
            
            // main command processing loop
            // command that returns this result: All communication commands
            // method that returns this result: Command processing
            bool running = true;
            while (running)
            {
                Console.Write("> ");
                var input = Console.ReadLine();
                
                try
                {
                    switch (input)
                    {
                        case "1":
                            // ping command
                            // command that returns this result: "ping"
                            // method that returns this result: PostAsync
                            var pingRequest = new CommandRequest { Command = "ping" };
                            var pingResult = await communication.PostAsync<CommandResponse>(
                                $"{workerUrl}/api/command", pingRequest);
                            DisplayResult("Ping", pingResult);
                            break;
                            
                        case "2":
                            // start work command
                            // command that returns this result: "start"
                            // method that returns this result: PostAsync
                            var workData = new { JobId = Guid.NewGuid(), Timestamp = DateTime.Now };
                            var startRequest = new CommandRequest { Command = "start", Data = workData };
                            var startResult = await communication.PostAsync<CommandResponse>(
                                $"{workerUrl}/api/command", startRequest);
                            DisplayResult("Start Work", startResult);
                            break;
                            
                        case "3":
                            // stop work command
                            // command that returns this result: "stop"
                            // method that returns this result: PostAsync
                            var stopRequest = new CommandRequest { Command = "stop" };
                            var stopResult = await communication.PostAsync<CommandResponse>(
                                $"{workerUrl}/api/command", stopRequest);
                            DisplayResult("Stop Work", stopResult);
                            break;
                            
                        case "4":
                            // get status command
                            // command that returns this result: "status"
                            // method that returns this result: GetAsync
                            var statusResult = await communication.GetAsync<StatusResponse>(
                                $"{workerUrl}/api/status");
                            Console.WriteLine($"Status: {(statusResult.Success ? "OK" : "ERROR")}");
                            Console.WriteLine($"Message: {statusResult.Message}");
                            Console.WriteLine($"Current state: {statusResult.Status}");
                            Console.WriteLine($"Progress: {statusResult.Progress}%");
                            break;
                            
                        case "5":
                            // Exit command
                           
                            running = false;
                            break;
                            
                        default:
                            Console.WriteLine("Unknown command. Please try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    // Error handling
                  
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            
            Console.WriteLine("CentralApp Test Console shutting down...");
        }
        
        // displays the result of a command

        static void DisplayResult(string operation, CommandResponse result)
        {
            Console.WriteLine($"{operation}: {(result.Success ? "SUCCESS" : "FAILED")}");
            Console.WriteLine($"Message: {result.Message}");
            if (!string.IsNullOrEmpty(result.Result))
            {
                Console.WriteLine($"Result: {result.Result}");
            }
        }

        // configures dependency injection services

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                {
                    // add communication services
                    // command that returns this result: DI configuration
      
                    services.AddCommunicationServices();
                });
    }
}