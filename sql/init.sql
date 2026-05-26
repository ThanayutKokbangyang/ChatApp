-- =====================================================================
-- SQL Server initialization script
-- รันครั้งเดียวตอน container startup ครั้งแรก
-- =====================================================================

-- ===== AuthDb (สำหรับ Auth Service) =====
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'AuthDb')
BEGIN
    CREATE DATABASE AuthDb;
END
GO

USE AuthDb;
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    CREATE TABLE Users (
        Id           UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Email        NVARCHAR(256) NOT NULL UNIQUE,
        Username     NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(500) NOT NULL,
        Role         NVARCHAR(50)  NOT NULL DEFAULT 'user',
        IsActive     BIT           NOT NULL DEFAULT 1,
        CreatedAt    DATETIME2     NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RefreshTokens' AND xtype='U')
BEGIN
    CREATE TABLE RefreshTokens (
        Id          UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        UserId      UNIQUEIDENTIFIER NOT NULL REFERENCES Users(Id),
        Token       NVARCHAR(500)    NOT NULL UNIQUE,
        ExpiresAt   DATETIME2        NOT NULL,
        IsRevoked   BIT              NOT NULL DEFAULT 0,
        CreatedAt   DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        ReplacedBy  NVARCHAR(500)    NULL
    );

    CREATE INDEX IX_RefreshTokens_Token  ON RefreshTokens(Token);
    CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
END
GO

-- ===== ChatDb (สำหรับ Chat Service) =====
USE master;
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ChatDb')
BEGIN
    CREATE DATABASE ChatDb;
END
GO

USE ChatDb;
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ChatUsers' AND xtype='U')
BEGIN
    CREATE TABLE ChatUsers (
        Id          UNIQUEIDENTIFIER PRIMARY KEY,
        Username    NVARCHAR(100) NOT NULL,
        AvatarUrl   NVARCHAR(500) NULL,
        LastSeenAt  DATETIME2     NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Rooms' AND xtype='U')
BEGIN
    CREATE TABLE Rooms (
        Id              UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Name            NVARCHAR(100)    NOT NULL,
        Description     NVARCHAR(500)    NULL,
        CreatedByUserId UNIQUEIDENTIFIER NOT NULL,
        CreatedAt       DATETIME2        NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Messages' AND xtype='U')
BEGIN
    CREATE TABLE Messages (
        Id        UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        RoomId    UNIQUEIDENTIFIER NOT NULL REFERENCES Rooms(Id),
        SenderId  UNIQUEIDENTIFIER NOT NULL REFERENCES ChatUsers(Id),
        Content   NVARCHAR(MAX)    NOT NULL,
        SentAt    DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        IsDeleted BIT              NOT NULL DEFAULT 0
    );

    -- Index สำคัญสำหรับ query GetByRoomAsync (paged messages by room)
    CREATE INDEX IX_Messages_RoomId_SentAt ON Messages(RoomId, SentAt DESC);
END
GO

-- ===== Sample data: ห้อง default =====
USE ChatDb;
GO

IF NOT EXISTS (SELECT 1 FROM Rooms WHERE Name = 'ห้องทั่วไป')
BEGIN
    INSERT INTO Rooms (Id, Name, Description, CreatedByUserId, CreatedAt)
    VALUES (
        NEWID(),
        N'ห้องทั่วไป',
        N'ห้องแชทสำหรับพูดคุยทั่วไป',
        '00000000-0000-0000-0000-000000000000',
        GETUTCDATE()
    );
END
GO

PRINT 'Initialization complete.';
