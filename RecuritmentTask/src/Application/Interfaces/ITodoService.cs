using Microsoft.EntityFrameworkCore;
using RecuritmentTask.src.Domain.Entities;

namespace RecuritmentTask.src.Application.Interfaces
{
    public interface ITodoService
    {
         Task<List<Todo>> GetAllTodosAsync();
    }
}
