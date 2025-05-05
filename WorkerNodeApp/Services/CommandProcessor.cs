using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Communication.Models;
using Microsoft.Extensions.Logging;

namespace WorkerNodeApp.Services
{
   // processes commands received from the CentralApp
    public class CommandProcessor
   {
       // dictionary of command handlers
       // command that returns this result: "ping", "start", "stop", "hello"
       
       private readonly Dictionary<string, Func<object, Task<string>>> _commandHandlers;
       
       // current status of the worker node
       // command that returns this result: All commands that modify state
       // method that returns this result: GetStatus
       private string _currentStatus = "Idle";
       
       // P\progress of current work
       // command that returns this result: "start", "stop"
       // method that returns this result: GetStatus
       private double _progress = 0;
       
       // cancellation source for work operations
       // command that returns this result: "start", "stop"
       // method that returns this result: StartWorkHandler, StopWorkHandler
       private CancellationTokenSource _workCancellationSource;
       
       // start time of the worker node
       // command that returns this result: "ping"
       // method that returns this result: PingHandler
       private readonly DateTime _startTime;

       // logger for tracking events
      
       private readonly ILogger<CommandProcessor> _logger;

 
       public CommandProcessor(ILogger<CommandProcessor> logger = null)
       {
           _startTime = DateTime.Now;
           _logger = logger;

           // register command handlers
           // command that returns this result: "ping", "start", "stop", "hello"
           // method that returns this result: Command registration
           _commandHandlers = new Dictionary<string, Func<object, Task<string>>>(StringComparer.OrdinalIgnoreCase)
           {
               ["start"] = StartWorkHandler,
               ["stop"] = StopWorkHandler,
               ["ping"] = PingHandler,
               ["hello"] = HelloHandler
           };
       }

       // processes a command and returns the result
     
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

           // find and execute command handler
           // method that returns this result: ProcessCommandAsync
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
                   _logger?.LogError(ex, $"Error executing command: {command}");
                   return new CommandResponse
                   {
                       Success = false,
                       Message = $"Error executing command: {ex.Message}"
                   };
               }
           }
           else
           {
               _logger?.LogWarning($"Unknown command: {command}");
               return new CommandResponse
               {
                   Success = false,
                   Message = $"Unknown command: {command}"
               };
           }
       }

       // gets the current status of the worker node
       // command that returns this result: "status"
       
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

       // handler for the "start" command
       // command that returns this result: "start"
      
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

       // handler for the "stop" command
       // command that returns this result: "stop"
     
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

       // handler for the "ping" command
       // command that returns this result: "ping"
      
       private Task<string> PingHandler(object data)
       {
           return Task.FromResult($"Pong! Time: {DateTime.Now}, Uptime: {(DateTime.Now - _startTime).TotalSeconds} seconds");
       }

       // handler for the "hello" command
       // command that returns this result: "hello"
     
       private Task<string> HelloHandler(object data)
       {
           string name = data?.ToString() ?? "there";
           return Task.FromResult($"Hello, {name}! Greetings from Worker Node.");
       }

       // simulates a long-running work process
      
       private async Task DoWork(object data, CancellationToken cancellationToken)
       {
           try
           {
               Console.WriteLine($"Starting work with data: {data}");
               
               // simulate progress in 5% increments
               for (int i = 0; i <= 100; i += 5)
               {
                   if (cancellationToken.IsCancellationRequested)
                   {
                       Console.WriteLine("Work cancelled");
                       return;
                   }
                   
                   _progress = i;
                   Console.WriteLine($"Work progress: {i}%");
                   
                   // delay to simulate actual work
                   await Task.Delay(500, cancellationToken);
               }
               
           
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