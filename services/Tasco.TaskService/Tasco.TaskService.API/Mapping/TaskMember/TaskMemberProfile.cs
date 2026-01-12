using AutoMapper;
using Tasco.TaskService.API.Protos;
using Tasco.TaskService.Service.BusinessModels;
using Tasco.TaskService.Repository.Entities;
using Tasco.TaskService.Repository.Paginate;

namespace Tasco.TaskService.API.Mapping.TaskMember
{
    public class TaskMemberProfile : Profile
    {
        public TaskMemberProfile()
        {
            CreateMap<TaskMemberRequest, TaskMemberBusinessModel>()
                .ForMember(dest => dest.WorkTaskId, opt => opt.ConvertUsing(new StringToGuidConverter(), src => src.WorkTaskId))
                .ForMember(dest => dest.UserId, opt => opt.ConvertUsing(new StringToGuidConverter(), src => src.UserId))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.AssignedByUserId, opt => opt.ConvertUsing(new StringToGuidConverter(), src => src.AssignedByUserId))
                .ForMember(dest => dest.AssignedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            CreateMap<UpdateTaskMemberRequest, TaskMemberBusinessModel>()
                .ForMember(dest => dest.WorkTaskId, opt => opt.ConvertUsing(new StringToGuidConverter(), src => src.Member.WorkTaskId))
                .ForMember(dest => dest.UserId, opt => opt.ConvertUsing(new StringToGuidConverter(), src => src.Member.UserId))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Member.Role))
                .ForMember(dest => dest.AssignedByUserId, opt => opt.ConvertUsing(new StringToGuidConverter(), src => src.Member.AssignedByUserId))
                .ForMember(dest => dest.AssignedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            CreateMap<TaskMemberBusinessModel, Repository.Entities.TaskMember>()
                .ForMember(dest => dest.WorkTaskId, opt => opt.MapFrom(src => src.WorkTaskId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.AssignedByUserId, opt => opt.MapFrom(src => src.AssignedByUserId))
                .ForMember(dest => dest.AssignedDate, opt => opt.MapFrom(src => src.AssignedDate))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            CreateMap<Repository.Entities.TaskMember, TaskMemberBusinessModel>();

            CreateMap<Repository.Entities.TaskMember, TaskMemberResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.WorkTaskId, opt => opt.MapFrom(src => src.WorkTaskId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.UserEmail ?? ""))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.AssignedDate, opt => opt.MapFrom(src => src.AssignedDate.ToString("yyyy-MM-ddTHH:mm:ssZ")))
                .ForMember(dest => dest.AssignedByUserId, opt => opt.MapFrom(src => src.AssignedByUserId.ToString()));

            CreateMap<Paginate<Repository.Entities.TaskMember>, TaskMemberListResponse>()
                .ForMember(dest => dest.TaskMembers, opt => opt.MapFrom(src => src.Items))
                .ForMember(dest => dest.TotalCount, opt => opt.MapFrom(src => src.Total))
                .ForMember(dest => dest.PageCount, opt => opt.MapFrom(src => src.TotalPages))
                .ForMember(dest => dest.CurrentPage, opt => opt.MapFrom(src => src.Page));

            CreateMap<Paginate<TaskMemberBusinessModel>, TaskMemberListResponse>()
                .ForMember(dest => dest.TaskMembers, opt => opt.MapFrom(src => src.Items))
                .ForMember(dest => dest.TotalCount, opt => opt.MapFrom(src => src.Total))
                .ForMember(dest => dest.PageCount, opt => opt.MapFrom(src => src.TotalPages))
                .ForMember(dest => dest.CurrentPage, opt => opt.MapFrom(src => src.Page));
        }
    }

    public class StringToGuidConverter : IValueConverter<string, Guid>
    {
        public Guid Convert(string sourceMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(sourceMember))
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Empty GUID value encountered");
                return Guid.Empty;
            }
            
            if (!Guid.TryParse(sourceMember, out var guid))
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Invalid GUID format: {sourceMember}");
                return Guid.Empty;
            }
            
            return guid;
        }
    }
} 