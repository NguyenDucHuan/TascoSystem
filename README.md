# TascoSystem

Hệ thống quản lý dự án và công việc theo kiến trúc Microservices.

## 📋 Giới thiệu

TascoSystem là hệ thống backend quản lý dự án và nhiệm vụ (Task Management System) được thiết kế theo kiến trúc microservices. Dự án tập trung phát triển các API services và infrastructure, được xây dựng bởi đội ngũ **3 Backend Engineers**. 

> ⚠️ **Lưu ý**: Repository này chỉ bao gồm phần **Backend**. Frontend sẽ được phát triển riêng biệt. 

## 🏗️ Kiến trúc Backend

### Microservices
- **API Gateway** (Port 5000) - Cổng API trung tâm, xử lý routing và JWT authentication
- **User Auth Service** (Port 5001) - Quản lý xác thực và phân quyền
- **Project Service** (Port 5002) - Quản lý dự án
- **Task Service** (Port 5003) - Quản lý công việc/nhiệm vụ
- **Orchestrator** (Port 5004) - Điều phối workflow giữa các services
- **Notification Service** - Xử lý gửi thông báo email

### Infrastructure
- **SQL Server** (Ports 1434, 1435) - Database cho Auth & Task Service
- **PostgreSQL** (Port 5432) - Database cho Project Service
- **RabbitMQ** (Ports 5672, 15672) - Message broker

## 🚀 Công nghệ

- . NET Core / ASP.NET Core
- Microsoft SQL Server 2019
- PostgreSQL 15
- RabbitMQ
- Docker & Docker Compose
- JWT Authentication

## 🛠️ Cài đặt và chạy

### Yêu cầu
- Docker Desktop
- Docker Compose

### Khởi động hệ thống

```bash
# Clone repository
git clone https://github.com/NguyenDucHuan/TascoSystem.git
cd TascoSystem

# Chạy tất cả services
docker-compose up -d

# Xem logs
docker-compose logs -f

# Dừng services
docker-compose down
```

## 🔌 Endpoints

| Service | Port | URL |
|---------|------|-----|
| API Gateway | 5000 | http://localhost:5000 |
| User Auth Service | 5001 | http://localhost:5001 |
| Project Service | 5002 | http://localhost:5002 |
| Task Service | 5003 | http://localhost:5003 |
| Orchestrator | 5004 | http://localhost:5004 |
| RabbitMQ Management | 15672 | http://localhost:15672 |

## 📊 Database

### SQL Server - Auth Service
```
Host: localhost: 1434
Database: TascoAuthDb
User: sa
Password: Password123@
```

### SQL Server - Task Service
```
Host: localhost:1435
Database: TascoTaskDb
User: sa
Password: Password123@
```

### PostgreSQL - Project Service
```
Host: localhost:5432
Database: ProjectManagementDB
User: postgres
Password: 12345
```

### RabbitMQ Console
```
URL: http://localhost:15672
Username: admin
Password: admin123
```

## 🔐 Bảo mật

**⚠️ QUAN TRỌNG**: Các credentials trong `docker-compose.yml` chỉ dùng cho môi trường **Development**. 
Không sử dụng cho Production! 

## 📁 Cấu trúc

```
TascoSystem/
├── services/
│   ├── Tasco.Gateway/
│   ├── Tasco.UserAuthService/
│   ├── Tasco. ProjectService/
│   ├── Tasco.TaskService/
│   ├── Tasco.Orchestrator/
│   └── Tasco.NotificationService/
└── docker-compose.yml
```

## 📝 License

Chưa có license. 

---

**📌 README này được gợi ý bởi GitHub Copilot**

*Developed by TascoSystem Backend Team*
