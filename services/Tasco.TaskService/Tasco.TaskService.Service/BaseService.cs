using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using Tasco.TaskService.Repository.Entities;
using Tasco.TaskService.Repository.UnitOfWork;

namespace Tasco.TaskService.Service
{
	public abstract class BaseService<T> where T : class
	{
		protected readonly IUnitOfWork<TaskManagementDbContext> _unitOfWork;
		protected readonly ILogger<T> _logger;
		protected readonly IMapper _mapper;
		protected readonly IHttpContextAccessor _httpContextAccessor;

		protected BaseService(
			IUnitOfWork<TaskManagementDbContext> unitOfWork,
			ILogger<T> logger,
			IMapper mapper,
			IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_logger = logger;
			_mapper = mapper;
			_httpContextAccessor = httpContextAccessor;
		}

		protected string GetUserIdFromJwt()
		{
			// Authentication is disabled, return null
			_logger.LogInformation("Authentication is disabled, returning null for user ID");
			return null;
		}

		protected string GetUserEmailFromJwt()
		{
			// Authentication is disabled, return null
			_logger.LogInformation("Authentication is disabled, returning null for user email");
			return null;
		}

		protected string GetRoleFromJwt()
		{
			// Authentication is disabled, return null
			_logger.LogInformation("Authentication is disabled, returning null for user role");
			return null;
		}
	}
}
