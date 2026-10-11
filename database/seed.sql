-- Data contoh Smart Door (CompanyCode = 'KEL4').
-- Password admin = hash SHA-256 dari 'admin123' (ganti + tambah salt di aplikasi).
-- Cara pakai: sqlcmd -S localhost\SQLEXPRESS -E -i database/seed.sql
USE SmartDoor;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.ADMIN WHERE CompanyCode = 'KEL4' AND id_admin = 1)
INSERT INTO dbo.ADMIN (CompanyCode, id_admin, username, password_hash, CreatedBy)
VALUES ('KEL4', 1, 'admin',
    '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9',
    'admin');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.PENGGUNA WHERE CompanyCode = 'KEL4' AND id_pengguna = 1)
INSERT INTO dbo.PENGGUNA (CompanyCode, id_pengguna, id_admin, nama, nim_nip, jenis_kelamin, umur, CreatedBy)
VALUES ('KEL4', 1, 1, 'Bayu Putra', '12345678', 'L', 21, 'admin');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.PINTU WHERE CompanyCode = 'KEL4' AND id_pintu = 1)
INSERT INTO dbo.PINTU (CompanyCode, id_pintu, nama_pintu, durasi_buka, CreatedBy)
VALUES ('KEL4', 1, 'Pintu Lab', 5, 'admin');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.KARTU_RFID WHERE CompanyCode = 'KEL4' AND id_kartu = 1)
INSERT INTO dbo.KARTU_RFID (CompanyCode, id_kartu, id_pengguna, uid_kartu, CreatedBy)
VALUES ('KEL4', 1, 1, 'A1B2C3D4', 'admin');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.HAK_AKSES WHERE CompanyCode = 'KEL4' AND id_akses = 1)
INSERT INTO dbo.HAK_AKSES (CompanyCode, id_akses, id_kartu, id_pintu, CreatedBy)
VALUES ('KEL4', 1, 1, 1, 'admin');
GO
