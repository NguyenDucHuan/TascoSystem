using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Tasco.TaskService.API.Controllers
{
    public class BaseController<T> : ControllerBase where T : BaseController<T>
    {
        protected ILogger<T> _logger;
        protected IMapper _mapper;

        public BaseController(ILogger<T> logger, IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
        }
    }
}