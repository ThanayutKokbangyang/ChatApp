# ChatApp - Real-time Chat Website

โปรเจค chat website แบบ real-time ใช้สถาปัตยกรรม **Clean Architecture** และ **Microservices**

## Tech Stack

- **.NET 9** — Auth Service + Chat Service + Blazor WebAssembly
- **SignalR** — Real-time WebSocket messaging
- **Dapper** — Micro-ORM
- **SQL Server 2022** — Database (แยก AuthDb / ChatDb)
- **JWT Authentication** — Stateless auth, short-lived access token (15 min) + refresh token rotation
- **Docker + Nginx** — Reverse proxy + HTTPS + WebSocket upgrade
- **Blazor WebAssembly** — Frontend SPA

## Architecture

```
            [ Browser ]
                 │
                 ▼ HTTPS / WSS
        ┌─────────────────┐
        │  Nginx Gateway  │  (port 8080 → 8443 redirect, TLS termination)
        └─────────────────┘
           │      │      │
       /   │  /auth/  /chat/ , /hubs/
           ▼      ▼      ▼
    [Blazor]  [Auth]  [Chat]
                │      │
                ▼      ▼
       AuthDb         ChatDb
                          │
                          ▼
                  uploads/avatars (volume)

## Design Patterns ที่ใช้ในโปรเจคนี้

| Pattern | จุดที่ใช้ |
|---------|----------|
| **Clean Architecture** | แยก Domain / Application / Infrastructure / API ทุก service |
| **Repository Pattern** | `IUserRepository`, `IMessageRepository`, `IRoomRepository`, `IChatUserRepository`, `IRefreshTokenRepository` |
| **Factory Pattern** | `IDbConnectionFactory` → `SqlConnectionFactory` |
| **Factory Method** | `User.Create()`, `Message.Create()`, `Room.Create()`, `RefreshToken.Create()` |
| **Strategy Pattern** | `ITokenService`, `IPasswordHasher`, `ICacheService`, `IFileStorageService` |
| **Service Layer / Facade** | `IAuthService` → `AuthService`, `IChatService` → `ChatAppService` |
| **CQRS** | Commands (`SendMessageCommand`, `LoginCommand`) vs Queries (`GetRoomHistoryQuery`) |
| **Command Pattern** | `record` types เป็น immutable command objects |
| **Builder Pattern** | `AddAuthInfrastructure()`, `AddChatInfrastructure()` extension methods |
| **Chain of Responsibility** | ASP.NET Middleware Pipeline + `AuthorizationMessageHandler` (HTTP DelegatingHandler) |
| **Observer Pattern** | `AuthenticationStateProvider`, SignalR events (`MessageReceived`, `UserJoined`) |
| **API Gateway Pattern** | Nginx routing → multiple services |
| **Cache-Aside Pattern** | `ChatAppService.GetCachedUserAsync()` |
| **Hub / Mediator Pattern** | `ChatHub` — clients communicate via hub, ไม่ต้องรู้กันเอง |
| **DTO Pattern** | `MessageDto`, `TokenResponseDto` แยก API contract จาก Domain |

## Project Structure

```
ChatApp (Solution)
├── src/
│   ├── ChatApp.AuthService/
│   │   ├── ChatApp.AuthService.API              (Minimal API endpoints)
│   │   ├── ChatApp.AuthService.Application       (Service Layer, Commands, DTOs)
│   │   ├── ChatApp.AuthService.Domain            (Entities, Interfaces)
│   │   └── ChatApp.AuthService.Infrastructure    (Repositories, JWT, BCrypt)
│   └── ChatApp.ChatService/
│       ├── ChatApp.ChatService.API              (REST + SignalR Hub)
│       ├── ChatApp.ChatService.Application
│       ├── ChatApp.ChatService.Domain
│       └── ChatApp.ChatService.Infrastructure
├── Web/
│   └── ChatApp.Web                              (Blazor WebAssembly)
├── nginx/
│   ├── nginx.conf                               (reverse proxy + HTTPS)
│   └── blazor.conf                              (static hosting)
└── sql/
    ├── init.sql                                 (database schema)
    └── entrypoint.sh                            (SQL Server bootstrap)
```

## วิธีรันโปรเจค

### 1. สร้าง self-signed certificate สำหรับ HTTPS (dev only)

ใช้ Docker สร้าง cert (ไม่ต้องติดตั้ง openssl บนเครื่อง) — รันที่ root ของโปรเจค:

```powershell

### 2. รัน Docker Compose

```bashง

### 3. เปิดเว็บ https://localhost:8443
> Browser จะเตือนเรื่อง certificate ไม่ valid (เพราะ self-signed) — กด "Advanced" → "Proceed to localhost"
> (ถ้าหาปุ่มไม่เจอ คลิกบนหน้าจอเตือนแล้วพิมพ์ `thisisunsafe`)

### 4. ทดสอบ

1. คลิก **สมัครสมาชิก** สร้าง account ใหม่
2. **เข้าสู่ระบบ** ด้วย account ที่สมัคร
3. **สร้างห้องแชท** ใหม่
4. เปิดอีก browser/tab ด้วย account อื่น → เข้าห้องเดียวกัน
5. ส่งข้อความ — เห็นทันทีในทั้ง 2 tab (real-time WebSocket!)
6. ไปที่หน้า **โปรไฟล์** → อัพโหลดรูป avatar

## Endpoints

### Auth Service (proxied ที่ `/auth/`)

| Method | Path | Description |
|--------|------|-------------|
| POST | `/auth/api/auth/register` | สมัครสมาชิก |
| POST | `/auth/api/auth/login` | login → access + refresh token |
| POST | `/auth/api/auth/refresh` | refresh access token (token rotation) |
| POST | `/auth/api/auth/revoke` | revoke refresh token |
| GET | `/auth/api/auth/me` | ดูข้อมูล user ปัจจุบัน |

### Chat Service (proxied ที่ `/chat/` และ `/hubs/`)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/chat/api/rooms` | list ห้องทั้งหมด |
| POST | `/chat/api/rooms` | สร้างห้องใหม่ |
| GET | `/chat/api/rooms/{id}/messages` | history pagination |
| POST | `/chat/api/profile/avatar` | upload รูป avatar |
| GET | `/chat/api/profile/me` | ดูโปรไฟล์ |
| WS | `/hubs/chat` | SignalR Hub (real-time) |

## JWT Authentication Flow

1. Client ส่ง email + password ไปที่ `/login`
2. Server ตรวจ password (BCrypt) → ออก `access_token` (JWT) + `refresh_token`
3. Client เก็บ `access_token` (15 นาที) + `refresh_token` (7 วัน) ใน LocalStorage
4. ใส่ `Authorization: Bearer {access_token}` ทุก API call
5. WebSocket ส่ง token ผ่าน `?access_token=xxx` (เพราะ browser ไม่อนุญาตให้ตั้ง header ใน WebSocket)
6. Access token หมดอายุ → ใช้ refresh token ขอ token ใหม่ (token rotation: ตัวเก่า revoke, ตัวใหม่ออก)

## Security Notes

- ✅ JWT signing key + DB password อยู่ใน environment variables
- ✅ HTTPS บังคับ (HSTS + 301 redirect)
- ✅ BCrypt password hashing (work factor 12)
- ✅ Refresh token rotation (ป้องกัน token reuse attack)
- ✅ Short-lived access token (15 นาที) ลด window ที่ token ถูกขโมยไปใช้ได้
- ✅ SQL injection ป้องกันด้วย parameterized queries (Dapper)
- ✅ File upload validation (size + extension)
