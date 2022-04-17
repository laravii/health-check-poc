using WorkerHealth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddHealthChecks();

builder.Services.AddControllers();
 builder.Services.AddHostedService<Worker>();
var app = builder.Build();

app.MapHealthChecks("/health");

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
