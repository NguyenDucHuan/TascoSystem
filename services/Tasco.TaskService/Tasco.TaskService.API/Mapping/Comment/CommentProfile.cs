using AutoMapper;
using Tasco.TaskService.API.Protos;
using Tasco.TaskService.Service.BusinessModels;
using Tasco.TaskService.Repository.Entities;

namespace Tasco.TaskService.API.Mapping.Comment
{
    public class CommentProfile : Profile
    {
        public CommentProfile()
        {
            CreateMap<Protos.CommentRequest, CommentBusinessModel>()
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => Guid.Parse(src.TaskId)))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => Guid.Parse(src.UserId)))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName));
            
            CreateMap<Protos.UpdateCommentRequest, CommentBusinessModel>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => Guid.Parse(src.UserId)))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName));
            
            CreateMap<CommentBusinessModel, Repository.Entities.Comment>();
            CreateMap<Repository.Entities.Comment, CommentBusinessModel>();
            CreateMap<Repository.Entities.Comment, Protos.CommentResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => src.TaskId.ToString()))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId.ToString()))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")))
                .ForMember(dest => dest.UpdatedAt,
                    opt => opt.MapFrom(src =>
                        src.UpdatedAt.HasValue ? src.UpdatedAt.Value.ToString("yyyy-MM-ddTHH:mm:ssZ") : ""));
        }
    }
} 