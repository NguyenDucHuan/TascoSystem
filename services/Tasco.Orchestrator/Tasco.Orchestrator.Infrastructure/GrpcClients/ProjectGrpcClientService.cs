using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Tasco.ProjectService.Service.Services.GRpcService;
using static Tasco.ProjectService.Service.Services.GRpcService.Project;

namespace Tasco.Orchestrator.Infrastructure.GrpcClients
{
    public class ProjectGrpcClientService
    {
        private readonly Project.ProjectClient _projectClient;
        private readonly ILogger<ProjectGrpcClientService> _logger;

        public ProjectGrpcClientService(Project.ProjectClient projectClient, ILogger<ProjectGrpcClientService> logger)
        {
            _projectClient = projectClient;
            _logger = logger;
        }

        /// <summary>
        /// Lấy thông tin dự án theo ID
        /// </summary>
        public async Task<ProjectResponse> GetProjectByIdAsync(string projectId, string userId)
        {
            try
            {
                _logger.LogInformation("Calling GetProjectById for project {ProjectId}, user {UserId}", projectId, userId);

                var request = new GetProjectByIdRequest
                {
                    ProjectId = projectId,
                    UserId = userId
                };

                var response = await _projectClient.GetProjectByIdAsync(request);

                _logger.LogInformation("GetProjectById completed successfully for project {ProjectId}", projectId);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "gRPC error occurred while getting project by ID");
                throw new Exception($"gRPC error: {ex.Status.Detail}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while getting project by ID");
                throw;
            }
        }

        /// <summary>
        /// Tạo dự án mới
        /// </summary>
        public async Task<BoolResponse> CreateProjectAsync(string name, string description, string ownerId, string ownerName, string ownerEmail)
        {
            try
            {
                _logger.LogInformation("Calling CreateProject with name {Name}, owner {OwnerId}", name, ownerId);

                var request = new CreateProjectRequest
                {
                    Name = name,
                    OwnerEmail = ownerEmail,
                    OwnerName = ownerName,
                    Description = description,
                    OwnerId = ownerId
                };

                var response = await _projectClient.CreateProjectAsync(request);

                _logger.LogInformation("CreateProject completed with success: {Success}", response.Data);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "gRPC error occurred while creating project");
                return new BoolResponse
                {
                    Data = false,
                    Message = $"gRPC error: {ex.Status.Detail}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while creating project");
                return new BoolResponse
                {
                    Data = false,
                    Message = "Internal server error"
                };
            }
        }

        /// <summary>
        /// Cập nhật dự án
        /// </summary>
        public async Task<BoolResponse> UpdateProjectAsync(string id, string name, string description, string updateBy)
        {
            try
            {
                _logger.LogInformation("Calling UpdateProject for project {ProjectId}, updated by {UpdateBy}", id, updateBy);

                var request = new UpdateProjectRequest
                {
                    Id = id,
                    Name = name,
                    Description = description,
                    UpdateBy = updateBy
                };

                var response = await _projectClient.UpdateProjectAsync(request);

                _logger.LogInformation("UpdateProject completed with success: {Success}", response.Data);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "gRPC error occurred while updating project");
                return new BoolResponse
                {
                    Data = false,
                    Message = $"gRPC error: {ex.Status.Detail}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while updating project");
                return new BoolResponse
                {
                    Data = false,
                    Message = "Internal server error"
                };
            }
        }

        /// <summary>
        /// Xóa dự án
        /// </summary>
        public async Task<BoolResponse> DeleteProjectAsync(string projectId, string userId)
        {
            try
            {
                _logger.LogInformation("Calling DeleteProject for project {ProjectId}, user {UserId}", projectId, userId);

                var request = new GetProjectByIdRequest
                {
                    ProjectId = projectId,
                    UserId = userId
                };

                var response = await _projectClient.DeleteProjectAsync(request);

                _logger.LogInformation("DeleteProject completed with success: {Success}", response.Data);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "gRPC error occurred while deleting project");
                return new BoolResponse
                {
                    Data = false,
                    Message = $"gRPC error: {ex.Status.Detail}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while deleting project");
                return new BoolResponse
                {
                    Data = false,
                    Message = "Internal server error"
                };
            }
        }

        /// <summary>
        /// Lấy danh sách dự án cho thành viên
        /// </summary>
        public async Task<PageProjectResponse> GetProjectForMemberAsync(string userId, string role, int pageSize, int pageNumber, string search = "", bool isDelete = false)
        {
            try
            {
                _logger.LogInformation("Calling GetProjectForMember for user {UserId}, role {Role}, page {PageNumber}", userId, role, pageNumber);

                var request = new GetProjectBymenberPage
                {
                    UserId = userId,
                    Role = role,
                    PageSize = pageSize,
                    PageNumber = pageNumber,
                    Search = search ?? "",
                    IsDelete = isDelete
                };

                var response = await _projectClient.GetProjectForManberAsync(request);

                _logger.LogInformation("GetProjectForMember completed successfully, returned {Count} projects", response.Projects.Count);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "gRPC error occurred while getting projects for member");
                return new PageProjectResponse
                {
                    TotalPage = 0,
                    PageSize = pageSize,
                    PageNumber = pageNumber,
                    TotalCount = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while getting projects for member");
                return new PageProjectResponse
                {
                    TotalPage = 0,
                    PageSize = pageSize,
                    PageNumber = pageNumber,
                    TotalCount = 0
                };
            }
        }

        /// <summary>
        /// Lay danh sách dự án để apply
        /// <summary>
        public async Task<PageProjectResponse> GetProjectForApplyAsync(int pageSize, int pageNumber, string search = "", bool isDelete = false)
        {
            try
            {
                _logger.LogInformation("Calling GetProjectForApply for page {PageNumber}", pageNumber);
                var request = new SearchProjectsRequest
                {
                    IncludeDeleted = isDelete,
                    PageSize = pageSize,
                    PageNumber = pageNumber,
                    SearchTerm = search ?? ""
                };
                var response = await _projectClient.GetPageProjectsAsync(request);
                _logger.LogInformation("GetProjectForApply completed successfully, returned {Count} projects", response.Projects.Count);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "gRPC error occurred while getting projects for apply");
                return new PageProjectResponse
                {
                    TotalPage = 0,
                    PageSize = pageSize,
                    PageNumber = pageNumber,
                    TotalCount = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while getting projects for apply");
                return new PageProjectResponse
                {
                    TotalPage = 0,
                    PageSize = pageSize,
                    PageNumber = pageNumber,
                    TotalCount = 0
                };
            }
        }
    }
}