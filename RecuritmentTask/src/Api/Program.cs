using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RecuritmentTask.src.Application.Validators;
using RecuritmentTask.src.RecruitmentTask.Application.Interfaces;
using RecuritmentTask.src.RecruitmentTask.Application.Services;
using RecuritmentTask.src.RecruitmentTask.Domain.Entities;
using RecuritmentTask.src.RecruitmentTask.Infrastructure.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) 
    .WriteTo.Console() 
    .WriteTo.File("Logs/todo.log", rollingInterval: RollingInterval.Day) 
    .CreateLogger();

var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
var connectionString = $"Host={dbHost};Database=todo_db;Username=testuser;Password=testpassword";

builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseNpgsql(connectionString));


builder.Services.AddScoped<IValidator<Todo>, TodoValidator>();

builder.Services.AddScoped<ITodoService, TodoService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseSerilog();


var app = builder.Build();

await using var scope = app.Services.CreateAsyncScope();
await using var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
await db.Database.MigrateAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
