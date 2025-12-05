
---------------------------------------------------------------------
-- Bảng Phân quyền
CREATE TABLE Roles (
    RoleID INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(50) NOT NULL 
);
GO

-- Bảng Users: Lưu mật khẩu dạng thường (Không Hash)
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,
    Username VARCHAR(50) NOT NULL UNIQUE,
    Password VARCHAR(50) NOT NULL, -- Cột này lưu pass thường (ví dụ: '123')
    RoleID INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    
    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);
GO

---------------------------------------------------------------------
-- 4. DỮ LIỆU MẪU (SEED DATA)
---------------------------------------------------------------------
INSERT INTO Roles (RoleName) VALUES ('Admin');   -- ID: 1
INSERT INTO Roles (RoleName) VALUES ('Student'); -- ID: 2

-- Admin: pass là "admin123"
INSERT INTO Users (FullName, Username, Password, RoleID) 
VALUES (N'Quản Trị Hệ Thống', 'admin', 'admin123', 1);

-- Sinh viên: pass là "123"
INSERT INTO Users (FullName, Username, Password, RoleID) 
VALUES (N'Nguyễn Văn A', 'SV001', '123', 2);
GO

---------------------------------------------------------------------
-- 5. CÁC THỦ TỤC (STORED PROCEDURES)
---------------------------------------------------------------------

-- 5.1. Procedure Đăng Nhập
CREATE PROCEDURE sp_Login
    @Username VARCHAR(50),
    @Password VARCHAR(50)
AS
BEGIN
    SELECT u.UserID, u.FullName, u.Username, r.RoleName 
    FROM Users u 
    INNER JOIN Roles r ON u.RoleID = r.RoleID 
    WHERE u.Username = @Username AND u.Password = @Password;
END
GO

-- 5.2. Procedure Đăng Ký Admin
CREATE PROCEDURE sp_RegisterAdmin
    @Username VARCHAR(50),
    @Password VARCHAR(50)
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Users WHERE Username = @Username)
    BEGIN
        SELECT 0 AS ResultCode, N'Tên đăng nhập đã tồn tại' AS Message;
        RETURN;
    END

    INSERT INTO Users (FullName, Username, Password, RoleID)
    VALUES (N'Administrator', @Username, @Password, 1);

    SELECT 1 AS ResultCode, N'Đăng ký Admin thành công' AS Message;
END
GO

-- 5.3. Procedure Tạo Sinh Viên
CREATE PROCEDURE sp_CreateStudent
    @FullName NVARCHAR(100),
    @StudentID VARCHAR(50)
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Users WHERE Username = @StudentID)
    BEGIN
        SELECT 0 AS ResultCode, N'Mã sinh viên đã tồn tại' AS Message;
        RETURN;
    END

    -- Mặc định password là "123"
    INSERT INTO Users (FullName, Username, Password, RoleID)
    VALUES (@FullName, @StudentID, '123', 2);

    SELECT 1 AS ResultCode, N'Thêm sinh viên thành công' AS Message;
END
GO

CREATE TABLE Notifications (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(200),
    Content NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Bảng Nhật ký hoạt động
CREATE TABLE ActivityLog (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ActionName NVARCHAR(200), -- Ví dụ: "Đã thêm thông báo..."
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

IF COL_LENGTH('Student', 'trangthai') IS NULL
BEGIN
    ALTER TABLE Student ADD trangthai NVARCHAR(50);
END
GO