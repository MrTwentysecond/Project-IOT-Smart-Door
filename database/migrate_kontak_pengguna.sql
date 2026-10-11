-- Kontak pengguna untuk form pendaftaran (NULL = belum diisi).
-- Rerunnable: dijaga COL_LENGTH.
USE SmartDoor;
GO

IF COL_LENGTH(N'dbo.PENGGUNA', N'kontak') IS NULL
    ALTER TABLE dbo.PENGGUNA ADD kontak VARCHAR(64) NULL;
GO
