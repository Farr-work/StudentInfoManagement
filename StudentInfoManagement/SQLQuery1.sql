
CREATE TABLE Roles (
    RoleID INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(50) NOT NULL -- 1: Admin, 2: Student
);
GO

-- Bảng Users: Lưu thông tin tài khoản
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,       -- Họ tên hiển thị
    Username VARCHAR(50) NOT NULL UNIQUE,  -- Tên đăng nhập / Mã SV
    PasswordHash NVARCHAR(255) NOT NULL,   -- Mật khẩu đã mã hóa SHA256
    RoleID INT NOT NULL,                   -- Khóa ngoại trỏ sang Roles
    CreatedAt DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);
GO

---------------------------------------------------------------------
-- 3. DỮ LIỆU MẪU (SEED DATA)
---------------------------------------------------------------------

-- Tạo 2 quyền cơ bản
INSERT INTO Roles (RoleName) VALUES ('Admin');   -- ID: 1
INSERT INTO Roles (RoleName) VALUES ('Student'); -- ID: 2

-- Tạo tài khoản Admin mẫu
-- User: admin
-- Pass: admin123 (Hash SHA256: 240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9)
INSERT INTO Users (FullName, Username, PasswordHash, RoleID)
VALUES (N'Quản Trị Hệ Thống', 'admin', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 1);

-- Tạo tài khoản Sinh viên mẫu
-- User: SV001
-- Pass: 123 (Hash SHA256: a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3)
INSERT INTO Users (FullName, Username, PasswordHash, RoleID)
VALUES (N'Nguyễn Văn A', 'SV001', 'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', 2);
GO

---------------------------------------------------------------------
-- 4. CÁC THỦ TỤC (STORED PROCEDURES)
---------------------------------------------------------------------

-- 4.1. Thủ tục Đăng nhập
-- Logic: Kiểm tra user/pass, trả về Role để C# điều hướng
CREATE PROCEDURE sp_Login
    @Username VARCHAR(50),
    @Password NVARCHAR(255) -- C# truyền vào chuỗi đã Hash
AS
BEGIN
    SELECT u.UserID, u.FullName, u.Username, r.RoleName
    FROM Users u
    INNER JOIN Roles r ON u.RoleID = r.RoleID
    WHERE u.Username = @Username AND u.PasswordHash = @Password;
END;
GO

-- 4.2. Thủ tục Đăng ký ADMIN (Dùng cho form Đăng ký công khai)
-- Logic: Chỉ tạo tài khoản RoleID = 1. Việc check mã "2005" nằm ở C#.
CREATE PROCEDURE sp_RegisterAdmin
    @Username VARCHAR(50),
    @PasswordHash NVARCHAR(255)
AS
BEGIN
    -- 1. Kiểm tra trùng tên đăng nhập
    IF EXISTS (SELECT 1 FROM Users WHERE Username = @Username)
    BEGIN
        SELECT 0 AS ResultCode, N'Tên đăng nhập đã tồn tại!' AS Message;
        RETURN;
    END

    -- 2. Thêm mới Admin
    -- Form đăng ký của bạn không có ô Họ Tên cho Admin, ta đặt tạm là Administrator
    INSERT INTO Users (FullName, Username, PasswordHash, RoleID)
    VALUES (N'Administrator', @Username, @PasswordHash, 1);

    SELECT 1 AS ResultCode, N'Đăng ký Admin thành công!' AS Message;
END;
GO

-- 4.3. Thủ tục Thêm Sinh Viên (Dùng cho Admin quản lý)
-- Logic: Admin nhập Mã SV + Tên. Mật khẩu mặc định là hash của "123".
CREATE PROCEDURE sp_CreateStudent
    @FullName NVARCHAR(100),
    @StudentID VARCHAR(50),  -- Mã sinh viên làm Username
    @DefaultPasswordHash NVARCHAR(255) -- Hash của "123" truyền từ C# xuống
AS
BEGIN
    -- 1. Kiểm tra trùng mã sinh viên
    IF EXISTS (SELECT 1 FROM Users WHERE Username = @StudentID)
    BEGIN
        SELECT 0 AS ResultCode, N'Mã sinh viên này đã tồn tại trong hệ thống.' AS Message;
        RETURN;
    END

    -- 2. Thêm mới Sinh viên (RoleID = 2)
    INSERT INTO Users (FullName, Username, PasswordHash, RoleID)
    VALUES (@FullName, @StudentID, @DefaultPasswordHash, 2);

    SELECT 1 AS ResultCode, N'Thêm sinh viên thành công!' AS Message;
END;
GO