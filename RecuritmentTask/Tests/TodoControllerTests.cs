using Xunit;
using Moq;
using RecuritmentTask.src.Api.Controllers;
using RecuritmentTask.src.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using RecuritmentTask.src.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;


namespace Tests;

public class TodoControllerTests
{

    private readonly Mock<ITodoService> _mockTodoService;
    private readonly TodoController _controller;

    public TodoControllerTests()
    {
        _mockTodoService = new Mock<ITodoService>();
        _controller = new TodoController(Mock.Of<ILogger<TodoController>>(), _mockTodoService.Object);
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
        var todo = new Todo { Title = "New Todo", ExpiryDate = currentDate.AddDays(1) };
        var createdTodo = new Todo { Id = 1, Title = "New Todo", ExpiryDate = currentDate.AddDays(1) };

        _mockTodoService.Setup(s => s.CreateTodoAsync(It.IsAny<Todo>())).ReturnsAsync(createdTodo);

        // Act
        var result = await _controller.CreateTodo(todo);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult!.Value.Should().Be(createdTodo);
    }


    [Fact]
    public async Task CreateTodo_ShouldReturnBadRequest_WhenModelIsInvalid()
    {
        // Arrange
        _controller.ModelState.AddModelError("Title", "Title is required");

        // Act
        var result = await _controller.CreateTodo(new Todo { Id = 1, CompletedPercentage = 0.5, Title = "", ExpiryDate = DateTime.UtcNow });

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
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
        var updatedTodo = new Todo  { Id = 1, CompletedPercentage = 0.5, Title = "", ExpiryDate = DateTime.UtcNow  };
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