using Microsoft.AspNetCore.Mvc;
using RecuritmentTask.src.RecruitmentTask.Api.Constants;
using RecuritmentTask.src.RecruitmentTask.Api.Enums;
using RecuritmentTask.src.RecruitmentTask.Application.Interfaces;
using RecuritmentTask.src.RecruitmentTask.Domain.Entities;

namespace RecuritmentTask.src.RecruitmentTask.Api.Controllers
{
    [ApiController]
    [Route("todo/")]
    public class TodoController : ControllerBase
    {
        private readonly ILogger<TodoController> _logger;
        private readonly ITodoService _todoService;

        public TodoController(ILogger<TodoController> logger, ITodoService todoService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _todoService = todoService ?? throw new ArgumentNullException(nameof(todoService));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTodos()
        {
            var todos = await _todoService.GetAllTodosAsync();
            return Ok(todos);
        }

        [HttpGet("{id:int}")]
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
        public async Task<IActionResult> CreateTodo([FromBody] Todo todo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdTodo = await _todoService.CreateTodoAsync(todo);
                return CreatedAtAction(nameof(GetTodoById), new { id = createdTodo.Id }, createdTodo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the Todo.");
                return StatusCode(StatusCodes.Status500InternalServerError, ErrorMessages.InternalError);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateTodo([FromBody] Todo todo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedTodo = await _todoService.UpdateTodoAsync(todo);
                return Ok(updatedTodo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the Todo.");
                return StatusCode(StatusCodes.Status500InternalServerError, ErrorMessages.InternalError);
            }
        }

        [HttpDelete("{id:int}")]
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
                return StatusCode(StatusCodes.Status500InternalServerError, ErrorMessages.InternalError);
            }
        }

        [HttpPatch("{id:int}/change-percentage")]
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
                return StatusCode(StatusCodes.Status500InternalServerError, ErrorMessages.InternalError);
            }
        }

        [HttpPatch("{id:int}/mark-as-done")]
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
                return StatusCode(StatusCodes.Status500InternalServerError, ErrorMessages.InternalError);
            }
        }
    }


}
