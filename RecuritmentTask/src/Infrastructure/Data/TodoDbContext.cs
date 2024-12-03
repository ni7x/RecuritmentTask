using Microsoft.EntityFrameworkCore;
using RecuritmentTask.src.RecruitmentTask.Domain.Entities;

namespace RecuritmentTask.src.RecruitmentTask.Infrastructure.Data
{
    public class TodoDbContext : DbContext
    {
        public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) { }

        public DbSet<Todo> Todos { get; set; }

    }
}
