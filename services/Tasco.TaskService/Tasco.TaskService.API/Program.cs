using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using Tasco.TaskService.API.GrpcServices;
using Tasco.TaskService.API.Mapping;
using Tasco.TaskService.Repository.Entities;
using Tasco.TaskService.Repository.UnitOfWork;
using Tasco.TaskService.Service.Implementations;
using Tasco.TaskService.Service.Interfaces;
using DotNetEnv;
using Tasco.TaskService.API.Middlewares;
using Tasco.Shared.Notifications.Interfaces;
using Tasco.Shared.Notifications.Publishers;
using Microsoft.AspNetCore.Server.Kestrel.Core;
namespace Tasco.TaskService.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Load .env file
            Env.Load();

            // Add configuration from .env
            builder.Configuration.AddEnvironmentVariables();

            // Add services to the container.
            builder.Services.AddControllers();
            //Add Swagger for API documentation
            var enableSwagger = builder.Configuration.GetValue<bool>("EnableSwagger", false);
            if (enableSwagger)
            {
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo() { Title = "Tasco Task Service API", Version = "v1" });
                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                    {
                        In = ParameterLocation.Header,
                        Description = "Please enter JWT with Bearer into field. Example: 'Bearer {token}'",
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "Bearer",
                        BearerFormat = "JWT"
                    });
                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] { }
                        }
                    });
                    options.MapType<TimeOnly>(() => new OpenApiSchema
                    {
                        Type = "string",
                        Format = "time",
                        Example = OpenApiAnyFactory.CreateFromJson("\"13:45:42.0000000\"")
                    });
                });
            }
            // Add gRPC services
            builder.Services.AddGrpc();
            builder.WebHost.ConfigureKestrel(options =>
            {
                // gRPC (HTTP/2)
                options.ListenAnyIP(8080, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http2;
                });
                options.ListenAnyIP(8081, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http1;
                });
            });
            // Add DbContext
            builder.Services.AddDbContext<TaskManagementDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add UnitOfWork
            builder.Services.AddScoped<IUnitOfWork<TaskManagementDbContext>, UnitOfWork<TaskManagementDbContext>>();

            // Add AutoMapper
            builder.Services.AddAutoMapper(typeof(Program).Assembly);

            // Add Services
            builder.Services.AddScoped<IWorkTaskService, WorkTaskService>();
            builder.Services.AddScoped<ITaskActionService, TaskActionService>();
            builder.Services.AddScoped<ITaskMemberService, TaskMemberService>();
            builder.Services.AddScoped<IWorkAreaService, WorkAreaService>();
            builder.Services.AddScoped<ICommentService, CommentService>();
            builder.Services.AddScoped<ITaskObjectiveService, TaskObjectiveService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();

            // Add Notification Services
            builder.Services.AddScoped<INotificationPublisher, RabbitMQNotificationPublisher>();
            // Add HttpContextAccessor
            builder.Services.AddHttpContextAccessor();

            // Add JWT Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                var jwtKey = builder.Configuration["Jwt:Key"];
                if (string.IsNullOrEmpty(jwtKey))
                {
                    throw new InvalidOperationException("JWT Key is not configured. Please set Jwt:Key in configuration.");
                }

                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey))
                };

                options.TokenValidationParameters.NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
                options.TokenValidationParameters.RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment() && enableSwagger)
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TaskManagementDbContext>();
                try
                {
                    dbContext.Database.Migrate(); // Apply pending migrations
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while applying migrations.");
                    throw; // Re-throw to ensure startup fails if migrations fail
                }
            }

            app.UseHttpsRedirection();
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Map gRPC services
            app.MapGrpcService<WorkTaskGrpcService>();
            app.MapGrpcService<WorkAreaGrpcService>();
            app.MapGrpcService<TaskMemberGrpcService>();
            app.MapGrpcService<CommentGrpcService>();
            app.MapGrpcService<TaskObjectiveGrpcService>();
            app.Run();
        }
    }
}