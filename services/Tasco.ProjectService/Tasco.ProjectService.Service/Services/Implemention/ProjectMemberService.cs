using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasco.ProjectService.Repository.UnitOfWork;
using Tasco.ProjectService.Service.Helper;
using Tasco.ProjectService.Service.Services.Interface;
using Tasco.ProjectService.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Tasco.ProjectService.Service.Constant;
namespace Tasco.ProjectService.Service.Services.Implemention
{
    public class ProjectMemberService : IProjectMemberService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        public ProjectMemberService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<MethodResult<bool>> RemoveMemberFromProjectAsync(Guid projectId, Guid memberId, Guid owner)
        {
            try
            {
                var existingMember = await _uow.GetRepository<ProjectMember>().SingleOrDefaultAsync(
                    predicate: x => x.ProjectId == projectId && x.UserId == memberId);
                var ownerMenber = await _uow.GetRepository<ProjectMember>().SingleOrDefaultAsync(
                    predicate: x => x.ProjectId == projectId && x.UserId == owner && x.Role == MenberRoleConstants.Owner);
                if (existingMember == null)
                {
                    return new MethodResult<bool>.Failure("Member not found in the project.", 404);
                }
                if (ownerMenber == null)
                {
                    return new MethodResult<bool>.Failure("Owner not found in the project.", 404);
                }

                existingMember.Status = MenberRoleConstants.RemovedStatus;
                existingMember.RemoveDate = DateTime.UtcNow;
                existingMember.RemovedBy = owner;

                if (existingMember.Role == MenberRoleConstants.Owner)
                {
                    return new MethodResult<bool>.Failure("Cannot remove the project owner.", 400);
                }
                _uow.GetRepository<ProjectMember>().UpdateAsync(existingMember);
                await _uow.CommitAsync();
                return new MethodResult<bool>.Success(true, "Đã xóa thành viên khỏi dự án thành công.");

            }
            catch
            {
                return new MethodResult<bool>.Failure("An error occurred while removing member from project.", 500);
            }

        }

        public async Task<MethodResult<bool>> UpdateApprovedStatusAsync(Guid projectId, Guid memberId, string ApprovedStatus, Guid? owner, string? gmail, string? userName)
        {
            try
            {
                var exisProject = await _uow.GetRepository<Project>().SingleOrDefaultAsync(
                    predicate: x => x.Id == projectId);

                if (exisProject == null)
                {
                    return new MethodResult<bool>.Failure("Project not found.", 404);
                }
                if (ApprovedStatus != MenberRoleConstants.ApprovedStatus && ApprovedStatus != MenberRoleConstants.PendingStatus && ApprovedStatus != MenberRoleConstants.RejectedStatus && ApprovedStatus != MenberRoleConstants.RemovedStatus)
                {
                    return new MethodResult<bool>.Failure("Invalid Status.", 400);
                }
                else
                {
                    var existingMember = await _uow.GetRepository<ProjectMember>().SingleOrDefaultAsync(
                        predicate: x => x.ProjectId == projectId && x.UserId == memberId);

                    if (ApprovedStatus == MenberRoleConstants.PendingStatus)
                    {
                        if (existingMember != null)
                        {
                            return new MethodResult<bool>.Failure("You Have approved to Project", 404);
                        }
                        var newMember = new ProjectMember
                        {
                            ProjectId = projectId,
                            UserId = memberId,
                            UserName = userName,
                            UserEmail = gmail,
                            Role = MenberRoleConstants.Member,
                            Status = MenberRoleConstants.PendingStatus,
                            ApprovedUpdateDate = DateTime.UtcNow
                        };
                        await _uow.GetRepository<ProjectMember>().InsertAsync(newMember);
                        await _uow.CommitAsync();
                        return new MethodResult<bool>.Success(true, "You success to appoved project successfully.");
                    }
                    else if (existingMember != null && existingMember.Status != MenberRoleConstants.RemovedStatus)
                    {
                        if (exisProject.OwnerId != owner)
                        {
                            return new MethodResult<bool>.Failure("You are not the owner of the project.", 404);
                        }
                        if (existingMember == null)
                        {
                            return new MethodResult<bool>.Failure("Member not found in approved list the project.", 404);
                        }

                        existingMember.Status = ApprovedStatus;
                        if (ApprovedStatus != MenberRoleConstants.RemovedStatus)
                        {
                            existingMember.ApprovedUpdateDate = DateTime.UtcNow;
                            existingMember.ApprovedBy = owner;
                            _uow.GetRepository<ProjectMember>().UpdateAsync(existingMember);
                            await _uow.CommitAsync();
                            return new MethodResult<bool>.Success(true, "Member Approved updated successfully.");
                        }
                        else
                        {
                            existingMember.RemoveDate = DateTime.UtcNow;
                            existingMember.RemovedBy = owner;
                            _uow.GetRepository<ProjectMember>().UpdateAsync(existingMember);
                            await _uow.CommitAsync();
                            return new MethodResult<bool>.Success(true, "Member Removed successfully.");
                        }
                    }
                }
                return new MethodResult<bool>.Failure("An error occurred while accessing member to project.", 500);
            }
            catch
            {
                return new MethodResult<bool>.Failure("An error occurred while accessing member to project.", 500);
            }
        }

        public async Task<MethodResult<bool>> UpdateMemberRoleAsync(Guid projectId, Guid memberId, string role, Guid? owner)
        {
            try
            {
                var exisProject = await _uow.GetRepository<Project>().SingleOrDefaultAsync(
                    predicate: x => x.Id == projectId);

                if (exisProject == null)
                {
                    return new MethodResult<bool>.Failure("Project not found.", 404);
                }

                if (exisProject.OwnerId != owner)
                {
                    return new MethodResult<bool>.Failure("You are not the owner of the project.", 404);
                }

                var existingMember = await _uow.GetRepository<ProjectMember>().SingleOrDefaultAsync(
                    predicate: x => x.ProjectId == projectId && x.UserId == memberId);

                if (existingMember == null)
                {
                    return new MethodResult<bool>.Failure("Member not found in the project.", 404);
                }

                if (role != MenberRoleConstants.Owner && role != MenberRoleConstants.Member)
                {
                    return new MethodResult<bool>.Failure("Invalid role.", 400);
                }

                existingMember.Role = role;
                existingMember.ApprovedUpdateDate = DateTime.UtcNow;
                existingMember.ApprovedBy = owner;

                _uow.GetRepository<ProjectMember>().UpdateAsync(existingMember);
                await _uow.CommitAsync();

                return new MethodResult<bool>.Success(true, "Member role updated successfully.");
            }
            catch
            {
                return new MethodResult<bool>.Failure("An error occurred while updating member role.", 500);
            }
        }
    }
}
