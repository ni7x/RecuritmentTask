using Microsoft.EntityFrameworkCore;
using RecuritmentTask.src.Domain.Entities;

namespace RecuritmentTask.src.Infrastructure.Data
{
    public class TodoDbContext : DbContext
    {
        public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) { }

        public DbSet<Todo> Todos { get; set; }

    }
}
