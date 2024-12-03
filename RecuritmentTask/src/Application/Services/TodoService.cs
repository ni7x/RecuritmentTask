using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RecuritmentTask.src.RecruitmentTask.Api.Enums;
using RecuritmentTask.src.RecruitmentTask.Application.Interfaces;
using RecuritmentTask.src.RecruitmentTask.Domain.Entities;
using RecuritmentTask.src.RecruitmentTask.Infrastructure.Data;
using System.ComponentModel.DataAnnotations;

namespace RecuritmentTask.src.RecruitmentTask.Application.Services
{
    public class TodoService(IValidator<Todo> validator, TodoDbContext dbContext) : ITodoService
    {
        private readonly TodoDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        private readonly IValidator<Todo> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

        public async Task<List<Todo>> GetAllTodosAsync()
        {
            return await _dbContext.Set<Todo>().ToListAsync();
        }

        public async Task<Todo?> GetTodoByIdAsync(int id)
        {
            return await _dbContext.Set<Todo>().FindAsync(id);
        }

        public async Task<List<Todo>> GetTodosByDateAsync(DateTime? startDate, DateTime? endDate) //[stardDate, endDate)
        {
            IQueryable<Todo> query = _dbContext.Set<Todo>();

            if (startDate.HasValue)
            {
                query = query.Where(todo => todo.ExpiryDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(todo => todo.ExpiryDate < endDate.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<List<Todo>> GetTodosByTimeFrameAsync(TimeFrame timeFrame)
        {
            DateTime today = DateTime.UtcNow.Date;
            DateTime? startDate = null;
            DateTime? endDate = null;

            switch (timeFrame)
            {
                case TimeFrame.Today:
                    startDate = today;
                    endDate = today.AddDays(1);
                    break;

                case TimeFrame.Tommorow:
                    startDate = today.AddDays(1);
                    endDate = today.AddDays(2);
                    break;

                case TimeFrame.CurrentWeek:
                    int diff = (7 - (int)today.DayOfWeek) % 7; //Sunday-0, Monday-1,. .. Saturday-6
                    startDate = today;
                    endDate = today.AddDays(diff);
                    break;

                default:
                    throw new ArgumentException("Invalid timeframe. Use 'Today', 'Tommorow', or 'CurrentWeek'.");
            }

            return await GetTodosByDateAsync(startDate, endDate); ;
        }

        public async Task<Todo> CreateTodoAsync(Todo todo)
        {
            var validationResult = await _validator.ValidateAsync(todo);
            if (!validationResult.IsValid)
            {
                throw new FluentValidation.ValidationException(validationResult.Errors);
            }
            await _dbContext.Set<Todo>().AddAsync(todo);
            await _dbContext.SaveChangesAsync();
            return todo;
        }

        public async Task<Todo> UpdateTodoAsync(Todo todo)
        {
            var validationResult = await _validator.ValidateAsync(todo);
            if (!validationResult.IsValid)
            {
                throw new FluentValidation.ValidationException(validationResult.Errors);
            }
            _dbContext.Set<Todo>().Update(todo);
            await _dbContext.SaveChangesAsync();
            return todo;
        }

        public async Task<bool> DeleteTodoAsync(int id)
        {
            var todo = await GetTodoByIdAsync(id);
            if (todo == null) return false;

            _dbContext.Set<Todo>().Remove(todo);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<Todo> SetTodoCompletionPercentageAsync(int id, double completedPercentage)
        {
            if (completedPercentage < 0 || completedPercentage > 1)
            {
                throw new ArgumentOutOfRangeException("Completed percentage must be between 0.0 and 1.0.");
            }

            var todo = await _dbContext.Set<Todo>().FindAsync(id) ?? throw new InvalidOperationException($"Todo with ID {id} not found.");
            todo.CompletedPercentage = completedPercentage;
            await _dbContext.SaveChangesAsync();

            return todo;
        }

        public async Task<Todo> MarkTodoAsDoneAsync(int id)
        {
            return await SetTodoCompletionPercentageAsync(id, 1.0);
        }

    }

}
