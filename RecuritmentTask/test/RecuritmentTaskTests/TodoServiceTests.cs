using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using RecuritmentTask.src.Api.Enums;
using RecuritmentTask.src.Application.Services;
using RecuritmentTask.src.Application.Validators;
using RecuritmentTask.src.Domain.Entities;
using RecuritmentTask.src.Infrastructure.Data;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace RecuritmentTask.Tests
{
    public class TodoServiceTests
    {
        private readonly TodoService _service;
        private readonly TodoDbContext _dbContext;
        private readonly IValidator<Todo> _validator;

        public TodoServiceTests()
        {
            var options = new DbContextOptionsBuilder<TodoDbContext>()
                .UseInMemoryDatabase("TodoTestDb")  
                .Options;
            _validator = new TodoValidator();
            _dbContext = new TodoDbContext(options);
            _service = new TodoService(_validator, _dbContext);
        }

        [Fact]
        public async Task CreateTodoAsync_ShouldCreateTodo_WhenTodoIsValid()
        {
            // Arrange
            var validTodo = new Todo
            {
                Title = "Valid Title",
                Description = "Valid Description",
                ExpiryDate = DateTime.UtcNow.AddDays(1),
                CompletedPercentage = 0.5
            };

            // Act
            var createdTodo = await _service.CreateTodoAsync(validTodo);

            // Assert
            createdTodo.Should().BeEquivalentTo(validTodo);
        }

        [Theory]
        [MemberData(nameof(TodoTestDataHelper.GetInvalidData), MemberType = typeof(TodoTestDataHelper))]
        public async Task CreateTodoAsync_ShouldThrowException_WhenTodoIsInvalid(string title, string description, string expiryDateString, double completedPercentage)
        {
            // Arrange
            DateTime expiryDate = DateTime.Parse(expiryDateString);

            var invalidTodo = new Todo
            {
                Title = title,  
                Description = description,  
                ExpiryDate = expiryDate,
                CompletedPercentage = completedPercentage
            };

            // Act
            Func<Task> act = async () => await _service.CreateTodoAsync(invalidTodo);

            // Assert
            await act.Should().ThrowAsync<FluentValidation.ValidationException>(); //Should propably check each validation error for each case separately
   
        }

        [Fact]
        public async Task GetTodoByIdAsync_ShouldReturnTodo_WhenTodoExists()
        {
            // Arrange
            var validTodo = new Todo
            {
                Title = "Valid Title",
                Description = "Valid Description",
                ExpiryDate = DateTime.UtcNow.AddDays(1),
                CompletedPercentage = 0.5
            };

            await _dbContext.Set<Todo>().AddAsync(validTodo);
            await _dbContext.SaveChangesAsync();

            // Act
            var fetchedTodo = await _service.GetTodoByIdAsync(validTodo.Id);

            // Assert
            fetchedTodo.Should().NotBeNull();
            fetchedTodo.Should().BeEquivalentTo(validTodo);
        }

        [Fact]
        public async Task GetTodoByIdAsync_ShouldReturnNull_WhenTodoDoesNotExist()
        {
            // Act
            var fetchedTodo = await _service.GetTodoByIdAsync(9991);

            // Assert
            fetchedTodo.Should().BeNull();
        }

        [Theory]
        [InlineData(TimeFrame.Today)]
        [InlineData(TimeFrame.Tommorow)]
        [InlineData(TimeFrame.CurrentWeek)]
        public async Task GetTodosByTimeFrameAsync_ShouldReturnTodosWithingTimeFrame(TimeFrame timeFrame)
        {
            var today = DateTime.UtcNow.Date;
            var todos = new List<Todo>
            {
                new Todo { Title = "title 1", Description = "Description 1", ExpiryDate = today.AddHours(10) },
                new Todo {  Title = "title 2", Description = "Description 2",  ExpiryDate = today.AddDays(1).AddHours(5) },
                new Todo {  Title = "title 3", Description = "Description 3", ExpiryDate = today.AddDays(7) }
            };

            await _dbContext.Todos.AddRangeAsync(todos);
            await _dbContext.SaveChangesAsync();

            var result = await _service.GetTodosByTimeFrameAsync(timeFrame);

            switch (timeFrame)
            {
                case TimeFrame.Today:
                    result.Should().AllSatisfy(todo => todo.ExpiryDate.Date.Should().Be(today.Date));
                    break;
                case TimeFrame.Tommorow:
                    result.Should().AllSatisfy(todo => todo.ExpiryDate.Date.Should().Be(today.AddDays(1).Date));
                    break;
                case TimeFrame.CurrentWeek:
                    result.Should().AllSatisfy(todo => todo.ExpiryDate.Date.Should().BeOnOrBefore(today.AddDays(6).Date));
                    break;
            }
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        public async Task SetTodoCompletionPercentageAsync_ShouldThrowArgumentOutOfRangeException_WhenPercentageIsInvalid(int invalidPercentage)
        {
    
            // Act
            Func<Task> act = async () => await _service.SetTodoCompletionPercentageAsync(1, invalidPercentage);

            // Assert
            await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
                .WithMessage("Specified argument was out of the range of valid values. (Parameter 'Completed percentage must be between 0.0 and 1.0.')");
        }

        [Fact]
        public async Task DeleteTodoAsync_ShouldDeleteTodo_WhenTodoExists()
        {
            // Arrange
            var validTodo = new Todo
            {
                Title = "Valid Title",
                Description = "Valid Description",
                ExpiryDate = DateTime.UtcNow.AddDays(1),
                CompletedPercentage = 0.5
            };

            await _dbContext.Set<Todo>().AddAsync(validTodo);
            await _dbContext.SaveChangesAsync();
            // Act
            var result = await _service.DeleteTodoAsync(validTodo.Id);
            // Assert
            result.Should().BeTrue();
            var deletedTodo = await _dbContext.Set<Todo>().FindAsync(validTodo.Id);
            deletedTodo.Should().BeNull();
        }

    }

}