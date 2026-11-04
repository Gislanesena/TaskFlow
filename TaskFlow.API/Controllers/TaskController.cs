using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.API;
using TaskFlow.API.Data;
using TaskFlow.API.DTOs;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TaskController : ControllerBase
{
    private readonly TaskFlowDbContext _context;

    public TaskController(TaskFlowDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<TaskDto>>> GetTasks(
        [FromQuery] string? projectId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? query = null)
    {
        var tasksQuery = _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .Include(t => t.CreatedBy)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(projectId) && Guid.TryParse(projectId, out var projectGuid))
        {
            tasksQuery = tasksQuery.Where(t => t.ProjectId == projectGuid);
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TaskStatus>(status, true, out var taskStatus))
        {
            tasksQuery = tasksQuery.Where(t => t.Status == taskStatus);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalized = query.Trim();
            tasksQuery = tasksQuery.Where(t =>
                t.Title.Contains(normalized) ||
                (t.Description != null && t.Description.Contains(normalized)) ||
                (t.Assignee != null && t.Assignee.FullName.Contains(normalized)));
        }

        var tasks = await tasksQuery
            .OrderByDescending(t => t.UpdatedAt)
            .ToListAsync();

        return Ok(tasks.Select(MapTask).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskDto dto)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var createdById))
        {
            return Unauthorized();
        }

        var projectExists = await _context.Projects.AnyAsync(p => p.Id == dto.ProjectId);
        if (!projectExists)
        {
            return NotFound("Projeto não encontrado.");
        }

        if (!Enum.TryParse<TaskStatus>(dto.Status, true, out var status))
        {
            status = TaskStatus.Pending;
        }

        if (!Enum.TryParse<TaskPriority>(dto.Priority, true, out var priority))
        {
            priority = TaskPriority.Medium;
        }

        var task = new TaskItem
        {
            ProjectId = dto.ProjectId,
            Title = dto.Title,
            Description = dto.Description,
            Status = status,
            Priority = priority,
            DueDate = dto.DueDate,
            AssigneeId = dto.AssigneeId,
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        await AddActivityAsync(task.Id, "Task", "Create", $"Tarefa '{task.Title}' foi criada.");

        var saved = await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .Include(t => t.CreatedBy)
            .SingleAsync(t => t.Id == task.Id);

        return Ok(MapTask(saved));
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<TaskDto>> UpdateTask(Guid id, [FromBody] UpdateTaskDto dto)
    {
        var task = await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .Include(t => t.CreatedBy)
            .SingleOrDefaultAsync(t => t.Id == id);

        if (task is null)
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(dto.Title))
            task.Title = dto.Title;
        if (dto.Description is not null)
            task.Description = dto.Description;
        if (!string.IsNullOrWhiteSpace(dto.Status) && Enum.TryParse<TaskStatus>(dto.Status, true, out var status))
            task.Status = status;
        if (!string.IsNullOrWhiteSpace(dto.Priority) && Enum.TryParse<TaskPriority>(dto.Priority, true, out var priority))
            task.Priority = priority;
        if (dto.DueDate.HasValue)
            task.DueDate = dto.DueDate.Value;
        if (dto.AssigneeId.HasValue)
            task.AssigneeId = dto.AssigneeId.Value;

        task.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await AddActivityAsync(task.Id, "Task", "Update", $"Tarefa '{task.Title}' foi atualizada.");

        return Ok(MapTask(task));
    }

    private async Task AddActivityAsync(Guid entityId, string entityType, string action, string details)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid? userId = null;

        if (!string.IsNullOrWhiteSpace(userIdClaim) && Guid.TryParse(userIdClaim, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        _context.AuditLogs.Add(new AuditLog
        {
            EntityId = entityId,
            EntityType = entityType,
            Action = action,
            Details = details,
            PerformedById = userId,
            PerformedAt = DateTime.UtcNow,
        });

        await _context.SaveChangesAsync();
    }

    private TaskDto MapTask(TaskItem task)
    {
        return new TaskDto
        {
            Id = task.Id,
            ProjectId = task.ProjectId,
            ProjectName = task.Project?.Name ?? string.Empty,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status.ToString(),
            Priority = task.Priority.ToString(),
            DueDate = task.DueDate,
            AssigneeId = task.AssigneeId,
            AssigneeName = task.Assignee?.FullName,
            CreatedById = task.CreatedById,
            CreatedByName = task.CreatedBy?.FullName ?? "Sistema",
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
        };
    }
}
