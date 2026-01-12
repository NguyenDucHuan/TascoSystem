using AutoMapper;
using Tasco.TaskService.API.Protos;
using Tasco.TaskService.Service.BusinessModels;
using Tasco.TaskService.Repository.Entities;
using Tasco.TaskService.Repository.Paginate;

namespace Tasco.TaskService.API.Mapping.WorkArea
{
    public class WorkAreaProfile : Profile
    {
        public WorkAreaProfile()
        {
            CreateMap<WorkAreaRequest, WorkAreaBusinessModel>()
                .ForMember(dest => dest.ProjectId, opt => opt.ConvertUsing(new StringToGuidConverter(), src => src.ProjectId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? string.Empty))
                .ForMember(dest => dest.DisplayOrder, opt => opt.ConvertUsing(new StringToIntConverter(), src => src.DisplayOrder))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.CreatedByUserId, opt => opt.ConvertUsing(new StringToGuidConverter(), src => src.CreateByUserId))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));
            CreateMap<Repository.Entities.TaskMember, WorkAreaTaskMemberResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId.ToString()))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName ?? "")) // tránh null
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.UserEmail ?? "")) // tránh null
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role ?? ""));
            CreateMap<UpdateWorkAreaRequest, WorkAreaBusinessModel>()
                .ForMember(dest => dest.ProjectId, opt => opt.ConvertUsing(new StringToGuidConverter(), src => src.Area.ProjectId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Area.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Area.Description ?? string.Empty))
                .ForMember(dest => dest.DisplayOrder, opt => opt.ConvertUsing(new StringToIntConverter(), src => src.Area.DisplayOrder))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.CreatedByUserId, opt => opt.ConvertUsing(new StringToGuidConverter(), src => src.Area.CreateByUserId))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            CreateMap<WorkAreaBusinessModel, Repository.Entities.WorkArea>()
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? string.Empty))
                .ForMember(dest => dest.DisplayOrder, opt => opt.MapFrom(src => src.DisplayOrder))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            CreateMap<Repository.Entities.WorkArea, WorkAreaBusinessModel>();

            CreateMap<Repository.Entities.WorkArea, WorkAreaResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.CreatedByUserId, opt => opt.MapFrom(src => src.CreatedByUserId.ToString()))
                .ForMember(dest => dest.CreatedDate,
                    opt => opt.MapFrom(src => src.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ssZ")))
                .ForMember(dest => dest.WorkTasks, opt => opt.MapFrom(src => src.WorkTasks));

            CreateMap<Paginate<Repository.Entities.WorkArea>, WorkAreaListResponse>()
                .ForMember(dest => dest.WorkAreas, opt => opt.MapFrom(src => src.Items))
                .ForMember(dest => dest.TotalCount, opt => opt.MapFrom(src => src.Total))
                .ForMember(dest => dest.PageCount, opt => opt.MapFrom(src => src.TotalPages))
                .ForMember(dest => dest.CurrentPage, opt => opt.MapFrom(src => src.Page));
            CreateMap<Paginate<WorkAreaBusinessModel>, WorkAreaListResponse>()
                .ForMember(dest => dest.WorkAreas, opt => opt.MapFrom(src => src.Items))
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
                // Log warning for empty GUID
                System.Diagnostics.Debug.WriteLine($"Warning: Empty GUID value encountered");
                return Guid.Empty;
            }

            if (!Guid.TryParse(sourceMember, out var guid))
            {
                // Log warning for invalid GUID format
                System.Diagnostics.Debug.WriteLine($"Warning: Invalid GUID format: {sourceMember}");
                return Guid.Empty;
            }

            return guid;
        }
    }

    public class StringToIntConverter : IValueConverter<string, int>
    {
        public int Convert(string sourceMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(sourceMember))
                return 0;
            return int.TryParse(sourceMember, out var result) ? result : 0;
        }
    }
}