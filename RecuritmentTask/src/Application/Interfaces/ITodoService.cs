using Microsoft.EntityFrameworkCore;
using RecuritmentTask.src.Api.Enums;
using RecuritmentTask.src.Domain.Entities;

namespace RecuritmentTask.src.Application.Interfaces
{
    public interface ITodoService
    {
        Task<List<Todo>> GetAllTodosAsync();
        Task<Todo?> GetTodoByIdAsync(int id);
        Task<Todo> CreateTodoAsync(Todo todo);
        Task<Todo> UpdateTodoAsync(Todo todo);
        Task<bool> DeleteTodoAsync(int id);
        Task<List<Todo>> GetTodosByDateAsync(DateTime? startDate, DateTime? endDate);
        Task<List<Todo>> GetTodosByTimeFrameAsync(TimeFrame date);
        Task<Todo> SetTodoCompletionPercentageAsync(int id, double completedPercentage);
        Task<Todo> MarkTodoAsDoneAsync(int id);
    }
}
