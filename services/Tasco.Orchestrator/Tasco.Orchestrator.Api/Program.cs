
using Tasco.Orchestrator.Api.Controllers;
using Tasco.Orchestrator.Api.Midlewares;
using Tasco.Orchestrator.Infrastructure.GrpcClients;
using Tasco.ProjectService.Service.Protos;
using Tasco.ProjectService.Service.Services.GRpcService;

namespace Tasco.Orchestrator.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --add project grpc client service
            builder.Services.AddScoped<ProjectGrpcClientService>();
            builder.Services.AddScoped<ProjectMenberGrpcClientService>();
            builder.Services.AddScoped<WorkAreaGrpcClientService>();
            builder.Services.AddScoped<WorkTaskGrpcClientService>();
            builder.Services.AddScoped<SubTaskGrpcClientService>();
            builder.Services.AddScoped<TaskObjectiveGrpcClientService>();
            builder.Services.AddScoped<TaskMemberClientService>();
            builder.Services.AddScoped<CommentGrpcClientService>();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            // --- CORS Configuration ---
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // --- Swagger Configuration ---
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Orchestrator Service API", Version = "v1" });
            });
            builder.Services.AddGrpcClient<Project.ProjectClient>(o =>
            {
                //o.Address = new Uri("http://localhost:5002");
                o.Address = new Uri("http://projectservice:8080");
            });
            builder.Services.AddGrpcClient<ProjectMemberService.ProjectMemberServiceClient>(o =>
            {
                //o.Address = new Uri("http://localhost:5002");
                o.Address = new Uri("http://projectservice:8080");
            });
            builder.Services.AddGrpcClient<WorkAreaService.WorkAreaServiceClient>(o =>
            {
                o.Address = new Uri("http://taskservice:8080");
            });
            builder.Services.AddGrpcClient<WorkTaskService.WorkTaskServiceClient>(o =>
            {
                o.Address = new Uri("http://taskservice:8080");
            });
            builder.Services.AddGrpcClient<SubTaskService.SubTaskServiceClient>(o =>
            {
                o.Address = new Uri("http://taskservice:8080");
            });
            builder.Services.AddGrpcClient<TaskObjectiveService.TaskObjectiveServiceClient>(o =>
            {
                o.Address = new Uri("http://taskservice:8080");
            });
            builder.Services.AddGrpcClient<TaskMemberService.TaskMemberServiceClient>(o =>
            {
                o.Address = new Uri("http://taskservice:8080");
            });
            builder.Services.AddGrpcClient<CommentService.CommentServiceClient>(o =>
            {
                o.Address = new Uri("http://taskservice:8080");
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }


            app.UseCors("AllowAll");

            app.UseClaimsFromHeaders();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
