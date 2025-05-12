using CentralApp.Data;
using Microsoft.EntityFrameworkCore;
using Communication.Services;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add controller support (for API endpoints)
builder.Services.AddControllers();

// Register the database context and configure MySQL connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 36))
    ));

// Register communication service
builder.Services.AddSingleton<ICommunication, Communication>();

// Register WorkerScheduler as a background service
builder.Services.AddHostedService<WorkerScheduler>();

// Configure worker node URL from environment variable
var workerNodeUrl = Environment.GetEnvironmentVariable("WORKER_NODE_URL") ?? "http://localhost:8080";
builder.Services.AddSingleton(workerNodeUrl);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
