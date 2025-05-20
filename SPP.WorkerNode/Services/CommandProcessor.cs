using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SPP.Communication.Models;

namespace SPP.WorkerNode.Services
{

    /// processes commands received from the CentralApp
    /// handles different command types and maintains state information
    public class CommandProcessor
    {
        private readonly Dictionary<string, Func<object, Task<string>>> _commandHandlers;
        private string _currentStatus = "Idle";
        private double _progress = 0;
        private CancellationTokenSource _workCancellationSource;
        private readonly DateTime _startTime;

        public List<string> GetRegisteredCommands()
        {
            return new List<string>(_commandHandlers.Keys);
        }


        /// initializes a new instance of the CommandProcessor
        public CommandProcessor()
        {
            _startTime = DateTime.Now;

            // register command handlers for different command types
            _commandHandlers = new Dictionary<string, Func<object, Task<string>>>(StringComparer.OrdinalIgnoreCase)
            {
                ["start"] = StartWorkHandler,
                ["stop"] = StopWorkHandler,
                ["ping"] = PingHandler,
                ["hello"] = HelloHandler
            };
        }


        /// processes a command and returns the result>
        public async Task<CommandResponse> ProcessCommandAsync(string command, object data)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return new CommandResponse
                {
                    Success = false,
                    Message = "Command cannot be empty"
                };
            }

            // check if we have a handler for this command
            if (_commandHandlers.TryGetValue(command, out var handler))
            {
                try
                {

                    var result = await handler(data);
                    return new CommandResponse
                    {
                        Success = true,
                        Message = $"Command '{command}' executed successfully",
                        Result = result
                    };
                }
                catch (Exception ex)
                {
                    return new CommandResponse
                    {
                        Success = false,
                        Message = $"Error executing command: {ex.Message}"
                    };
                }
            }
            else
            {

                return new CommandResponse
                {
                    Success = false,
                    Message = $"Unknown command: {command}"
                };
            }
        }


        /// gets the current status of the worker node
        public StatusResponse GetStatus()
        {
            return new StatusResponse
            {
                Success = true,
                Message = "Status retrieved successfully",
                Status = _currentStatus,
                Progress = _progress
            };
        }


        /// handler for the "start" command, begins a work operation
        private Task<string> StartWorkHandler(object data)
        {
            if (_currentStatus == "Working")
            {
                return Task.FromResult("Already working on a task. Stop the current task first.");
            }

            _workCancellationSource?.Cancel();
            _workCancellationSource = new CancellationTokenSource();


            var token = _workCancellationSource.Token;
            _ = Task.Run(() => DoWork(data, token), token);

            _currentStatus = "Working";
            _progress = 0;

            return Task.FromResult($"Work started with data: {data}");
        }


        /// handler for the "stop" command, stops the current work operation
        private Task<string> StopWorkHandler(object data)
        {
            if (_currentStatus != "Working")
            {
                return Task.FromResult("No work in progress to stop.");
            }

            _workCancellationSource?.Cancel();
            _currentStatus = "Idle";

            return Task.FromResult("Work stopped successfully.");
        }


        /// handler for the "ping" command, simple connectivity test
        private Task<string> PingHandler(object data)
        {
            return Task.FromResult($"Pong! Time: {DateTime.Now}, Uptime: {(DateTime.Now - _startTime).TotalSeconds} seconds");
        }

        //just for test
        private Task<string> HelloHandler(object data)
        {
            string name = data?.ToString() ?? "there";
            return Task.FromResult($"Hello, {name}! Greetings from Worker Node.");
        }

        /// <summary>
        /// Simulates a long-running work process.
        /// </summary>
        /// <param name="data">Work data</param>
        /// <param name="cancellationToken">Token to monitor for cancellation</param>
        private async Task DoWork(object data, CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine($"Starting work with data: {data}");

                // Simulate progress in 5% increments
                for (int i = 0; i <= 100; i += 5)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Console.WriteLine("Work cancelled");
                        return;
                    }

                    _progress = i;
                    Console.WriteLine($"Work progress: {i}%");

                    // Delay to simulate actual work
                    await Task.Delay(500, cancellationToken);
                }

                // Work completed
                _currentStatus = "Idle";
                Console.WriteLine("Work completed successfully");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Work was cancelled");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during work: {ex.Message}");
                _currentStatus = "Error";
            }
        }
        
    }
}