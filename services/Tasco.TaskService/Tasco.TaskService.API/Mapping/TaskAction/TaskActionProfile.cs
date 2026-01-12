using AutoMapper;
using Tasco.TaskService.API.Protos;
using Tasco.TaskService.Service.BusinessModels;
using Tasco.TaskService.Repository.Entities;

namespace Tasco.TaskService.API.Mapping.TaskAction
{
    public class TaskActionProfile : Profile
    {
        public TaskActionProfile()
        {
            CreateMap<TaskActionBusinessModel, Repository.Entities.TaskAction>()
                .ForMember(dest => dest.WorkTaskId, opt => opt.MapFrom(src => src.WorkTaskId));

            CreateMap<Repository.Entities.TaskAction, Protos.WorkAreaTaskActionResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.WorkTaskId, opt => opt.MapFrom(src => src.WorkTaskId.ToString()))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId.ToString()))
                .ForMember(dest => dest.ActionDate, opt => opt.MapFrom(src => src.ActionDate.ToString("yyyy-MM-ddTHH:mm:ssZ")));
        }
    }
} 