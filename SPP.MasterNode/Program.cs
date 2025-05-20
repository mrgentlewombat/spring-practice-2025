using SPP.Domain.Data;
using SPP.Communication.Services;
using SPP.Communication.Contracts; 
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using SPP.MasterNode.Services;           // WorkerRegistryService for managing registered workers
using SPP.Domain.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add controllers (API endpoints)
builder.Services.AddControllers();

// Register custom service to store and manage worker node registrations
builder.Services.AddSingleton<WorkerRegistryService>();

// Configure database context using MySQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 36))
    ));

// Register generic repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

// Register the communication layer for HTTP requests to workers
builder.Services.AddSingleton<ICommunication, SPP.Communication.Services.Communication>();

// Register background service that periodically sends commands to worker nodes
builder.Services.AddHostedService<WorkerScheduler>();

// Load worker endpoint URL from environment or use default
var workerNodeUrl = Environment.GetEnvironmentVariable("WORKER_NODE_URL") 
                    ?? "http://localhost:5001/api/agent/process";
builder.Services.AddSingleton(workerNodeUrl);

// Enable Swagger/OpenAPI for testing and documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger UI only in Development (optional: remove condition to enable in Production)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure middleware for HTTPS and routing
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Start the application
app.Run();
