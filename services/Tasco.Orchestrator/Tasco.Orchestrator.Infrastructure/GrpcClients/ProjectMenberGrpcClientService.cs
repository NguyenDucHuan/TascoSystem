using Microsoft.Extensions.Logging;
using Tasco.ProjectService.Service.Protos;
using Grpc.Core;


namespace Tasco.Orchestrator.Infrastructure.GrpcClients
{
    public class ProjectMenberGrpcClientService
    {
        private readonly ProjectMemberService.ProjectMemberServiceClient _projectMemberClient;
        private readonly ILogger<ProjectMenberGrpcClientService> _logger;

        public ProjectMenberGrpcClientService(ProjectMemberService.ProjectMemberServiceClient projectMemberClient, ILogger<ProjectMenberGrpcClientService> logger)
        {
            _projectMemberClient = projectMemberClient;
            _logger = logger;
        }

        /// <summary>
        /// Xóa thành viên khỏi dự án
        /// </summary>
        public async Task<RemoveMemberResponse> RemoveMemberFromProjectAsync(string projectId, string memberId, string ownerId)
        {
            try
            {
                _logger.LogInformation("Calling RemoveMemberFromProject for project {ProjectId}, member {MemberId}", projectId, memberId);

                var request = new RemoveMemberRequest
                {
                    ProjectId = projectId,
                    MemberId = memberId,
                    OwnerId = ownerId
                };

                var response = await _projectMemberClient.RemoveMemberFromProjectAsync(request);

                _logger.LogInformation("RemoveMemberFromProject completed with success: {Success}", response.Success);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "gRPC error occurred while removing member from project");
                return new RemoveMemberResponse
                {
                    Success = false,
                    Message = $"gRPC error: {ex.Status.Detail}",
                    StatusCode = (int)ex.Status.StatusCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while removing member from project");
                return new RemoveMemberResponse
                {
                    Success = false,
                    Message = "Internal server error",
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// Cập nhật trạng thái phê duyệt thành viên
        /// </summary>
        public async Task<UpdateApprovedStatusResponse> UpdateApprovedStatusAsync(string projectId, string memberId, string userName, string userEmail, string approvedStatus, string ownerId = null)
        {
            try
            {
                _logger.LogInformation("Calling UpdateApprovedStatus for project {ProjectId}, member {MemberId}, status {Status}",
                    projectId, memberId, approvedStatus);

                var request = new UpdateApprovedStatusRequest
                {
                    ProjectId = projectId,
                    MemberId = memberId,
                    UserGmail = userEmail,
                    UserName = userName,
                    ApprovedStatus = approvedStatus.ToUpper(),
                    OwnerId = ownerId
                };

                if (!string.IsNullOrEmpty(ownerId))
                {
                    request.OwnerId = ownerId;
                }

                var response = await _projectMemberClient.UpdateApprovedStatusAsync(request);

                _logger.LogInformation("UpdateApprovedStatus completed with success: {Success}", response.Success);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "gRPC error occurred while updating approved status");
                return new UpdateApprovedStatusResponse
                {
                    Success = false,
                    Message = $"gRPC error: {ex.Status.Detail}",
                    StatusCode = (int)ex.Status.StatusCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while updating approved status");
                return new UpdateApprovedStatusResponse
                {
                    Success = false,
                    Message = "Internal server error",
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// Cập nhật vai trò thành viên
        /// </summary>
        public async Task<UpdateMemberRoleResponse> UpdateMemberRoleAsync(string projectId, string memberId, string role, string ownerId = null)
        {
            try
            {
                _logger.LogInformation("Calling UpdateMemberRole for project {ProjectId}, member {MemberId}, role {Role}",
                    projectId, memberId, role);

                var request = new UpdateMemberRoleRequest
                {
                    ProjectId = projectId,
                    MemberId = memberId,
                    Role = role.ToUpper()
                };

                if (!string.IsNullOrEmpty(ownerId))
                {
                    request.OwnerId = ownerId;
                }

                var response = await _projectMemberClient.UpdateMemberRoleAsync(request);

                _logger.LogInformation("UpdateMemberRole completed with success: {Success}", response.Success);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "gRPC error occurred while updating member role");
                return new UpdateMemberRoleResponse
                {
                    Success = false,
                    Message = $"gRPC error: {ex.Status.Detail}",
                    StatusCode = (int)ex.Status.StatusCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while updating member role");
                return new UpdateMemberRoleResponse
                {
                    Success = false,
                    Message = "Internal server error",
                    StatusCode = 500
                };
            }
        }
    }
}
