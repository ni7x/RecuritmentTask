using Microsoft.EntityFrameworkCore;
using RecuritmentTask.src.Application.Interfaces;
using RecuritmentTask.src.Domain.Entities;
using RecuritmentTask.src.Infrastructure.Data;

namespace RecuritmentTask.src.Application.Services
{
    public class TodoService : ITodoService
    {
        private readonly TodoDbContext _dbContext;
        public TodoService(TodoDbContext dbContext) {
            _dbContext = dbContext;
        }

        public async Task<List<Todo>> GetAllTodosAsync()
        {
            return await _dbContext.Set<Todo>().ToListAsync();
        }
    }

}
