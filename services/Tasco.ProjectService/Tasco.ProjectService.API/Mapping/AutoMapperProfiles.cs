using AutoMapper;
using Tasco.ProjectService.Repository.Entities;
using Tasco.ProjectService.Service.BussinessModel.ProjectBussinessModel;

namespace Tasco.ProjectService.API.Mapping
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Project, ProjectResponse>()
                .ForMember(dest => dest.Members, opt => opt.MapFrom(src => src.Members));
            CreateMap<ProjectMember, ProjectMemberResponse>();
            CreateMap<ProjectResponse, Project>()
                .ForMember(dest => dest.Members, opt => opt.Ignore());
            CreateMap<ProjectMemberResponse, ProjectMember>();
            CreateMap<Project, ProjectResponse>()
                .ForMember(dest => dest.Members, opt => opt.MapFrom(src => src.Members));
            CreateMap<ProjectMember, ProjectMemberResponse>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.ApprovedStatus, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.ApprovedUpdateDate, opt => opt.MapFrom(src => src.ApprovedUpdateDate))
                .ForMember(dest => dest.RemoveDate, opt => opt.MapFrom(src => src.RemoveDate));

        }
    }
}
