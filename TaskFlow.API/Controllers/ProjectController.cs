using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.API;
using TaskFlow.API.Data;
using TaskFlow.API.DTOs;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectController : ControllerBase
{
    private readonly TaskFlowDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProjectController(TaskFlowDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProjectDto>>> GetProjects()
    {
        var projects = await _context.Projects
            .Include(p => p.Owner)
            .Include(p => p.Tasks)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Ok(projects.Select(MapProject).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectDto dto)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var ownerId))
        {
            return Unauthorized();
        }

        var project = new Project
        {
            Name = dto.Name,
            Description = dto.Description,
            Status = dto.Status,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        await AddActivityAsync(project.Id, "Project", "Create", $"Projeto '{project.Name}' foi criado.");

        var result = await _context.Projects
            .Include(p => p.Owner)
            .Include(p => p.Tasks)
            .SingleAsync(p => p.Id == project.Id);

        return Ok(MapProject(result));
    }

    private async Task AddActivityAsync(Guid entityId, string entityType, string action, string details)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        Guid? userGuid = null;

        if (!string.IsNullOrWhiteSpace(userId) && Guid.TryParse(userId, out var parsedUserId))
        {
            userGuid = parsedUserId;
        }

        _context.AuditLogs.Add(new AuditLog
        {
            EntityId = entityId,
            EntityType = entityType,
            Action = action,
            Details = details,
            PerformedById = userGuid,
            PerformedAt = DateTime.UtcNow,
        });

        await _context.SaveChangesAsync();
    }

    private ProjectDto MapProject(Project project)
    {
        var tasks = project.Tasks ?? new List<TaskItem>();
        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Status = project.Status,
            OwnerId = project.OwnerId,
            OwnerName = project.Owner?.FullName ?? "Sem responsável",
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            TotalTasks = tasks.Count,
            PendingTasks = tasks.Count(t => t.Status == TaskStatus.Pending),
            InProgressTasks = tasks.Count(t => t.Status == TaskStatus.InProgress),
            CompletedTasks = tasks.Count(t => t.Status == TaskStatus.Completed),
        };
    }
}
