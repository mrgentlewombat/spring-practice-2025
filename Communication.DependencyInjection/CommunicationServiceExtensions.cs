using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Communication.Http;
using Communication.Contracts;
using Communication.Services;
using WorkerNodeApp.Services;
using WorkerNodeApp.Communication;


namespace Communication.DependencyInjection
{
    // centralized dependency injection configuration
    // command that returns this result: Service registration
    // method that returns this result: AddCommunicationServices
    public static class CommunicationServiceExtensions
    {
        // configures services for communication layer
     
        public static IServiceCollection AddCommunicationServices(
            this IServiceCollection services, 
            ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            // configure HttpClient
            // command that returns this result: HTTP communication setup
          
            services.AddHttpClient();

            // register HTTP request helper
            // command that returns this result: HTTP request preparation
            // method that returns this result: AddTransient/AddScoped/AddSingleton
            services.Add(new ServiceDescriptor(
                typeof(HttpRequestHelper), 
                typeof(HttpRequestHelper), 
                lifetime
            ));

            // register communication service
            // command that returns this result: Communication interface implementation
            // method that returns this result: AddTransient/AddScoped/AddSingleton
           services.Add(new ServiceDescriptor(
    typeof(ICommunication), 
    typeof(Communication.Services.Communication), 
    lifetime
));
            // register command processor
            // command that returns this result: Command processing
            // method that returns this result: AddTransient/AddScoped/AddSingleton
            services.Add(new ServiceDescriptor(
                typeof(CommandProcessor), 
                typeof(CommandProcessor), 
                lifetime
            ));

            // optional: Add logging
        
            services.AddLogging(configure => 
            {
                configure.AddConsole();
                configure.AddDebug();
            });

            return services;
        }

        // configures command listener with dependency injection
        // command that returns this result: Listener configuration
     
        public static IServiceCollection AddCommandListener(
            this IServiceCollection services, 
            string ipAddress = "127.0.0.1", 
            int port = 5001)
        {
            // register command listener
            // Command that returns this result: Listener setup
            // method that returns this result: AddTransient
            services.AddTransient(provider => 
                new CommandListener(
                    ipAddress, 
                    port, 
                    provider.GetRequiredService<CommandProcessor>(),
                    provider.GetRequiredService<ILogger<CommandListener>>()
                )
            );

            return services;
        }
    }
}