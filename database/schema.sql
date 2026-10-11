-- Smart Door Access — skema SQL Server (migrasi dari SQLite/DB Browser).
-- 6 tabel, tiap tabel memakai 7 default field (CompanyCode, Status, IsDeleted,
-- CreatedBy, CreatedDate, LastUpdatedBy, LastUpdatedDate) sesuai ketentuan dosen.
-- CompanyCode = bagian primary key gabungan di tiap tabel (contoh: 'KEL4').
-- Cara pakai (setelah SQL Server terinstal):
--   sqlcmd -S localhost\SQLEXPRESS -E -i database/schema.sql
--   sqlcmd -S localhost\SQLEXPRESS -E -i database/seed.sql

IF DB_ID(N'SmartDoor') IS NULL
    CREATE DATABASE SmartDoor;
GO
USE SmartDoor;
GO

-- ================= ADMIN =================
IF OBJECT_ID(N'dbo.ADMIN', N'U') IS NULL
CREATE TABLE dbo.ADMIN (
    CompanyCode     VARCHAR(32)  NOT NULL,
    id_admin        INT          NOT NULL,
    username        VARCHAR(64)  NOT NULL,
    password_hash   VARCHAR(256) NOT NULL,
    Status          TINYINT      NULL DEFAULT (1),
    IsDeleted       TINYINT      NULL DEFAULT (0),
    CreatedBy       VARCHAR(32)  NULL,
    CreatedDate     DATETIME2    NULL DEFAULT (SYSDATETIME()),
    LastUpdatedBy   VARCHAR(32)  NULL,
    LastUpdatedDate DATETIME2    NULL,
    CONSTRAINT PK_ADMIN PRIMARY KEY (CompanyCode, id_admin),
    CONSTRAINT UQ_ADMIN_username UNIQUE (CompanyCode, username)
);
GO

-- ================= PENGGUNA =================
IF OBJECT_ID(N'dbo.PENGGUNA', N'U') IS NULL
CREATE TABLE dbo.PENGGUNA (
    CompanyCode     VARCHAR(32)  NOT NULL,
    id_pengguna     INT          NOT NULL,
    id_admin        INT          NOT NULL,
    nama            VARCHAR(100) NOT NULL,
    nim_nip         VARCHAR(32)  NULL,
    jenis_kelamin   VARCHAR(16)  NULL,
    umur            INT          NULL,
    kontak          VARCHAR(64)  NULL,
    Status          TINYINT      NULL DEFAULT (1),
    IsDeleted       TINYINT      NULL DEFAULT (0),
    CreatedBy       VARCHAR(32)  NULL,
    CreatedDate     DATETIME2    NULL DEFAULT (SYSDATETIME()),
    LastUpdatedBy   VARCHAR(32)  NULL,
    LastUpdatedDate DATETIME2    NULL,
    CONSTRAINT PK_PENGGUNA PRIMARY KEY (CompanyCode, id_pengguna),
    CONSTRAINT FK_PENGGUNA_ADMIN FOREIGN KEY (CompanyCode, id_admin)
        REFERENCES dbo.ADMIN (CompanyCode, id_admin),
    CONSTRAINT CK_PENGGUNA_gender CHECK (jenis_kelamin IN ('L', 'P')),
    CONSTRAINT CK_PENGGUNA_umur CHECK (umur IS NULL OR umur >= 0)
);
GO

-- ================= PINTU =================
IF OBJECT_ID(N'dbo.PINTU', N'U') IS NULL
CREATE TABLE dbo.PINTU (
    CompanyCode     VARCHAR(32)  NOT NULL,
    id_pintu        INT          NOT NULL,
    nama_pintu      VARCHAR(100) NOT NULL,
    durasi_buka     INT          NULL DEFAULT (5),
    total_buka      INT          NOT NULL DEFAULT (0),
    batas_servis    INT          NULL,
    servis_tiap_hari INT         NULL,
    servis_terakhir DATETIME2    NULL,
    Status          TINYINT      NULL DEFAULT (1),
    IsDeleted       TINYINT      NULL DEFAULT (0),
    CreatedBy       VARCHAR(32)  NULL,
    CreatedDate     DATETIME2    NULL DEFAULT (SYSDATETIME()),
    LastUpdatedBy   VARCHAR(32)  NULL,
    LastUpdatedDate DATETIME2    NULL,
    CONSTRAINT PK_PINTU PRIMARY KEY (CompanyCode, id_pintu),
    CONSTRAINT CK_PINTU_batas CHECK (batas_servis IS NULL OR batas_servis >= 0),
    CONSTRAINT CK_PINTU_hari CHECK (servis_tiap_hari IS NULL OR servis_tiap_hari >= 0),
    CONSTRAINT CK_PINTU_total CHECK (total_buka >= 0)
);
GO

-- ================= KARTU_RFID =================
IF OBJECT_ID(N'dbo.KARTU_RFID', N'U') IS NULL
CREATE TABLE dbo.KARTU_RFID (
    CompanyCode     VARCHAR(32)  NOT NULL,
    id_kartu        INT          NOT NULL,
    id_pengguna     INT          NOT NULL,
    uid_kartu       VARCHAR(32)  NOT NULL,
    berlaku_sampai  DATETIME2    NULL,
    maks_pakai      INT          NULL,
    total_pakai     INT          NOT NULL DEFAULT (0),
    Status          TINYINT      NULL DEFAULT (1),
    IsDeleted       TINYINT      NULL DEFAULT (0),
    CreatedBy       VARCHAR(32)  NULL,
    CreatedDate     DATETIME2    NULL DEFAULT (SYSDATETIME()),
    LastUpdatedBy   VARCHAR(32)  NULL,
    LastUpdatedDate DATETIME2    NULL,
    CONSTRAINT PK_KARTU_RFID PRIMARY KEY (CompanyCode, id_kartu),
    CONSTRAINT FK_KARTU_PENGGUNA FOREIGN KEY (CompanyCode, id_pengguna)
        REFERENCES dbo.PENGGUNA (CompanyCode, id_pengguna),
    CONSTRAINT UQ_KARTU_uid UNIQUE (CompanyCode, uid_kartu),
    CONSTRAINT CK_KARTU_maks CHECK (maks_pakai IS NULL OR maks_pakai >= 0),
    CONSTRAINT CK_KARTU_total CHECK (total_pakai >= 0)
);
GO

-- ================= HAK_AKSES =================
IF OBJECT_ID(N'dbo.HAK_AKSES', N'U') IS NULL
CREATE TABLE dbo.HAK_AKSES (
    CompanyCode     VARCHAR(32) NOT NULL,
    id_akses        INT         NOT NULL,
    id_kartu        INT         NOT NULL,
    id_pintu        INT         NOT NULL,
    Status          TINYINT     NULL DEFAULT (1),
    IsDeleted       TINYINT     NULL DEFAULT (0),
    CreatedBy       VARCHAR(32) NULL,
    CreatedDate     DATETIME2   NULL DEFAULT (SYSDATETIME()),
    LastUpdatedBy   VARCHAR(32) NULL,
    LastUpdatedDate DATETIME2   NULL,
    CONSTRAINT PK_HAK_AKSES PRIMARY KEY (CompanyCode, id_akses),
    CONSTRAINT FK_HAK_KARTU FOREIGN KEY (CompanyCode, id_kartu)
        REFERENCES dbo.KARTU_RFID (CompanyCode, id_kartu),
    CONSTRAINT FK_HAK_PINTU FOREIGN KEY (CompanyCode, id_pintu)
        REFERENCES dbo.PINTU (CompanyCode, id_pintu),
    CONSTRAINT UQ_HAK_kartu_pintu UNIQUE (CompanyCode, id_kartu, id_pintu)
);
GO

-- ================= LOG_AKSES =================
-- id_kartu boleh NULL untuk kartu asing (tap tidak dikenal tetap tercatat).
IF OBJECT_ID(N'dbo.LOG_AKSES', N'U') IS NULL
CREATE TABLE dbo.LOG_AKSES (
    CompanyCode     VARCHAR(32)  NOT NULL,
    id_log          INT IDENTITY(1,1) NOT NULL,
    id_kartu        INT          NULL,
    id_pintu        INT          NOT NULL,
    uid_dipindai    VARCHAR(32)  NOT NULL,
    waktu           DATETIME2    NOT NULL DEFAULT (SYSDATETIME()),
    hasil           VARCHAR(16)  NOT NULL,
    alasan          VARCHAR(32)  NULL,
    Status          TINYINT      NULL DEFAULT (1),
    IsDeleted       TINYINT      NULL DEFAULT (0),
    CreatedBy       VARCHAR(32)  NULL,
    CreatedDate     DATETIME2    NULL DEFAULT (SYSDATETIME()),
    LastUpdatedBy   VARCHAR(32)  NULL,
    LastUpdatedDate DATETIME2    NULL,
    CONSTRAINT PK_LOG_AKSES PRIMARY KEY (CompanyCode, id_log),
    CONSTRAINT FK_LOG_PINTU FOREIGN KEY (CompanyCode, id_pintu)
        REFERENCES dbo.PINTU (CompanyCode, id_pintu),
    CONSTRAINT CK_LOG_hasil CHECK (hasil IN ('diterima', 'ditolak'))
);
GO
-- FK id_kartu sengaja tidak dibuat: NULL untuk kartu asing + hindari konflik hapus kartu.
CREATE INDEX IX_LOG_uid_waktu ON dbo.LOG_AKSES (CompanyCode, uid_dipindai, waktu);
GO
