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
public class DashboardController : ControllerBase
{
    private readonly TaskFlowDbContext _context;

    public DashboardController(TaskFlowDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardDto>> GetDashboard()
    {
        var projects = await _context.Projects
            .Include(p => p.Tasks)
            .Include(p => p.Owner)
            .ToListAsync();

        var allTasks = await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .Include(t => t.CreatedBy)
            .ToListAsync();

        var recentTasks = allTasks
            .OrderByDescending(t => t.UpdatedAt)
            .Take(8)
            .ToList();

        var auditLogs = await _context.AuditLogs
            .Include(a => a.PerformedBy)
            .OrderByDescending(a => a.PerformedAt)
            .Take(6)
            .ToListAsync();

        var totalTasks = allTasks.Count;
        var pending = allTasks.Count(t => t.Status == TaskStatus.Pending);
        var inProgress = allTasks.Count(t => t.Status == TaskStatus.InProgress);
        var completed = allTasks.Count(t => t.Status == TaskStatus.Completed);
        var completionRate = totalTasks == 0 ? 0 : Math.Round((double)completed / totalTasks * 100, 2);

        var result = new DashboardDto
        {
            TotalProjects = projects.Count,
            TotalTasks = totalTasks,
            PendingTasks = pending,
            InProgressTasks = inProgress,
            CompletedTasks = completed,
            CompletionRate = completionRate,
            Projects = projects.Select(p => new ProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Status = p.Status,
                OwnerId = p.OwnerId,
                OwnerName = p.Owner?.FullName ?? "Sem responsável",
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                TotalTasks = p.Tasks.Count,
                PendingTasks = p.Tasks.Count(t => t.Status == TaskStatus.Pending),
                InProgressTasks = p.Tasks.Count(t => t.Status == TaskStatus.InProgress),
                CompletedTasks = p.Tasks.Count(t => t.Status == TaskStatus.Completed),
            }).ToList(),
            RecentTasks = recentTasks.Select(t => new TaskDto
            {
                Id = t.Id,
                ProjectId = t.ProjectId,
                ProjectName = t.Project?.Name ?? string.Empty,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status.ToString(),
                Priority = t.Priority.ToString(),
                DueDate = t.DueDate,
                AssigneeId = t.AssigneeId,
                AssigneeName = t.Assignee?.FullName,
                CreatedById = t.CreatedById,
                CreatedByName = t.CreatedBy?.FullName ?? "Sistema",
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
            }).ToList(),
            RecentActivity = auditLogs.Select(a => new ActivityDto
            {
                Id = a.Id,
                EntityType = a.EntityType,
                Action = a.Action,
                Details = a.Details ?? string.Empty,
                PerformedAt = a.PerformedAt,
            }).ToList(),
        };

        return Ok(result);
    }
}
