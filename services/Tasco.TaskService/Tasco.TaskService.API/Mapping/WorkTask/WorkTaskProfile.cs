using AutoMapper;
using Tasco.TaskService.API.Protos;
using Tasco.TaskService.Service.BusinessModels;
using Tasco.TaskService.API.Mapping;
using Tasco.TaskService.Repository.Entities;

namespace Tasco.TaskService.API.Mapping.WorkTask
{
    public class WorkTaskProfile : Profile
    {
        public WorkTaskProfile()
        {
            CreateMap<Protos.WorkTaskRequest, WorkTaskBusinessModel>()
                .ForMember(dest => dest.WorkAreaId, opt => opt.MapFrom(new GuidValueResolver<Protos.WorkTaskRequest>(src => src.WorkAreaId, "WorkAreaId")))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.StartDate) ? DateTime.Parse(src.StartDate) : (DateTime?)null))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.EndDate) ? DateTime.Parse(src.EndDate) : (DateTime?)null))
                .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.DueDate) ? DateTime.Parse(src.DueDate) : (DateTime?)null))
                .ForMember(dest => dest.CreatedByUserId, opt => opt.MapFrom(new GuidValueResolver<Protos.WorkTaskRequest>(src => src.CreatedByUserId, "UserId")))
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUserName))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<WorkTaskBusinessModel, Repository.Entities.WorkTask>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.WorkArea, opt => opt.Ignore());

            CreateMap<Repository.Entities.WorkTask, Protos.WorkTaskResponseUnique>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.WorkAreaId, opt => opt.MapFrom(src => src.WorkAreaId.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.HasValue ? src.StartDate.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ") : string.Empty))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.HasValue ? src.EndDate.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ") : string.Empty))
                .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.DueDate.HasValue ? src.DueDate.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ") : string.Empty))
                .ForMember(dest => dest.Progress, opt => opt.MapFrom(src => src.Progress))
                .ForMember(dest => dest.CreatedByUserId, opt => opt.MapFrom(src => src.CreatedByUserId.ToString()))
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUserName))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")));
            // Đã xóa dòng .ForMember(dest => dest.ModifiedDate, ...) vì WorkTask không có ModifiedDate

            CreateMap<Repository.Entities.WorkTask, Protos.WorkTaskResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.HasValue ? src.StartDate.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ") : string.Empty))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.HasValue ? src.EndDate.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ") : string.Empty))
                .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.DueDate.HasValue ? src.DueDate.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ") : string.Empty))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")))
                .ForMember(dest => dest.CompletedDate, opt => opt.MapFrom(src => src.CompletedDate.HasValue ? src.CompletedDate.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ") : string.Empty))
                .ForMember(dest => dest.CreatedByUserId, opt => opt.MapFrom(src => src.CreatedByUserId.ToString()))
                .ForMember(dest => dest.TaskActions, opt => opt.MapFrom(src => src.TaskActions))
                .ForMember(dest => dest.TaskMembers, opt => opt.MapFrom(src => src.TaskMembers))
                .ForMember(dest => dest.TaskObjectives, opt => opt.MapFrom(src => src.TaskObjectives));
        }
    }
}
