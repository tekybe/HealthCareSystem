using System.Reflection;
using FluentValidation;
using MediatR;
using Serilog;
using HealthcareSystem.API.Behaviors;
using HealthcareSystem.Application.Queries;
using HealthcareSystem.Infrastructure.Repositories;
using HealthcareSystem.Application.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Serilog ILogger instance
builder.Services.AddSingleton<Serilog.ILogger>(Log.Logger);

// Register MediatR manually
var applicationAssembly = typeof(GetPatientQuery).Assembly;
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));

// Register Fluent Validation
builder.Services.AddValidatorsFromAssembly(typeof(GetPatientQueryValidator).Assembly);

// Register validation pipeline behavior
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Register repositories
builder.Services.AddScoped<IPatientRepository, PatientRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
