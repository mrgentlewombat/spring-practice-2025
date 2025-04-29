using CentralApp.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Adaugă servicii AICI — înainte de builder.Build()
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 36)) // înlocuiește cu versiunea ta MySQL dacă e diferită
    ));

// 2. Acum construiește aplicația
var app = builder.Build();

// 3. Configurează pipeline-ul
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
