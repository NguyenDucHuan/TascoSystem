using AutoMapper;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Tasco.ProjectService.Service.Services.Interface;
using Tasco.Shared.Notifications.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Tasco.ProjectService.Service.Services.GRpcService
{
    public class GrpcProjectService : Project.ProjectBase
    {
        private readonly IProjectService _projectService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<GrpcProjectService> _logger;

        public GrpcProjectService(IProjectService projectService, INotificationService notificationService, ILogger<GrpcProjectService> logger)
        {
            _projectService = projectService;
            _notificationService = notificationService;
            _logger = logger;
            _logger.LogInformation("GrpcProjectService constructor called");
        }
        public override async Task<ProjectResponse> GetProjectById(GetProjectByIdRequest request, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation("Getting project by ID: {ProjectId}", request.ProjectId);

                var result = await _projectService.GetProjectByIdAsync(Guid.Parse(request.ProjectId));

                return result.Match(
                    (errorMessage, statusCode) =>
                    {
                        _logger.LogError("Failed to get project by ID: {Error}", errorMessage);
                        throw new RpcException(new Status(
                            statusCode == 404 ? StatusCode.NotFound : StatusCode.Internal,
                            errorMessage
                        ));
                    },
                    (data, message) =>
                    {
                        return new ProjectResponse
                        {
                            Id = data.Id.ToString(),
                            Name = data.Name,
                            Description = data.Description ?? "",
                            OwnerId = data.OwnerId.ToString(),
                            CreatedAt = data.CreatedAt.ToString("o"),
                            UpdateBy = data.UpdateBy.ToString(),
                            UpdatedAt = data.UpdatedAt.ToString("o"),
                            IsDeleted = data.IsDeleted,
                            DeletedAt = data.DeletedAt?.ToString("o") ?? "",
                            Members =
                            {
                                data.Members.Select(m => new ProjectMember
                                {
                                    UserId = m.UserId.ToString(),
                                    Role = m.Role,
                                    UserName = m.UserName ?? "",
                                    UserGmail = m.UserEmail ?? "",
                                    ApprovedStatus = m.ApprovedStatus,
                                    ApprovedUpdateDate = m.ApprovedUpdateDate.ToString("o"),
                                    IsRemoved = m.IsRemoved,
                                    RemoveDate = m.RemoveDate?.ToString("o") ?? ""
                                })
                            }
                        };
                    });
            }
            catch (FormatException)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid project ID format"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting project by ID");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }
        public override async Task<BoolResponse> UpdateProject(UpdateProjectRequest request, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation("Updating project: {ProjectId}", request.Id);

                var projectId = Guid.Parse(request.Id);
                var updateBy = Guid.Parse(request.UpdateBy);
                var result = await _projectService.UpdateProjectAsync(projectId, request.Name, request.Description, updateBy);

                return result.Match(
                    (errorMessage, statusCode) =>
                    {
                        _logger.LogError("Failed to update project: {Error}", errorMessage);
                        throw new RpcException(new Status(
                            statusCode == 404 ? StatusCode.NotFound :
                            statusCode == 400 ? StatusCode.InvalidArgument : StatusCode.Internal,
                            errorMessage
                        ));
                    },
                    (data, message) =>
                    {
                        _logger.LogInformation("Project updated successfully: {ProjectId}", request.Id);
                        return new BoolResponse { Data = data, Message = message };
                    });
            }
            catch (FormatException)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ID format"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating project");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }
        public override async Task<BoolResponse> DeleteProject(GetProjectByIdRequest request, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation("Deleting project: {ProjectId}", request.ProjectId);

                var projectId = Guid.Parse(request.ProjectId);
                var userID = Guid.Parse(request.UserId);
                var result = await _projectService.DeleteProjectAsync(projectId, userID);

                return result.Match(
                    (errorMessage, statusCode) =>
                    {
                        _logger.LogError("Failed to delete project: {Error}", errorMessage);
                        throw new RpcException(new Status(
                            statusCode == 404 ? StatusCode.NotFound :
                            statusCode == 403 ? StatusCode.PermissionDenied : StatusCode.Internal,
                            errorMessage
                        ));
                    },
                    (data, message) =>
                    {
                        _logger.LogInformation("Project deleted successfully: {ProjectId}", request.ProjectId);
                        return new BoolResponse { Data = data, Message = message };
                    });
            }
            catch (FormatException)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ID format"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting project");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }
        public override async Task<PageProjectResponse> GetProjectForManber(GetProjectBymenberPage request, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation("Getting projects for member: {UserId} with role: {Role}",
                    request.UserId, request.Role);

                var memberId = Guid.Parse(request.UserId);
                var result = await _projectService.GetProjectForMenber(memberId, request.Role, request.PageNumber, request.PageSize, request.Search, request.IsDelete);

                return result.Match(
                    (errorMessage, statusCode) =>
                    {
                        _logger.LogError("Failed to get projects for member: {Error}", errorMessage);
                        throw new RpcException(new Status(
                            statusCode == 404 ? StatusCode.NotFound : StatusCode.Internal,
                            errorMessage
                        ));
                    },
                    (data, message) =>
                    {
                        return new PageProjectResponse
                        {
                            TotalPage = data.TotalPages,
                            PageNumber = data.Page,
                            PageSize = data.Size,
                            TotalCount = data.Total,
                            Projects =
                            {
                                data.Items.Select(p => new ProjectResponse
                                {
                                    Id = p.Id.ToString(),
                                    Name = p.Name,
                                    Description = p.Description ?? "",
                                    OwnerId = p.OwnerId.ToString(),
                                    CreatedAt = p.CreatedAt.ToString("o"),
                                    UpdateBy = p.UpdateBy.ToString(),
                                    UpdatedAt = p.UpdatedAt.ToString("o"),
                                    IsDeleted = p.IsDeleted,
                                    DeletedAt = p.DeletedAt?.ToString("o") ?? "",
                                    Members =
                                    {
                                        p.Members.Select(m => new ProjectMember
                                        {
                                            UserId = m.UserId.ToString(),
                                            Role = m.Role,
                                            UserName = m.UserName ?? "",
                                            UserGmail = m.UserEmail ?? "",
                                            ApprovedStatus = m.ApprovedStatus,
                                            ApprovedUpdateDate = m.ApprovedUpdateDate.ToString("o"),
                                            IsRemoved = m.IsRemoved,
                                            RemoveDate = m.RemoveDate?.ToString("o") ?? ""
                                        })
                                    }
                                })
                            }
                        };
                    });
            }
            catch (FormatException)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user ID format"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting projects for member");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }
        public override async Task<BoolResponse> CreateProject(CreateProjectRequest request, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation("Creating new project: {ProjectName}", request.Name);

                var ownerId = Guid.Parse(request.OwnerId);
                var result = await _projectService.CreateProjectAsync(request.Name, request.Description, ownerId, request.OwnerName, request.OwnerEmail);

                return result.Match(
                    (errorMessage, statusCode) =>
                    {
                        _logger.LogError("Failed to create project: {Error}", errorMessage);
                        throw new RpcException(new Status(
                            statusCode == 400 ? StatusCode.InvalidArgument : StatusCode.Internal,
                            errorMessage
                        ));
                    },
                    (data, message) =>
                    {
                        _logger.LogInformation("Project created successfully: {ProjectId}", message);
                        return result.Match(
                         (errorMessage, statusCode) =>
                         {
                             _logger.LogError("Failed to create project: {Error}", errorMessage);
                             throw new RpcException(new Status(
                                 statusCode == 404 ? StatusCode.NotFound :
                                 statusCode == 403 ? StatusCode.PermissionDenied : StatusCode.Internal,
                                 errorMessage
                             ));
                         },
                         (data, message) =>
                         {
                             _logger.LogInformation("Project create successfully: {ProjectId}", message);
                             return new BoolResponse { Data = data, Message = message };
                         });
                    });
            }
            catch (FormatException)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid owner ID format"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating project");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }
        public override async Task<PageProjectResponse> GetPageProjects(SearchProjectsRequest request, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation("Getting projects with search: {Search}, Page: {Page}, Size: {Size}",
                    request.SearchTerm, request.PageNumber, request.PageSize);
                var result = await _projectService.GetProjectsAsync(request.PageNumber, request.PageSize, request.SearchTerm, request.IncludeDeleted);
                return result.Match(
                    (errorMessage, statusCode) =>
                    {
                        _logger.LogError("Failed to get projects: {Error}", errorMessage);
                        throw new RpcException(new Status(
                            statusCode == 404 ? StatusCode.NotFound : StatusCode.Internal,
                            errorMessage
                        ));
                    },
                    (data, message) =>
                    {
                        return new PageProjectResponse
                        {
                            TotalPage = data.TotalPages,
                            PageNumber = data.Page,
                            PageSize = data.Size,
                            TotalCount = data.Total,
                            Projects =
                            {
                                data.Items.Select(p => new ProjectResponse
                                {
                                    Id = p.Id.ToString(),
                                    Name = p.Name,
                                    Description = p.Description ?? "",
                                    OwnerId = p.OwnerId.ToString(),
                                    CreatedAt = p.CreatedAt.ToString("o"),
                                    UpdateBy = p.UpdateBy.ToString(),
                                    UpdatedAt = p.UpdatedAt.ToString("o"),
                                    IsDeleted = p.IsDeleted,
                                    DeletedAt = p.DeletedAt?.ToString("o") ?? "",
                                    Members =
                                    {
                                        p.Members.Select(m => new ProjectMember
                                        {
                                            UserId = m.UserId.ToString(),
                                            Role = m.Role,
                                            UserName = m.UserName ?? "",
                                            UserGmail = m.UserEmail ?? "",
                                            ApprovedStatus = m.ApprovedStatus,
                                            ApprovedUpdateDate = m.ApprovedUpdateDate.ToString("o"),
                                            IsRemoved = m.IsRemoved,
                                            RemoveDate = m.RemoveDate?.ToString("o") ?? ""
                                        })
                                    }
                                })
                            }
                        };
                    });
            }
            catch (FormatException)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid search parameters"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting projects");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

    }
}
