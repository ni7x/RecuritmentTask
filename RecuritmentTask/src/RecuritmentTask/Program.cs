using FluentAssertions.Common;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecuritmentTask.src.Application.Interfaces;
using RecuritmentTask.src.Application.Services;
using RecuritmentTask.src.Application.Validators;
using RecuritmentTask.src.Domain.Entities;
using RecuritmentTask.src.Infrastructure.Data;
using Serilog;
using Swashbuckle.AspNetCore.Annotations; 

var builder = WebApplication.CreateBuilder(args);



//Setup logging to files

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("Logs/todo.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();



//Connect to database depending on whether it is ran by docker

var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
var connectionString = $"Host={dbHost};Database=todo_db;Username=testuser;Password=testpassword";

builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseNpgsql(connectionString));



//Configure Services

builder.Services.AddScoped<IValidator<Todo>, TodoValidator>();
builder.Services.AddScoped<ITodoService, TodoService>();
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
});

var app = builder.Build();



//Make migrations

await using var scope = app.Services.CreateAsyncScope();
await using var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
if (app.Environment.IsDevelopment())
{
    await db.Database.EnsureDeletedAsync(); // Drops the database if it exists
    await db.Database.EnsureCreatedAsync(); // Creates a new database with the current model
}
else
{
    await db.Database.MigrateAsync(); // Apply migrations
}


// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger/index.html"));
}
else
{
    app.MapGet("/", () => Results.Content("Welcome to the API"));
}
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();