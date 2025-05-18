using Domain;                            // AppDbContext
using Communication.Contracts;           // ICommunication
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 36))
    ));

builder.Services.AddSingleton<ICommunication, Communication.Services.Communication>();

builder.Services.AddHostedService<WorkerScheduler>();

var workerNodeUrl = Environment.GetEnvironmentVariable("WORKER_NODE_URL") ?? "http://localhost:8080";
builder.Services.AddSingleton(workerNodeUrl);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();