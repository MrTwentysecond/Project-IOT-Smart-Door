-- Expired kartu akses + jadwal servis berkala PINTU.
-- Rerunnable: semua ALTER dijaga COL_LENGTH / DEFAULT.
USE SmartDoor;
GO

-- KARTU_RFID: berlaku_sampai (tanggal), maks_pakai (kuota tap), total_pakai (counter).
IF COL_LENGTH(N'dbo.KARTU_RFID', N'berlaku_sampai') IS NULL
    ALTER TABLE dbo.KARTU_RFID ADD berlaku_sampai DATETIME2 NULL;
IF COL_LENGTH(N'dbo.KARTU_RFID', N'maks_pakai') IS NULL
    ALTER TABLE dbo.KARTU_RFID ADD maks_pakai INT NULL;
IF COL_LENGTH(N'dbo.KARTU_RFID', N'total_pakai') IS NULL
    ALTER TABLE dbo.KARTU_RFID ADD total_pakai INT NOT NULL
        CONSTRAINT DF_KARTU_total DEFAULT (0);
GO

-- PINTU: total_buka (counter tiap dibuka), batas_servis (tiap N buka),
-- servis_tiap_hari (tiap N hari meski jarang dipakai), servis_terakhir.
IF COL_LENGTH(N'dbo.PINTU', N'total_buka') IS NULL
    ALTER TABLE dbo.PINTU ADD total_buka INT NOT NULL
        CONSTRAINT DF_PINTU_total DEFAULT (0);
IF COL_LENGTH(N'dbo.PINTU', N'batas_servis') IS NULL
    ALTER TABLE dbo.PINTU ADD batas_servis INT NULL;
IF COL_LENGTH(N'dbo.PINTU', N'servis_tiap_hari') IS NULL
    ALTER TABLE dbo.PINTU ADD servis_tiap_hari INT NULL;
IF COL_LENGTH(N'dbo.PINTU', N'servis_terakhir') IS NULL
    ALTER TABLE dbo.PINTU ADD servis_terakhir DATETIME2 NULL;
GO

-- LOG_AKSES.alasan: kenapa ditolak (expired / kuota_habis / tanpa_akses / kartu_asing),
-- atau info servis (perlu_servis) pada tap yang diterima.
IF COL_LENGTH(N'dbo.LOG_AKSES', N'alasan') IS NULL
    ALTER TABLE dbo.LOG_AKSES ADD alasan VARCHAR(32) NULL;
GO

-- Seed: kartu contoh 1 tahun + kuota 1000 tap; pintu servis tiap 500 buka / 90 hari.
UPDATE dbo.KARTU_RFID
SET berlaku_sampai = DATEADD(YEAR, 1, SYSDATETIME()),
    maks_pakai = 1000
WHERE CompanyCode = 'KEL4' AND id_kartu = 1 AND berlaku_sampai IS NULL;
UPDATE dbo.PINTU
SET batas_servis = 500, servis_tiap_hari = 90, servis_terakhir = SYSDATETIME()
WHERE CompanyCode = 'KEL4' AND id_pintu = 1 AND batas_servis IS NULL;
GO
