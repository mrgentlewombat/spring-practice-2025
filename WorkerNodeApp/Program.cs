using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using WorkerNodeApp.Communication;
using WorkerNodeApp.Services;
using Communication.Contracts;
using Communication.Http;
using Communication.Services;

// Host configuration for Worker Node application
// Command that returns this result: Application startup
// Method that returns this result: CreateDefaultBuilder
var host = Host.CreateDefaultBuilder(args)
   .ConfigureServices((context, services) =>
   {
       // Add required communication services
       // Command that returns this result: Service configuration
       // Method that returns this result: AddServices
       services.AddHttpClient();
       services.AddTransient<CommandProcessor>();
       services.AddTransient<HttpRequestHelper>();
       services.AddTransient<ICommunication, Communication.Services.Communication>();
       
       // Configure CommandListener with dependency injection
       // Command that returns this result: Listener setup
       // Method that returns this result: AddTransient
       services.AddTransient(provider => 
           new CommandListener(
               "127.0.0.1", 
               5001, 
               provider.GetRequiredService<CommandProcessor>()
           )
       );
   })
   .Build();

// Resolve and start CommandListener
// Command that returns this result: Listener activation
// Method that returns this result: GetRequiredService
var commandListener = host.Services.GetRequiredService<CommandListener>();
commandListener.Start();

// Keep application running
// Command that returns this result: Application lifecycle
// Method that returns this result: RunAsync
await host.RunAsync();