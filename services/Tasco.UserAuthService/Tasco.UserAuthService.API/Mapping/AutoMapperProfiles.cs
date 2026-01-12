using AutoMapper;
using Tasco.UserAuthService.API.Models.RequestModels;
using Tasco.UserAuthService.API.Models.ResponseModels;
using Tasco.UserAuthService.Service.BusinessModels;

namespace Tasco.UserAuthService.API.Mapping
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // Request to Business Model mappings
            CreateMap<LoginRequest, LoginAccountBusiness>();
            CreateMap<RegisterRequest, RegisterAccountBusiness>();

            // Business Model to Response mappings
            CreateMap<AccountBusiness, LoginResponse>();
            CreateMap<AccountBusiness, AccountResponse>();
            CreateMap<TokenBusiness, TokenResponse>();
        }
    }
}
