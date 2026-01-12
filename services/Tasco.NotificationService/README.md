# 🔔 Tasco Notification Service

Dịch vụ thông báo thông minh cho hệ thống Tasco, hỗ trợ nhiều kênh gửi thông báo với template email tiếng Việt thân thiện.

## 🏗️ Architecture

Solution được chia thành **3 projects** theo Clean Architecture:

```
📦 Tasco.NotificationService
├── 🎯 Tasco.NotificationService.Worker      (Application Layer)
├── 📋 Tasco.NotificationService.Core        (Domain Layer)  
└── 🔧 Tasco.NotificationService.Infrastructure (Infrastructure Layer)
```

### 📋 **Core Project** (Domain Layer)
```
Tasco.NotificationService.Core/
├── Models/
│   ├── NotificationMessage.cs       # Domain model cho thông báo
│   └── NotificationResult.cs        # Kết quả xử lý thông báo
├── Interfaces/
│   ├── INotificationService.cs      # Interface cho service chính
│   ├── INotificationTemplateService.cs # Interface cho template service
│   ├── INotificationChannel.cs      # Interface cho delivery channels
│   └── IRabbitMQConnection.cs       # Interface cho RabbitMQ connection
└── Enums/
    └── NotificationStatus.cs        # Enums cho trạng thái
```

### 🔧 **Infrastructure Project** (Infrastructure Layer)
```
Tasco.NotificationService.Infrastructure/
├── Services/
│   ├── NotificationService.cs           # Business logic chính
│   └── NotificationTemplateService.cs   # Xử lý email templates
├── Channels/
│   └── EmailNotificationChannel.cs     # Email delivery channel
├── Messaging/
│   └── RabbitMQConnection.cs           # RabbitMQ connection implementation
└── SMTPs/
    ├── Models/
    │   └── Email.cs                    # Email model
    └── Repositories/
        ├── IEmailRepository.cs         # Interface cho email repo
        └── EmailRepository.cs          # SMTP implementation
```

### 🎯 **Worker Project** (Application Layer)
```
Tasco.NotificationService.Worker/
├── Program.cs                       # Entry point & DI configuration
├── Consumers/
│   └── NotificationMessageConsumer.cs # RabbitMQ message consumer
├── Constants/
│   └── QueueNames.cs               # Queue name constants
├── Assets/
│   └── Logos/
│       └── Tasco.png               # Logo for email templates
├── appsettings.json                # Configuration
└── Dockerfile                      # Container configuration
```

## ✨ Features

- **📧 Email Templates**: Template email tiếng Việt thân thiện với logo Tasco
- **🎨 Modern UI**: Email design hiện đại, responsive
- **🔄 Multi-Channel**: Hỗ trợ nhiều kênh thông báo (Email, SMS, Push, In-App)
- **⚡ Async Processing**: Xử lý bất đồng bộ với RabbitMQ
- **🏷️ Priority System**: Hệ thống ưu tiên thông báo (Low, Normal, High, Critical)
- **📊 Metadata Support**: Hỗ trợ metadata tùy chỉnh cho từng thông báo
- **🔗 Smart URLs**: Tự động tạo action URLs dựa trên loại thông báo

## 🚀 Getting Started

### Prerequisites
- .NET 8.0
- RabbitMQ Server
- SMTP Server (for email notifications)

### Configuration

Cập nhật `appsettings.json`:

```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/"
  },
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "EnableSsl": true,
    "FromEmail": "noreply@tasco.app",
    "FromName": "Tasco System"
  }
}
```

### Build & Run

```bash
# Build solution
dotnet build

# Run worker service
dotnet run --project Tasco.NotificationService.Worker
```

### Docker

```bash
# Build Docker image
docker build -t tasco-notification-service .

# Run container
docker run -d \
  --name tasco-notifications \
  -e RabbitMQ__HostName=rabbitmq-server \
  -e Smtp__Host=smtp.gmail.com \
  tasco-notification-service
```

## 📨 Email Templates

Service hỗ trợ các loại thông báo sau với template tiếng Việt:

- **📋 Task Status Changed**: Cập nhật trạng thái công việc
- **✨ Task Assigned**: Giao việc mới  
- **💬 Task Comment Added**: Bình luận mới
- **🎉 Project Created**: Tạo dự án mới
- **📝 Project Updated**: Cập nhật dự án
- **⏰ Deadline Reminder**: Nhắc nhở deadline
- **👋 Mention in Comment**: Được nhắc đến

## 🔧 Dependencies

### Core
- `RabbitMQ.Client 7.1.2` - RabbitMQ client

### Infrastructure  
- `RabbitMQ.Client 7.1.2` - Message queuing
- `Microsoft.Extensions.Logging.Abstractions 9.0.5` - Logging
- `Microsoft.Extensions.Configuration 8.0.0` - Configuration

### Worker
- `Microsoft.Extensions.Hosting 8.0.1` - Background service hosting
- `DotNetEnv 3.1.1` - Environment variables
- `Newtonsoft.Json 13.0.3` - JSON serialization

## 📄 License

This project is licensed under the MIT License. 