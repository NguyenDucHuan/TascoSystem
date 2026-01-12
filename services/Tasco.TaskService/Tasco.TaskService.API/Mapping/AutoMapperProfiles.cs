using AutoMapper;
using Tasco.TaskService.API.Payload.Request;
using Tasco.TaskService.API.Protos;
using Tasco.TaskService.Repository.Entities;
using Tasco.TaskService.Repository.Paginate;
using Tasco.TaskService.Service.BusinessModels;

namespace Tasco.TaskService.API.Mapping
{
    // This profile is now a placeholder. All entity mappings are in their own profiles.
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // All mappings are now in separate profiles by entity.
        }
    }
}