using SPP.Domain.Data;
using SPP.Communication.Services;
using SPP.Communication.Contracts;
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

// Evităm ambiguitatea: specificăm complet calea către clasa Communication
builder.Services.AddSingleton<ICommunication, SPP.Communication.Services.Communication>();

builder.Services.AddHostedService<WorkerScheduler>();

var workerNodeUrl = Environment.GetEnvironmentVariable("WORKER_NODE_URL") ?? "http://localhost:8080";
builder.Services.AddSingleton(workerNodeUrl);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
