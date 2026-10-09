-- Hapus tabel lama jika ada
DROP TABLE IF EXISTS LOG_AKSES;
DROP TABLE IF EXISTS HAK_AKSES;
DROP TABLE IF EXISTS KARTU_RFID;
DROP TABLE IF EXISTS PENGGUNA;
DROP TABLE IF EXISTS PINTU;
DROP TABLE IF EXISTS ADMIN;

-- 1. TABEL ADMIN
CREATE TABLE ADMIN (
    CompanyCode TEXT NOT NULL,
    id_admin INTEGER NOT NULL,
    username TEXT NOT NULL,
    password_hash TEXT NOT NULL,
    Status INTEGER DEFAULT 1,
    IsDeleted INTEGER DEFAULT 0,
    CreatedBy TEXT,
    CreatedDate TEXT,
    LastUpdatedBy TEXT,
    LastUpdatedDate TEXT,
    PRIMARY KEY (CompanyCode, id_admin)
);

-- 2. TABEL PENGGUNA
CREATE TABLE PENGGUNA (
    CompanyCode TEXT NOT NULL,
    id_pengguna INTEGER NOT NULL,
    id_admin INTEGER NOT NULL,
    nama TEXT NOT NULL,
    nim_nip TEXT,
    Status INTEGER DEFAULT 1,
    IsDeleted INTEGER DEFAULT 0,
    CreatedBy TEXT,
    CreatedDate TEXT,
    LastUpdatedBy TEXT,
    LastUpdatedDate TEXT,
    PRIMARY KEY (CompanyCode, id_pengguna),
    FOREIGN KEY (CompanyCode, id_admin) REFERENCES ADMIN(CompanyCode, id_admin)
);

-- 3. TABEL KARTU_RFID
CREATE TABLE KARTU_RFID (
    CompanyCode TEXT NOT NULL,
    id_kartu INTEGER NOT NULL,
    id_pengguna INTEGER NOT NULL,
    uid_kartu TEXT NOT NULL,
    Status INTEGER DEFAULT 1,
    IsDeleted INTEGER DEFAULT 0,
    CreatedBy TEXT,
    CreatedDate TEXT,
    LastUpdatedBy TEXT,
    LastUpdatedDate TEXT,
    PRIMARY KEY (CompanyCode, id_kartu),
    FOREIGN KEY (CompanyCode, id_pengguna) REFERENCES PENGGUNA(CompanyCode, id_pengguna)
);

-- 4. TABEL PINTU
CREATE TABLE PINTU (
    CompanyCode TEXT NOT NULL,
    id_pintu INTEGER NOT NULL,
    nama_pintu TEXT NOT NULL,
    durasi_buka INTEGER DEFAULT 5,
    Status INTEGER DEFAULT 1,
    IsDeleted INTEGER DEFAULT 0,
    CreatedBy TEXT,
    CreatedDate TEXT,
    LastUpdatedBy TEXT,
    LastUpdatedDate TEXT,
    PRIMARY KEY (CompanyCode, id_pintu)
);

-- 5. TABEL HAK_AKSES
CREATE TABLE HAK_AKSES (
    CompanyCode TEXT NOT NULL,
    id_akses INTEGER NOT NULL,
    id_kartu INTEGER NOT NULL,
    id_pintu INTEGER NOT NULL,
    Status INTEGER DEFAULT 1,
    IsDeleted INTEGER DEFAULT 0,
    CreatedBy TEXT,
    CreatedDate TEXT,
    LastUpdatedBy TEXT,
    LastUpdatedDate TEXT,
    PRIMARY KEY (CompanyCode, id_akses),
    FOREIGN KEY (CompanyCode, id_kartu) REFERENCES KARTU_RFID(CompanyCode, id_kartu),
    FOREIGN KEY (CompanyCode, id_pintu) REFERENCES PINTU(CompanyCode, id_pintu)
);

-- 6. TABEL LOG_AKSES
CREATE TABLE LOG_AKSES (
    CompanyCode TEXT NOT NULL,
    id_log INTEGER NOT NULL,
    id_kartu INTEGER,
    id_pintu INTEGER NOT NULL,
    uid_dipindai TEXT NOT NULL,
    waktu TEXT NOT NULL,
    hasil TEXT NOT NULL,
    Status INTEGER DEFAULT 1,
    IsDeleted INTEGER DEFAULT 0,
    CreatedBy TEXT,
    CreatedDate TEXT,
    LastUpdatedBy TEXT,
    LastUpdatedDate TEXT,
    PRIMARY KEY (CompanyCode, id_log),
    FOREIGN KEY (CompanyCode, id_kartu) REFERENCES KARTU_RFID(CompanyCode, id_kartu),
    FOREIGN KEY (CompanyCode, id_pintu) REFERENCES PINTU(CompanyCode, id_pintu)
);

-- INSERT DATA DUMMY AWAL (COMPANY CODE: KEL4)
INSERT INTO ADMIN VALUES ('KEL4', 1, 'admin', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918', 1, 0, 'system', '2026-10-07T08:00:00Z', NULL, NULL);
INSERT INTO PENGGUNA VALUES ('KEL4', 1, 1, 'Muhamad Falzen Ridwan', '312410062', 1, 0, 'admin', '2026-10-07T08:00:00Z', NULL, NULL);
INSERT INTO KARTU_RFID VALUES ('KEL4', 1, 1, 'A1B2C3D4', 1, 0, 'admin', '2026-10-07T08:00:00Z', NULL, NULL);
INSERT INTO PINTU VALUES ('KEL4', 1, 'Pintu Utama Lab', 5, 1, 0, 'admin', '2026-10-07T08:00:00Z', NULL, NULL);
INSERT INTO HAK_AKSES VALUES ('KEL4', 1, 1, 1, 1, 0, 'admin', '2026-10-07T08:00:00Z', NULL, NULL);