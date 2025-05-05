using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Communication.DependencyInjection;
using Communication.Http;
using Communication.Services;
using Communication.Contracts;

// host configuration with WebAPI-style Dependency Injection
// purpose: Set up service container and dependency management
// key components: Hosting, service registration, communication setup
var host = Host.CreateDefaultBuilder(args)
   .ConfigureServices((context, services) =>
   {
       // add HTTP client support for network communication
       // enables making HTTP requests across the application
       // command that returns this result: Network communication setup
       // method that returns this result: AddHttpClient
       services.AddHttpClient();
       
       // register communication-related services
       // transient: New instance created for each request
       // enables flexible, decoupled communication mechanisms
       // command that returns this result: Communication service registration
       // method that returns this result: AddTransient
       services.AddTransient<HttpRequestHelper>();
       services.AddTransient<ICommunication, Communication.Services.Communication>();
       
       // Add any additional application-specific services here
   })
   .Build();

// Keeps the application running and manages service lifecycle

await host.RunAsync();