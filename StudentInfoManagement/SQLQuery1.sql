-- =============================================
-- PHẦN 1: XÓA BẢNG CŨ VÀ DỮ LIỆU (RESET DATABASE)
-- =============================================
-- Lưu ý: Thứ tự xóa rất quan trọng. Phải xóa bảng "con" (bảng chứa khóa ngoại) trước, 
-- sau đó mới xóa được bảng "cha" (bảng được tham chiếu).

-- 1. Xóa các bảng không có ràng buộc hoặc bảng con cấp cuối cùng
DROP TABLE IF EXISTS Notifications;
DROP TABLE IF EXISTS ActivityLog;
DROP TABLE IF EXISTS Users;           -- Chứa FK tới Roles
DROP TABLE IF EXISTS REGISTRATIONS;   -- Chứa FK tới Student và SECTIONS

-- 2. Xóa các bảng trung gian (Lớp học phần, Sinh viên...)
DROP TABLE IF EXISTS SECTIONS;        -- Chứa FK tới SUBJECTS và LECTURERS
DROP TABLE IF EXISTS Student;         -- Được REGISTRATIONS tham chiếu (đã xóa REGISTRATIONS ở trên nên xóa được)
DROP TABLE IF EXISTS Roles;           -- Được Users tham chiếu

-- 3. Xóa các bảng danh mục gốc (Môn học, Giảng viên, Khoa...)
DROP TABLE IF EXISTS SUBJECTS;        -- Chứa FK tới DEPARTMENTS
DROP TABLE IF EXISTS LECTURERS;       -- Được SECTIONS tham chiếu
DROP TABLE IF EXISTS DEPARTMENTS;     -- Được SUBJECTS tham chiếu

-- =============================================
-- PHẦN 2: TẠO BẢNG MỚI (CREATE TABLES)
-- =============================================

-- 1. Bảng KHOA (DEPARTMENTS)
-- Danh mục các khoa trong trường
CREATE TABLE DEPARTMENTS (
    DepartmentID VARCHAR(10) PRIMARY KEY,      -- Mã Khoa
    DepartmentName NVARCHAR(100) NOT NULL      -- Tên Khoa
);

-- 2. Bảng GIẢNG VIÊN (LECTURERS)
-- Danh sách giảng viên
CREATE TABLE LECTURERS (
    LecturerID VARCHAR(10) PRIMARY KEY,        -- Mã Giảng viên
    LecturerName NVARCHAR(100) NOT NULL,       -- Tên Giảng viên
    Email VARCHAR(100)                         -- Email
);

-- 3. Bảng MÔN HỌC (SUBJECTS)
-- Lưu trữ thông tin môn học. Đã thêm cột Semester theo yêu cầu của bạn.
CREATE TABLE SUBJECTS (
    SubjectID VARCHAR(20) PRIMARY KEY,         -- Mã môn học (VD: INT1001)
    SubjectName NVARCHAR(200) NOT NULL,        -- Tên môn học
    Credits INT NOT NULL,                      -- Số tín chỉ
    Semester NVARCHAR(50),                     -- Học kỳ (Mặc định/Dự kiến)
    DepartmentID VARCHAR(10),                  -- Thuộc Khoa nào
    
    -- Liên kết khóa ngoại đến bảng DEPARTMENTS
    FOREIGN KEY (DepartmentID) REFERENCES DEPARTMENTS(DepartmentID)
);

-- 4. Bảng LỚP HỌC PHẦN (SECTIONS)
-- Các lớp mở ra để sinh viên đăng ký học (VD: Lớp Toán A1 - HK1 - GV A dạy)
CREATE TABLE SECTIONS (
    SectionID VARCHAR(20) PRIMARY KEY,         -- Mã lớp học phần (VD: INT1001_01)
    SubjectID VARCHAR(20) NOT NULL,            -- Thuộc môn học nào
    Semester NVARCHAR(50) NOT NULL,            -- Học kỳ mở lớp (VD: HK1 2025)
    LecturerID VARCHAR(10),                    -- Giảng viên dạy
    MaxCapacity INT DEFAULT 60,                -- Sĩ số tối đa
    
    -- Liên kết khóa ngoại
    FOREIGN KEY (SubjectID) REFERENCES SUBJECTS(SubjectID),
    FOREIGN KEY (LecturerID) REFERENCES LECTURERS(LecturerID)
);

-- 5. Bảng SINH VIÊN (Student)
-- Giữ nguyên tên thuộc tính như sơ đồ cũ của bạn
CREATE TABLE Student (
    masv VARCHAR(20) PRIMARY KEY,              -- Mã sinh viên (Khóa chính)
    hoten NVARCHAR(100) NOT NULL,              -- Họ tên
    tenlop VARCHAR(20),                        -- Tên lớp (Lớp hành chính)
    gioitinh NVARCHAR(10),                     -- Giới tính
    diachi NVARCHAR(255),                      -- Địa chỉ
    email VARCHAR(100),                        -- Email
    sdt VARCHAR(15),                           -- Số điện thoại
    trangthai NVARCHAR(50) DEFAULT N'Đang học' -- Trạng thái (Đang học, Bảo lưu...)
);

-- 6. Bảng ĐĂNG KÝ (REGISTRATIONS) - **Bảng Mới Quan Trọng**
-- Lưu trữ kết quả đăng ký môn học của sinh viên
CREATE TABLE REGISTRATIONS (
    RegistrationID INT IDENTITY(1,1) PRIMARY KEY,
    masv VARCHAR(20) NOT NULL,                 -- Sinh viên nào đăng ký
    SectionID VARCHAR(20) NOT NULL,            -- Đăng ký lớp nào
    RegistrationDate DATETIME DEFAULT GETDATE(), -- Ngày đăng ký
    Status NVARCHAR(50) DEFAULT N'Đã đăng ký',   -- Trạng thái (Đã đăng ký, Đã hủy)

    -- Liên kết khóa ngoại
    FOREIGN KEY (masv) REFERENCES Student(masv) ON DELETE CASCADE,
    FOREIGN KEY (SectionID) REFERENCES SECTIONS(SectionID)
);

-- 7. Bảng PHÂN QUYỀN (Roles)
CREATE TABLE Roles (
    RoleID INT PRIMARY KEY,                    -- ID quyền (1: Admin, 2: Student...)
    RoleName NVARCHAR(50) NOT NULL             -- Tên quyền
);

-- 8. Bảng NGƯỜI DÙNG HỆ THỐNG (Users)
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100),
    Username VARCHAR(50) NOT NULL UNIQUE,      -- Tên đăng nhập
    Password VARCHAR(255) NOT NULL,            -- Mật khẩu (Nên mã hóa)
    RoleID INT,                                -- Quyền hạn
    CreatedAt DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);

-- 9. Bảng LOG HOẠT ĐỘNG (ActivityLog)
CREATE TABLE ActivityLog (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ActionName NVARCHAR(255),                  -- Tên hành động
    CreatedAt DATETIME DEFAULT GETDATE()       -- Thời gian thực hiện
);

-- 10. Bảng THÔNG BÁO (Notifications)
CREATE TABLE Notifications (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(200),
    Content NVARCHAR(MAX),                     -- Nội dung thông báo
    CreatedAt DATETIME DEFAULT GETDATE()
);