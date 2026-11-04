using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskFlow.API;

namespace TaskFlow.API.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<TaskFlowDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        await context.Database.EnsureCreatedAsync();

        var roles = new[] { "Admin", "Manager", "Member" };
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = roleName, Description = roleName });
            }
        }

        var adminEmail = "admin@taskflow.local";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Administrador TaskFlow",
                JobTitle = "Gerente de Operações",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        if (!await context.Projects.AnyAsync())
        {
            var project = new Project
            {
                Name = "Lançamento do Relatório Web",
                Description = "Projeto de entrega final com gestão de tarefas e indicadores de equipes.",
                Status = "Active",
                OwnerId = adminUser.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            context.Projects.Add(project);
            await context.SaveChangesAsync();

            var task1 = new TaskItem
            {
                ProjectId = project.Id,
                Title = "Definir arquitetura da API",
                Description = "Mapear endpoints, fluxos e autenticação JWT para a aplicação.",
                Status = TaskStatus.InProgress,
                Priority = TaskPriority.High,
                DueDate = DateTime.UtcNow.AddDays(3),
                AssigneeId = adminUser.Id,
                CreatedById = adminUser.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            var task2 = new TaskItem
            {
                ProjectId = project.Id,
                Title = "Criar dashboard inicial",
                Description = "Montar indicadores, cards e filtros para acompanhar progresso do projeto.",
                Status = TaskStatus.Pending,
                Priority = TaskPriority.Medium,
                DueDate = DateTime.UtcNow.AddDays(5),
                AssigneeId = adminUser.Id,
                CreatedById = adminUser.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            var task3 = new TaskItem
            {
                ProjectId = project.Id,
                Title = "Validar acesso de usuários",
                Description = "Confirmar permissões por perfil e políticas de autenticação.",
                Status = TaskStatus.Completed,
                Priority = TaskPriority.Low,
                AssigneeId = adminUser.Id,
                CreatedById = adminUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
            };

            context.Tasks.AddRange(task1, task2, task3);

            context.AuditLogs.Add(new AuditLog
            {
                EntityType = "Project",
                EntityId = project.Id,
                Action = "Create",
                Details = "Projeto inicial criado para o lançamento do sistema.",
                PerformedById = adminUser.Id,
                PerformedAt = DateTime.UtcNow,
            });

            await context.SaveChangesAsync();
        }
    }
}
