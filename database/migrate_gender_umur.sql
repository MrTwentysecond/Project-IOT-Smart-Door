-- Migrasi: tambah jenis_kelamin ('L'/'P') + umur ke PENGGUNA. Aman diulang.
-- Cara pakai: sqlcmd -S "(localdb)\MSSQLLocalDB" -i database/migrate_gender_umur.sql
USE SmartDoor;
GO
IF COL_LENGTH(N'dbo.PENGGUNA', N'jenis_kelamin') IS NULL
    ALTER TABLE dbo.PENGGUNA ADD jenis_kelamin VARCHAR(16) NULL;
GO
IF COL_LENGTH(N'dbo.PENGGUNA', N'umur') IS NULL
    ALTER TABLE dbo.PENGGUNA ADD umur INT NULL;
GO
IF OBJECT_ID(N'CK_PENGGUNA_gender', N'C') IS NULL
    ALTER TABLE dbo.PENGGUNA ADD CONSTRAINT CK_PENGGUNA_gender CHECK (jenis_kelamin IN ('L', 'P'));
GO
IF OBJECT_ID(N'CK_PENGGUNA_umur', N'C') IS NULL
    ALTER TABLE dbo.PENGGUNA ADD CONSTRAINT CK_PENGGUNA_umur CHECK (umur IS NULL OR umur >= 0);
GO
-- Contoh: Bayu Putra, L, 21 tahun.
UPDATE dbo.PENGGUNA SET jenis_kelamin = 'L', umur = 21
WHERE CompanyCode = 'KEL4' AND id_pengguna = 1 AND jenis_kelamin IS NULL;
GO
