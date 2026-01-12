using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Tasco.ProjectService.Repository.Entities;
using Tasco.ProjectService.Repository.Paginate;
using Tasco.ProjectService.Repository.UnitOfWork;
using Tasco.ProjectService.Service.BussinessModel.ProjectBussinessModel;
using Tasco.ProjectService.Service.Helper;
using Tasco.ProjectService.Service.Services.Interface;
using Tasco.ProjectService.Service.Constant;
using System.Linq.Expressions;
using Tasco.Shared.Notifications.Models;

namespace Tasco.ProjectService.Service.Services.Implemention
{
    public class ProjectService : IProjectService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        public ProjectService(IUnitOfWork uow, IMapper mapper, INotificationService notificationService)
        {
            _uow = uow;
            _mapper = mapper;
            _notificationService = notificationService;
        }


        public async Task<MethodResult<bool>> CreateProjectAsync(string name, string? description, Guid ownerId, string ownerName, string ownerEmail)
        {
            try
            {
                await _uow.BeginTransactionAsync();
                var existingProject = await _uow.GetRepository<Project>().SingleOrDefaultAsync(
                    predicate: x => x.Name == name && x.OwnerId == ownerId);
                if (existingProject != null)
                {
                    return new MethodResult<bool>.Failure("Project with the same name already exists for this owner.", 400);
                }
                var newProject = new Project
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Description = description,
                    OwnerId = ownerId,
                    CreatedAt = DateTime.UtcNow,
                    UpdateBy = ownerId,
                    UpdatedAt = DateTime.UtcNow
                };
                await _uow.GetRepository<Project>().InsertAsync(newProject);
                await _uow.CommitAsync();
                var projectMember = new ProjectMember
                {
                    UserId = ownerId,
                    ProjectId = newProject.Id,
                    UserName = ownerName,
                    UserEmail = ownerEmail,
                    Role = MenberRoleConstants.Owner,
                    Status = MenberRoleConstants.ApprovedStatus,
                    ApprovedUpdateDate = DateTime.UtcNow
                };
                await _uow.GetRepository<ProjectMember>().InsertAsync(projectMember);
                await _uow.CommitAsync();
                await _uow.CommitTransactionAsync();
                return new MethodResult<bool>.Success(true, "Dự án đã được tạo thành công.");
            }
            catch
            {
                return new MethodResult<bool>.Failure("An error occurred while creating the project.", 500);
            }
        }

        public async Task<MethodResult<bool>> DeleteProjectAsync(Guid projectId, Guid DeleteBy)
        {
            try
            {
                var project = await _uow.GetRepository<Project>().SingleOrDefaultAsync(
                    predicate: x => x.Id == projectId);
                if (project == null)
                {
                    return new MethodResult<bool>.Failure("Project not found.", 404);
                }
                // Check if the project has members
                project.IsDeleted = true;
                project.DeletedBy = DeleteBy;
                project.DeletedAt = DateTime.UtcNow;
                // Delete the project
                _uow.GetRepository<Project>().UpdateAsync(project);
                await _uow.CommitAsync();
                return new MethodResult<bool>.Success(true, "Dự án đã được xóa thành công.");

            }
            catch
            {
                return new MethodResult<bool>.Failure("An error occurred while deleting the project.", 500);
            }

        }

        public async Task<MethodResult<IPaginate<ProjectResponse>>> GetProjectsAsync(int page = 1, int size = 10, string search = "", bool isdeleted = false)
        {
            try
            {
                var projects = await _uow.GetRepository<Project>()
                    .GetPagingListAsync(
                    selector: i => _mapper.Map<ProjectResponse>(i),
                    include: x => x.Include(p => p.Members),
                    page: page,
                    size: size,
                    predicate: x => (string.IsNullOrEmpty(search) || x.Name.Contains(search)) && x.IsDeleted == isdeleted
                  );

                return new MethodResult<IPaginate<ProjectResponse>>.Success(projects);
            }
            catch (Exception ex)
            {
                return new MethodResult<IPaginate<ProjectResponse>>.Failure($"An error occurred while retrieving projects: {ex.Message}", 500);
            }
        }

        public async Task<MethodResult<ProjectResponse>> GetProjectByIdAsync(Guid projectId)
        {
            try
            {
                var project = await _uow.GetRepository<Project>().SingleOrDefaultAsync(
                    predicate: x => x.Id == projectId,
                    selector: i => _mapper.Map<ProjectResponse>(i),
                    include: x => x.Include(p => p.Members));
                if (project == null)
                {
                    return new MethodResult<ProjectResponse>.Failure("Project not found.", 404);
                }
                return new MethodResult<ProjectResponse>.Success(project);
            }
            catch
            {
                return new MethodResult<ProjectResponse>.Failure("An error occurred while retrieving the project.", 500);
            }

        }

        public async Task<MethodResult<bool>> UpdateProjectAsync(Guid projectId, string? name, string? description, Guid updateBy)
        {
            try
            {
                var project = await _uow.GetRepository<Project>().SingleOrDefaultAsync(
                    predicate: x => x.Id == projectId);

                if (project == null)
                {
                    return new MethodResult<bool>.Failure("Project not found.", 404);
                }
                var changes = new List<string>();
                if (!string.IsNullOrEmpty(name))
                {
                    var existingProject = await _uow.GetRepository<Project>().SingleOrDefaultAsync(
                        predicate: x => x.Name == name && x.OwnerId == project.OwnerId);
                    if (existingProject != null && existingProject.Id != projectId)
                    {
                        return new MethodResult<bool>.Failure("Project with the same name already exists for this owner.", 400);
                    }
                    if (project.Name != name)
                        changes.Add($"Tên dự án: '{project.Name}' → '{name}'");
                    project.Name = name;
                }
                if (!string.IsNullOrEmpty(description))
                {
                    project.Description = description;
                    if (project.Description != name)
                        changes.Add($"Tên dự án: '{project.Description}' → '{description}'");
                }
                project.UpdateBy = updateBy;
                project.UpdatedAt = DateTime.UtcNow;
                _uow.GetRepository<Project>().UpdateAsync(project);
                await _uow.CommitAsync();

                var projectMenber = await _uow.GetRepository<ProjectMember>().SingleOrDefaultAsync(
                    predicate: x => x.ProjectId == projectId && x.UserId != updateBy && x.Status == MenberRoleConstants.ApprovedStatus);
                var UpdateByUser = await _uow.GetRepository<ProjectMember>().SingleOrDefaultAsync(
                    predicate: x => x.UserId == updateBy && x.ProjectId == projectId);
                var changesList = string.Join("\n• ", changes);
                var detailedMessage = $"📋 Các thay đổi:\n• {changesList}";
                foreach (var member in project.Members)
                {

                    var message = new NotificationMessage
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = Guid.NewGuid().ToString(),
                        Title = "📋 Cập nhật dự án mới",
                        Message = $"{UpdateByUser.UserName}: \"vừa cập nhật dự án {project.Name} 👏\"",
                        Type = NotificationType.ProjectUpdated,
                        ProjectId = projectId.ToString(),
                        TaskId = Guid.NewGuid().ToString(),
                        Priority = NotificationPriority.Normal,
                        Channels = new List<NotificationChannel> { NotificationChannel.Email },
                        CreatedAt = DateTime.UtcNow,
                        Metadata = new Dictionary<string, object>
                    {
                        { "assignee", $"{member.UserName}" },
                        { "author", $"{UpdateByUser.UserName} - {UpdateByUser.Role}" },
                        { "Action", $"Project have been updated by {UpdateByUser.UserName} " },
                        { "changes", changes },
                        { "changesText", detailedMessage },
                        { "updateTime", project.UpdatedAt },
                        { "email", $"{member.UserEmail}" }
                    }
                    };

                    await _notificationService.SendNotificationAsync(message);
                }


                return new MethodResult<bool>.Success(true, "Update project success!");

            }
            catch
            {
                return new MethodResult<bool>.Failure("An error occurred while updating the project.", 500);
            }

        }

        public async Task<MethodResult<IPaginate<ProjectResponse>>> GetProjectForMenber(Guid memberId, string? role, int page = 1, int size = 10, string search = "", bool isdeleted = false)
        {
            try
            {
                Expression<Func<Project, bool>> predicate = x =>
                    x.Members.Any(m => m.UserId == memberId && (string.IsNullOrEmpty(role) || m.Role == role)) &&
                    (string.IsNullOrEmpty(search) || x.Name.Contains(search)) &&
                    x.IsDeleted == isdeleted;
                var ProjectMenberIn = await _uow.GetRepository<Project>()
                    .GetPagingListAsync(
                        selector: i => _mapper.Map<ProjectResponse>(i),
                        predicate: predicate,
                        include: x => x.Include(p => p.Members),
                        page: page,
                        size: size);

                return new MethodResult<IPaginate<ProjectResponse>>.Success(ProjectMenberIn);

            }
            catch
            {
                return new MethodResult<IPaginate<ProjectResponse>>.Failure("An error occurred while retrieving projects from member.", 500);
            }
        }

    }
}
