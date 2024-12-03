using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RecuritmentTask.src.Application.Interfaces;
using RecuritmentTask.src.Application.Services;
using RecuritmentTask.src.Application.Validators;
using RecuritmentTask.src.Domain.Entities;
using RecuritmentTask.src.Infrastructure.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) 
    .WriteTo.Console() 
    .WriteTo.File("Logs/todo.log", rollingInterval: RollingInterval.Day) 
    .CreateLogger();

builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IValidator<Todo>, TodoValidator>();

builder.Services.AddScoped<ITodoService, TodoService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseSerilog();


var app = builder.Build();

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
