using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using RecuritmentTask.src.Api.Constants;
using RecuritmentTask.src.Api.Enums;
using RecuritmentTask.src.Application.Interfaces;
using RecuritmentTask.src.Domain.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace RecuritmentTask.src.Api.Controllers
{
    [ApiController]
    [Route("todo/")]
    public class TodoController : ControllerBase
    {
        private readonly ILogger<TodoController> _logger;
        private readonly ITodoService _todoService;
        private readonly IValidator<Todo> _validator;
        public TodoController(ILogger<TodoController> logger, ITodoService todoService, IValidator<Todo> validator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _todoService = todoService ?? throw new ArgumentNullException(nameof(todoService));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }


        [HttpGet]
        [SwaggerOperation(Summary = "Get all Todos", Description = "Retrieves all the Todos")]
        [SwaggerResponse(200, "Successfully retrieved the Todos", typeof(IEnumerable<Todo>))]
        public async Task<IActionResult> GetAllTodos()
        {
            var todos = await _todoService.GetAllTodosAsync();
            return Ok(todos);
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Get Todo by ID", Description = "Retrieves a specific Todo by its ID.")]
        [SwaggerResponse(200, "Successfully retrieved the Todo", typeof(Todo))]
        [SwaggerResponse(404, "Todo with the specified ID not found")]
        public async Task<IActionResult> GetTodoById(int id)
        {
            try
            {
                var todo = await _todoService.GetTodoByIdAsync(id);
                return todo != null ? Ok(todo) : NotFound($"Todo with ID {id} was not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving Todo with ID - {TodoId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, ErrorMessages.InternalError);
            }
        }

        [HttpGet("incoming")]
        [SwaggerOperation(Summary = "Get incoming Todos by TimeFrame", Description = "Retrieves Todos that fall within the specified timeframe.(Today, Tommorow, CurrentWeek)")]
        [SwaggerResponse(200, "Successfully retrieved Todos for the specified timeframe", typeof(IEnumerable<Todo>))]
        [SwaggerResponse(500, "Internal server error while fetching Todos")]
        public async Task<IActionResult> GetIncomingTodos([FromQuery] TimeFrame timeFrame)
        {
            try
            {
                var todos = await _todoService.GetTodosByTimeFrameAsync(timeFrame);
                return Ok(todos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving Todos for TimeFrame - {timeFrame}");
                return StatusCode(StatusCodes.Status500InternalServerError, ErrorMessages.InternalError);
            }
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Create a new Todo", Description = "Creates a new Todo.")]
        [SwaggerResponse(201, "Successfully created a Todo", typeof(Todo))]
        [SwaggerResponse(400, "Bad request due to invalid data")]
        [SwaggerResponse(500, "Internal server error while creating the Todo")]
        public async Task<IActionResult> CreateTodo([FromBody] Todo todo)
        {
            ValidationResult result = await _validator.ValidateAsync(todo);
            if (!result.IsValid)
            {
                return BadRequest(result); 
            }

            try
            {
                var createdTodo = await _todoService.CreateTodoAsync(todo);
                return CreatedAtAction(nameof(GetTodoById), new { id = createdTodo.Id }, createdTodo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the Todo.");
                return StatusCode(StatusCodes.Status500InternalServerError, ErrorMessages.InternalError + ex);
            }
        }

        [HttpPut]
        [SwaggerOperation(Summary = "Update an existing Todo", Description = "Updatesan  existing Todo.")]
        [SwaggerResponse(200, "Successfully updated the Todo", typeof(Todo))]
        [SwaggerResponse(400, "Bad request due to invalid data")]
        [SwaggerResponse(500, "Internal server error while updating the Todo")]
        public async Task<IActionResult> UpdateTodo([FromBody] Todo todo)
        {
            ValidationResult result = await _validator.ValidateAsync(todo);
        
            if (!result.IsValid)
            {
                return BadRequest(result);
            }

            try
            {
                var updatedTodo = await _todoService.UpdateTodoAsync(todo);
                return Ok(updatedTodo); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the Todo.");
                return StatusCode(StatusCodes.Status500InternalServerError, ErrorMessages.InternalError + ex);
            }
        }

        [HttpDelete("{id:int}")]
        [SwaggerOperation(Summary = "Delete a Todo", Description = "Deletes a Todo by its ID.")]
        [SwaggerResponse(204, "Successfully deleted the Todo")]
        [SwaggerResponse(404, "Todo with the specified ID not found")]
        [SwaggerResponse(500, "Internal server error while deleting the Todo")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            try
            {
                var result = await _todoService.DeleteTodoAsync(id);
                if (!result)
                {
                    return NotFound($"Todo with ID {id} not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the Todo.");
                return StatusCode(StatusCodes.Status500InternalServerError, ErrorMessages.InternalError + ex);
            }
        }

        [HttpPatch("{id:int}/change-percentage")]
        [SwaggerOperation(Summary = "Change Todo Percentage", Description = "Changes Todo's completion percentage([0,1]).")]
        [SwaggerResponse(200, "Successfully updated the Todo completion percentage", typeof(Todo))]
        [SwaggerResponse(400, "Bad request due to invalid percentage")]
        [SwaggerResponse(404, "Todo with the specified ID not found")]
        [SwaggerResponse(500, "Internal server error while updating completion percentage")]
        public async Task<IActionResult> SetTodoCompletionPercentage(int id, [FromBody] double completedPercentage)
        {
            try
            {
                var updatedTodo = await _todoService.SetTodoCompletionPercentageAsync(id, completedPercentage);

                if (updatedTodo == null)
                {
                    return NotFound($"Todo with ID {id} not found.");
                }

                return Ok(updatedTodo); 
            }
            catch (ArgumentOutOfRangeException ex) 
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)  
            {
                _logger.LogError(ex, "An error occurred while setting the completion percentage.");
                return StatusCode(StatusCodes.Status500InternalServerError, ErrorMessages.InternalError + ex);
            }
        }

        [HttpPatch("{id:int}/mark-as-done")]
        [SwaggerOperation(Summary = "Mark Todo as done", Description = "Marks a Todo as completed.")]
        [SwaggerResponse(200, "Successfully marked the Todo as done", typeof(Todo))]
        [SwaggerResponse(404, "Todo with the specified ID not found")]
        [SwaggerResponse(500, "Internal server error")]
        public async Task<IActionResult> MarkTodoAsDone(int id)
        {
            try
            {
                var updatedTodo = await _todoService.MarkTodoAsDoneAsync(id);

                if (updatedTodo == null)
                {
                    return NotFound($"Todo with ID {id} not found.");
                }

                return Ok(updatedTodo); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while marking the Todo as done.");
                return StatusCode(StatusCodes.Status500InternalServerError, ErrorMessages.InternalError + ex);
            }
        }
    }


}
