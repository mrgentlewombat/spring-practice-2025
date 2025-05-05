using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Communication.DependencyInjection;
using WorkerNodeApp.Communication;
using WorkerNodeApp.Services;

namespace WorkerNodeTestConsole
{
   // test console for Worker Node application
   // command that returns this result: Worker Node testing
 
   class Program
   {
       // main entry point for testing Worker Node
       // command that returns this result: Worker Node initialization
   
       static void Main(string[] args)
       {
           Console.WriteLine("Worker Node Test Console - Starting...");

           try
           {
               // create host with dependency injection
               // command that returns this result: Service configuration
               // method that returns this result: CreateHostBuilder
               using IHost host = CreateHostBuilder(args).Build();
               
               // resolve command processor and listener
               // command that returns this result: Service resolution
               // method that returns this result: GetRequiredService
               var commandProcessor = host.Services.GetRequiredService<CommandProcessor>();
               var commandListener = host.Services.GetRequiredService<CommandListener>();
               
               // start listening for commands
               // command that returns this result: Listener startup
               // method that returns this result: Start
               commandListener.Start();
               
               Console.WriteLine("Worker Node is running. Press Ctrl+C to stop.");
               
               // wait for cancellation
               // command that returns this result: Application lifecycle management
               // method that returns this result: ManualResetEvent
               var waitHandle = new ManualResetEvent(false);
               
               // handle graceful shutdown
               // command that returns this result: Shutdown process
               // method that returns this result: Console.CancelKeyPress handler
               Console.CancelKeyPress += (sender, e) => {
                   e.Cancel = true;
                   Console.WriteLine("Shutting down...");
                   commandListener.Stop();
                   waitHandle.Set();
               };
               
               // block until shutdown signal
               // command that returns this result: Application waiting
               // method that returns this result: WaitOne
               waitHandle.WaitOne();
           }
           catch (Exception ex)
           {
               // error handling
            
               Console.WriteLine($"Error: {ex.Message}");
               Console.WriteLine(ex.StackTrace);
           }
           
           Console.WriteLine("Worker Node Test Console shutting down...");
       }

       // configures dependency injection services
       // command that returns this result: Service configuration
     
       static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
               .ConfigureServices((_, services) =>
               {
                   // add communication services
                   // command that returns this result: DI configuration
                   // method that returns this result: AddCommunicationServices
                   services
                       .AddCommunicationServices()
                       .AddCommandListener(); // configure command listener
               })
               .ConfigureLogging(logging =>
               {
                   // configure logging
                  
                   logging.AddConsole();
                   logging.SetMinimumLevel(LogLevel.Information);
               });
   }
}