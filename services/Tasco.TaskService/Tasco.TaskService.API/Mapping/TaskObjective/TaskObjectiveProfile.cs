using AutoMapper;
using Tasco.TaskService.API.Protos;
using Tasco.TaskService.Service.BusinessModels;
using Tasco.TaskService.Repository.Entities;

namespace Tasco.TaskService.API.Mapping.TaskObjective
{
    public class TaskObjectiveProfile : Profile
    {
        public TaskObjectiveProfile()
        {
            CreateMap<Protos.UpdateTaskObjectiveRequest, TaskObjectiveBusinessModel>()
                .ForMember(dest => dest.WorkTaskId, opt => opt.ConvertUsing(new StringToGuidConverter(), src => src.WorkTaskId))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => src.IsCompleted))
                .ForMember(dest => dest.DisplayOrder, opt => opt.MapFrom(src => src.DisplayOrder))
                .ForMember(dest => dest.CompletedByUserId, opt => opt.ConvertUsing(new NullableStringToGuidConverter(), src => src.CompletedByUserId));

            CreateMap<TaskObjectiveBusinessModel, Repository.Entities.TaskObjective>();
            CreateMap<Repository.Entities.TaskObjective, TaskObjectiveBusinessModel>();
            CreateMap<Repository.Entities.TaskObjective, Protos.TaskObjectiveResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.WorkTaskId, opt => opt.MapFrom(src => src.WorkTaskId.ToString()))
                .ForMember(dest => dest.CreatedByUserId, opt => opt.MapFrom(src => src.CreatedByUserId.ToString()))
                .ForMember(dest => dest.CompletedByUserId,
                    opt => opt.MapFrom(src =>
                        src.CompletedByUserId.HasValue ? src.CompletedByUserId.Value.ToString() : ""))
                .ForMember(dest => dest.CreatedDate,
                    opt => opt.MapFrom(src => src.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ssZ")))
                .ForMember(dest => dest.CompletedDate,
                    opt => opt.MapFrom(src =>
                        src.CompletedDate.HasValue ? src.CompletedDate.Value.ToString("yyyy-MM-ddTHH:mm:ssZ") : ""));

            CreateMap<Repository.Entities.TaskObjective, Protos.WorkAreaTaskObjectiveResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.WorkTaskId, opt => opt.MapFrom(src => src.WorkTaskId.ToString()))
                .ForMember(dest => dest.CreatedByUserId, opt => opt.MapFrom(src => src.CreatedByUserId.ToString()))
                .ForMember(dest => dest.CompletedByUserId,
                    opt => opt.MapFrom(src =>
                        src.CompletedByUserId.HasValue ? src.CompletedByUserId.Value.ToString() : ""))
                .ForMember(dest => dest.CreatedDate,
                    opt => opt.MapFrom(src => src.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ssZ")))
                .ForMember(dest => dest.CompletedDate,
                    opt => opt.MapFrom(src =>
                        src.CompletedDate.HasValue ? src.CompletedDate.Value.ToString("yyyy-MM-ddTHH:mm:ssZ") : ""));
        }
    }

    public class StringToGuidConverter : IValueConverter<string, Guid>
    {
        public Guid Convert(string sourceMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(sourceMember))
            {
                return Guid.Empty;
            }
            
            if (!Guid.TryParse(sourceMember, out var guid))
            {
                return Guid.Empty;
            }
            
            return guid;
        }
    }

    public class NullableStringToGuidConverter : IValueConverter<string, Guid?>
    {
        public Guid? Convert(string sourceMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(sourceMember))
            {
                return null;
            }
            
            if (!Guid.TryParse(sourceMember, out var guid))
            {
                return null;
            }
            
            return guid;
        }
    }
} 