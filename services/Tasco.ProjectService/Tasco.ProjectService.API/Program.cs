using Microsoft.EntityFrameworkCore;
using Tasco.ProjectService.API.Mapping;
using Tasco.ProjectService.Repository.Entities;
using Tasco.ProjectService.Repository.Repositories;
using Tasco.ProjectService.Repository.UnitOfWork;
using Tasco.ProjectService.Service.Services.Implemention;
using Tasco.ProjectService.Service.Services.Interface;
using Tasco.ProjectService.API.Middlewares;
using Tasco.ProjectService.Service.Services.GRpcService;
using Tasco.Shared.Notifications.Interfaces;
using Tasco.Shared.Notifications.Publishers;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Tasco.Orchestrator.Api.Midlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

// --- Add gRPC services ---
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<GlobalGrpcExceptionInterceptor>();
});

// --- Db Context Configuration ---
builder.Services.AddDbContext<ProjectManagementDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<GlobalGrpcExceptionInterceptor>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IProjectMemberService, ProjectMemberService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork<ProjectManagementDbContext>>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
// --- Mapper Configuration ---
builder.Services.AddAutoMapper(typeof(AutoMapperProfiles));
// --- Unit of Work Configuration ---

// Add Notification Services
builder.Services.AddScoped<INotificationPublisher, RabbitMQNotificationPublisher>();

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
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Project Service API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProjectManagementDbContext>();
    try
    {
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while applying migrations.");
        throw; // Re-throw to ensure startup fails if migrations fail
    }
}

// Thêm middleware CORS
app.UseCors("AllowAll");

// Thêm middleware đọc claims từ header từ Gateway
app.UseClaimsFromHeaders();

app.UseAuthorization();

app.MapControllers();


// 👉 Map gRPC service
app.MapGrpcService<GrpcProjectService>();
app.MapGrpcService<GrpcProjectMenberService>();

app.Run();