using Microsoft.Extensions.Hosting;
using Tasco.NotificationService.Worker;
using Tasco.NotificationService.Worker.Consumers;
using Tasco.NotificationService.Core.Interfaces;
using Tasco.NotificationService.Infrastructure.Messaging;
using Tasco.NotificationService.Infrastructure.Services;
using Tasco.NotificationService.Infrastructure.Channels;
using Tasco.NotificationService.Infrastructure.SMTPs.Repositories;
using DotNetEnv;
using Microsoft.Extensions.Configuration;

// Load .env file
Env.Load();

var builder = Host.CreateApplicationBuilder(args);

// Add environment variables to configuration
builder.Configuration.AddEnvironmentVariables();

// Register dependencies
builder.Services.AddSingleton<IRabbitMQConnection, RabbitMQConnection>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationTemplateService, NotificationTemplateService>();

builder.Services.AddScoped<IEmailRepository, EmailRepository>();

// Register notification channels
builder.Services.AddScoped<INotificationChannel, EmailNotificationChannel>();

// Add background workers
builder.Services.AddHostedService<NotificationMessageConsumer>(); // Consumer lắng nghe queue

var host = builder.Build();
host.Run();
