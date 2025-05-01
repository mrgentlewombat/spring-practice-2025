<<<<<<< Updated upstream
using System;
using System.Threading.Tasks;
using Communication.Contracts;
using Communication.Models;
using Communication.Services;

namespace CentralApp
{

    /// entry point for the Central Application
    /// this application demonstrates how to use the communication layer to send commands to a Worker Node

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("CentralApp - Starting...");


            ICommunication communication = new Communication.Services.Communication();

            // URL of the Worker Node to communicate with
            string workerUrl = "http://127.0.0.1:5001";

            // display available commands
            Console.WriteLine("CentralApp running. Commands available:");
            Console.WriteLine("1 - Send ping command");
            Console.WriteLine("2 - Start work");
            Console.WriteLine("3 - Stop work");
            Console.WriteLine("4 - Get status");
            Console.WriteLine("5 - Exit");

            // main command loop
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
                            // send a ping command using POST
                            var pingRequest = new CommandRequest { Command = "ping" };
                            var pingResult = await communication.PostAsync<CommandResponse>(
                                $"{workerUrl}/api/command", pingRequest);
                            DisplayResult("Ping", pingResult);
                            break;

                        case "2":
                            // start a work operation using POST
                            var workData = new { JobId = Guid.NewGuid(), Timestamp = DateTime.Now };
                            var startRequest = new CommandRequest { Command = "start", Data = workData };
                            var startResult = await communication.PostAsync<CommandResponse>(
                                $"{workerUrl}/api/command", startRequest);
                            DisplayResult("Start Work", startResult);
                            break;

                        case "3":
                            // stop the current work operation using POST
                            var stopRequest = new CommandRequest { Command = "stop" };
                            var stopResult = await communication.PostAsync<CommandResponse>(
                                $"{workerUrl}/api/command", stopRequest);
                            DisplayResult("Stop Work", stopResult);
                            break;

                        case "4":
                            // get status using GET
                            var statusResult = await communication.GetAsync<StatusResponse>(
                                $"{workerUrl}/api/status");
                            Console.WriteLine($"Status: {(statusResult.Success ? "OK" : "ERROR")}");
                            Console.WriteLine($"Message: {statusResult.Message}");
                            Console.WriteLine($"Current state: {statusResult.Status}");
                            Console.WriteLine($"Progress: {statusResult.Progress}%");
                            break;

                        case "5":
                            // exit the application
                            running = false;
                            break;

                        default:
                            Console.WriteLine("Unknown command. Please try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    // handle any errors that occur during communication
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            Console.WriteLine("CentralApp shutting down...");
        }


        /// displays the result of a command operation

        static void DisplayResult(string operation, CommandResponse result)
        {
            Console.WriteLine($"{operation}: {(result.Success ? "SUCCESS" : "FAILED")}");
            Console.WriteLine($"Message: {result.Message}");
            if (!string.IsNullOrEmpty(result.Result))
            {
                Console.WriteLine($"Result: {result.Result}");
            }
        }
    }
}
=======
﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<WorkerScheduler>(); 

var app = builder.Build();
app.Run();
>>>>>>> Stashed changes
