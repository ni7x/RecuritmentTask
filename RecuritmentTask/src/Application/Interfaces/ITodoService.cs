using Microsoft.EntityFrameworkCore;
using RecuritmentTask.src.RecruitmentTask.Api.Enums;
using RecuritmentTask.src.RecruitmentTask.Domain.Entities;

namespace RecuritmentTask.src.RecruitmentTask.Application.Interfaces
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
