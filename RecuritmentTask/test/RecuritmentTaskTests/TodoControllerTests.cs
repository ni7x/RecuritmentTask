using Xunit;
using Moq;
using RecuritmentTask.src.Api.Controllers;
using RecuritmentTask.src.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using RecuritmentTask.src.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using FluentValidation;
using RecuritmentTask.src.Application.Validators;
using FluentValidation.Results;
using System.Diagnostics;


namespace RecuritmentTask.Tests
{
    public class TodoControllerTests
    {

        private readonly Mock<ITodoService> _mockTodoService;
        private readonly TodoController _controller;
        private readonly Mock<IValidator<Todo>> _mockValidator;

        public TodoControllerTests()
        {
            _mockTodoService = new Mock<ITodoService>();
            _mockValidator = new Mock<IValidator<Todo>>();
            _controller = new TodoController(Mock.Of<ILogger<TodoController>>(), _mockTodoService.Object, _mockValidator.Object);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(99)]
        [InlineData(1000)]
        public async Task GetTodoById_ShouldHandleValidAndInvalidIds(int id)
        {
            // Arrange
            var testTodo = new Todo { Id = 1, Title = "Test Todo", ExpiryDate = DateTime.UtcNow.AddDays(1), CompletedPercentage = 0 };
            _mockTodoService.Setup(s => s.GetTodoByIdAsync(1)).ReturnsAsync(testTodo);
            _mockTodoService.Setup(s => s.GetTodoByIdAsync(99)).ReturnsAsync((Todo?)null);
            _mockTodoService.Setup(s => s.GetTodoByIdAsync(1000)).ReturnsAsync((Todo?)null);

            // Act
            var result = await _controller.GetTodoById(id);

            // Assert
            if (id == 1)
            {
                result.Should().BeOfType<OkObjectResult>();
                var okResult = result as OkObjectResult;
                okResult!.Value.Should().Be(testTodo);
            }
            else
            {
                result.Should().BeOfType<NotFoundObjectResult>();
            }
        }



        [Fact]
        public async Task GetAllTodos_ShouldReturnOk_WithListOfTodos()
        {
            // Arrange
            var todos = new List<Todo>
            {
                new Todo { Id = 1, Title = "Todo 1", ExpiryDate = DateTime.UtcNow.AddDays(1), CompletedPercentage = 0 },
                new Todo { Id = 2, Title = "Todo 2", ExpiryDate = DateTime.UtcNow.AddDays(2), CompletedPercentage = 0.5 }
            };
            _mockTodoService.Setup(service => service.GetAllTodosAsync()).ReturnsAsync(todos);

            // Act
            var result = await _controller.GetAllTodos();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(todos);
        }

        [Fact]
        public async Task CreateTodo_ShouldReturnCreated()
        {
            // Arrange
            var currentDate = DateTime.UtcNow;
            var todo = new Todo
            {
                Title = "Todo 1",
                ExpiryDate = currentDate.AddDays(1),
                CompletedPercentage = 0
            };

            var createdTodo = new Todo
            {
                Id = 1,
                Title = "Todo 1",
                ExpiryDate = currentDate.AddDays(1),
                CompletedPercentage = 0
            };

            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<Todo>(), default))
                .ReturnsAsync(new ValidationResult());

            _mockTodoService.Setup(s => s.CreateTodoAsync(It.IsAny<Todo>())).ReturnsAsync(createdTodo);

            // Act
            var result = await _controller.CreateTodo(todo);

 
            // Assert
            result.Should().BeOfType<CreatedAtActionResult>(); 
        }


        [Fact]
        public async Task CreateTodo_ShouldReturnBadRequest_WhenModelIsInvalid()
        {
            var invalidTodo = new Todo { Title = "", CompletedPercentage = 1.5, ExpiryDate = DateTime.UtcNow };
            _mockValidator
                .Setup(v => v.ValidateAsync(invalidTodo, default))
                .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Title", "Title is required") }));

            // Act
            var result = await _controller.CreateTodo(invalidTodo);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;

            var value = badRequestResult?.Value;
            value.Should().NotBeNull();

            value.Should().BeOfType<FluentValidation.Results.ValidationResult>();
   
            var validationDetails = value as FluentValidation.Results.ValidationResult;
            validationDetails?.Errors.Should().ContainSingle();

        }
        
        [Fact]
        public async Task DeleteTodo_ShouldReturnNoContent_WhenTodoExists()
        {
            // Arrange
            _mockTodoService.Setup(s => s.DeleteTodoAsync(It.IsAny<int>())).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteTodo(1);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteTodo_ShouldReturnNotFound_WhenTodoDoesNotExist()
        {
            // Arrange
            _mockTodoService.Setup(s => s.DeleteTodoAsync(It.IsAny<int>())).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteTodo(99);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task SetTodoCompletionPercentage_ShouldReturnOk_WhenValid()
        {
            // Arrange
            var updatedTodo = new Todo { Id = 1, CompletedPercentage = 0.5, Title = "", ExpiryDate = DateTime.UtcNow };
            _mockTodoService.Setup(s => s.SetTodoCompletionPercentageAsync(1, 0.5)).ReturnsAsync(updatedTodo);

            // Act
            var result = await _controller.SetTodoCompletionPercentage(1, 0.5);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be(updatedTodo);
        }

        [Fact]
        public async Task SetTodoCompletionPercentage_ShouldReturnBadRequest_WhenPercentageIsInvalid()
        {
            // Arrange
            _mockTodoService.Setup(s => s.SetTodoCompletionPercentageAsync(It.IsAny<int>(), -0.5))
                .ThrowsAsync(new ArgumentOutOfRangeException());

            // Act
            var result = await _controller.SetTodoCompletionPercentage(1, -0.5);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task MarkTodoAsDone_ShouldReturnOk_WhenTodoIsMarkedDone()
        {
            // Arrange
            var updatedTodo = new Todo { Id = 1, CompletedPercentage = 0.5, Title = "", ExpiryDate = DateTime.UtcNow };
            _mockTodoService.Setup(s => s.MarkTodoAsDoneAsync(1)).ReturnsAsync(updatedTodo);

            // Act
            var result = await _controller.MarkTodoAsDone(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be(updatedTodo);
        }

        [Fact]
        public async Task MarkTodoAsDone_ShouldReturnNotFound_WhenTodoDoesNotExist()
        {
            // Arrange
            _mockTodoService.Setup(s => s.MarkTodoAsDoneAsync(It.IsAny<int>())).ReturnsAsync((Todo?)null!);

            // Act
            var result = await _controller.MarkTodoAsDone(99);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

    }
}