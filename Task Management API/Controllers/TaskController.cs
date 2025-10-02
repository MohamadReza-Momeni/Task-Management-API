using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task_Management_API.Data;
using Task_Management_API.DTOs;
using Task_Management_API.Models.Enums;
using Task_Management_API.Mappers;

namespace Task_Management_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly TaskDbContext _context;

        public TasksController(TaskDbContext context)
        {
            _context = context;
        }

        // GET: api/tasks
        [HttpGet]
        public async Task<ActionResult<object>> GetTasks(
            bool? isCompleted,
            Priority? priority,
            DateTime? dueBefore,
            int page = 1,
            int pageSize = 10,
            string sortBy = "CreatedAt",
            string order = "desc")
        {
            var query = _context.Tasks.AsQueryable();

            // Filtering
            if (isCompleted.HasValue)
                query = query.Where(t => t.IsCompleted == isCompleted.Value);

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            if (dueBefore.HasValue)
                query = query.Where(t => t.DueDate <= dueBefore.Value);

            // Sorting
            query = sortBy.ToLower() switch
            {
                "duedate" => (order == "asc") ? query.OrderBy(t => t.DueDate) : query.OrderByDescending(t => t.DueDate),
                "priority" => (order == "asc") ? query.OrderBy(t => t.Priority) : query.OrderByDescending(t => t.Priority),
                _ => (order == "asc") ? query.OrderBy(t => t.CreatedAt) : query.OrderByDescending(t => t.CreatedAt)
            };

            // Pagination
            var totalCount = await query.CountAsync();
            var tasks = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                page,
                pageSize,
                totalCount,
                tasks = tasks.Select(t => t.ToTaskResponse())
            });
        }

        // GET: api/tasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskResponse>> GetTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
                return NotFound();

            return Ok(task.ToTaskResponse());
        }

        // POST: api/tasks
        [HttpPost]
        public async Task<ActionResult<TaskResponse>> CreateTask(CreateTaskRequest request)
        {
            if (request.DueDate.HasValue && request.DueDate.Value < DateTime.UtcNow)
                return BadRequest(new { error = "Due date cannot be in the past." });

            var task = request.ToTaskItem();

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task.ToTaskResponse());
        }

        // PUT: api/tasks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, UpdateTaskRequest request)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return NotFound();

            task.UpdateTaskItem(request);

            await _context.SaveChangesAsync();

            return Ok(task.ToTaskResponse());
        }

        // DELETE: api/tasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return NotFound();

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
