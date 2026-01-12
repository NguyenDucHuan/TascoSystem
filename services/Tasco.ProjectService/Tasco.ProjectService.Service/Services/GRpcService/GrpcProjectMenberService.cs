using Microsoft.Extensions.Logging;
using Tasco.ProjectService.Service.Protos;
using Tasco.ProjectService.Service.Services.Interface;
using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace Tasco.ProjectService.Service.Services.GRpcService
{
    public class GrpcProjectMenberService : ProjectMemberService.ProjectMemberServiceBase
    {
        private readonly IProjectMemberService _projectMemberService;
        private readonly ILogger<GrpcProjectMenberService> _logger;
        public GrpcProjectMenberService(IProjectMemberService projectMemberService, ILogger<GrpcProjectMenberService> logger)
        {
            _projectMemberService = projectMemberService;
            _logger = logger;
        }

        public override async Task<RemoveMemberResponse> RemoveMemberFromProject(RemoveMemberRequest request, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation("Removing member {MemberId} from project {ProjectId}", request.MemberId, request.ProjectId);

                var projectId = Guid.Parse(request.ProjectId);
                var memberId = Guid.Parse(request.MemberId);
                var ownerId = Guid.Parse(request.OwnerId);


                var result = await _projectMemberService.RemoveMemberFromProjectAsync(projectId, memberId, ownerId);

                return result.Match(
                    (errorMessage, statusCode) => new RemoveMemberResponse
                    {
                        Success = false,
                        Message = errorMessage,
                        StatusCode = statusCode
                    },
                    (data, message) => new RemoveMemberResponse
                    {
                        Success = data,
                        Message = message,
                        StatusCode = 200
                    });
            }
            catch (FormatException)
            {
                _logger.LogError("Invalid GUID format in RemoveMemberFromProject request");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ID format"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error removing member from project");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        public override async Task<UpdateApprovedStatusResponse> UpdateApprovedStatus(UpdateApprovedStatusRequest request, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation("Updating approved status for member {MemberId} in project {ProjectId} to {Status}",
                    request.MemberId, request.ProjectId, request.ApprovedStatus);

                var projectId = Guid.Parse(request.ProjectId);
                var memberId = Guid.Parse(request.MemberId);
                Guid? ownerId = null;

                if (!string.IsNullOrEmpty(request.OwnerId))
                {
                    ownerId = Guid.Parse(request.OwnerId);
                }

                var result = await _projectMemberService.UpdateApprovedStatusAsync(projectId, memberId, request.ApprovedStatus, ownerId, request.UserGmail, request.UserName);

                return result.Match(
                    (errorMessage, statusCode) => new UpdateApprovedStatusResponse
                    {
                        Success = false,
                        Message = errorMessage,
                        StatusCode = statusCode
                    },
                    (data, message) => new UpdateApprovedStatusResponse
                    {
                        Success = data,
                        Message = message,
                        StatusCode = 200
                    });
            }
            catch (FormatException)
            {
                _logger.LogError("Invalid GUID format in UpdateApprovedStatus request");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ID format"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating approved status");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        public override async Task<UpdateMemberRoleResponse> UpdateMemberRole(UpdateMemberRoleRequest request, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation("Updating role for member {MemberId} in project {ProjectId} to {Role}",
                    request.MemberId, request.ProjectId, request.Role);

                var projectId = Guid.Parse(request.ProjectId);
                var memberId = Guid.Parse(request.MemberId);
                Guid? ownerId = null;

                if (!string.IsNullOrEmpty(request.OwnerId))
                {
                    ownerId = Guid.Parse(request.OwnerId);
                }

                var result = await _projectMemberService.UpdateMemberRoleAsync(projectId, memberId, request.Role, ownerId);

                return result.Match(
                    (errorMessage, statusCode) => new UpdateMemberRoleResponse
                    {
                        Success = false,
                        Message = errorMessage,
                        StatusCode = statusCode
                    },
                    (data, message) => new UpdateMemberRoleResponse
                    {
                        Success = data,
                        Message = message,
                        StatusCode = 200
                    });
            }
            catch (FormatException)
            {
                _logger.LogError("Invalid GUID format in UpdateMemberRole request");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ID format"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating member role");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }
    }
}
