IF DB_ID(N'LibraryDB') IS NULL
    CREATE DATABASE LibraryDB;
GO
USE LibraryDB;
GO

IF OBJECT_ID(N'dbo.ChiTietMuon', N'U') IS NOT NULL DROP TABLE dbo.ChiTietMuon;
IF OBJECT_ID(N'dbo.PhieuMuon', N'U') IS NOT NULL DROP TABLE dbo.PhieuMuon;
IF OBJECT_ID(N'dbo.DauSach', N'U') IS NOT NULL DROP TABLE dbo.DauSach;
IF OBJECT_ID(N'dbo.DocGia', N'U') IS NOT NULL DROP TABLE dbo.DocGia;
GO

CREATE TABLE dbo.DocGia (
    MaDocGia INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    Email VARCHAR(100) NULL UNIQUE,
    NgayHetHanThe DATETIME NULL
);
CREATE TABLE dbo.DauSach (
    MaSach INT IDENTITY(1,1) PRIMARY KEY,
    TenSach NVARCHAR(200) NOT NULL,
    TheLoai NVARCHAR(50) NULL,
    SoLuongCon INT NOT NULL CONSTRAINT CK_DauSach_SoLuongCon CHECK (SoLuongCon >= 0)
);
CREATE TABLE dbo.PhieuMuon (
    MaPhieu INT IDENTITY(1,1) PRIMARY KEY,
    MaDocGia INT NOT NULL,
    NgayMuon DATETIME NOT NULL CONSTRAINT DF_PhieuMuon_NgayMuon DEFAULT GETDATE(),
    HanTra DATETIME NOT NULL,
    TrangThai NVARCHAR(50) NOT NULL CONSTRAINT DF_PhieuMuon_TrangThai DEFAULT N'Đang mượn',
    CONSTRAINT FK_PhieuMuon_DocGia FOREIGN KEY (MaDocGia) REFERENCES dbo.DocGia(MaDocGia),
    CONSTRAINT CK_PhieuMuon_TrangThai CHECK (TrangThai IN (N'Đang mượn', N'Đã trả', N'Quá hạn'))
);
CREATE TABLE dbo.ChiTietMuon (
    MaPhieu INT NOT NULL,
    MaSach INT NOT NULL,
    SoLuong INT NOT NULL CONSTRAINT CK_ChiTietMuon_SoLuong CHECK (SoLuong > 0),
    CONSTRAINT PK_ChiTietMuon PRIMARY KEY (MaPhieu, MaSach),
    CONSTRAINT FK_ChiTietMuon_PhieuMuon FOREIGN KEY (MaPhieu) REFERENCES dbo.PhieuMuon(MaPhieu),
    CONSTRAINT FK_ChiTietMuon_DauSach FOREIGN KEY (MaSach) REFERENCES dbo.DauSach(MaSach)
);
GO

INSERT dbo.DocGia(HoTen, Email, NgayHetHanThe) VALUES
(N'Nguyễn Minh Anh', 'minhanh@example.com', DATEADD(MONTH, 6, GETDATE())),
(N'Trần Hoàng Nam', 'hoangnam@example.com', DATEADD(MONTH, 3, GETDATE())),
(N'Lê Thu Hà', 'thuha@example.com', DATEADD(DAY, -10, GETDATE()));
INSERT dbo.DauSach(TenSach, TheLoai, SoLuongCon) VALUES
(N'Lập trình C# căn bản', N'Công nghệ thông tin', 8),
(N'Cơ sở dữ liệu SQL Server', N'Công nghệ thông tin', 5),
(N'Kinh tế học vi mô', N'Kinh tế', 6),
(N'Tắt đèn', N'Văn học', 10);
GO

CREATE INDEX IX_PhieuMuon_MaDocGia_TrangThai ON dbo.PhieuMuon(MaDocGia, TrangThai);
CREATE INDEX IX_PhieuMuon_HanTra ON dbo.PhieuMuon(HanTra);
GO
