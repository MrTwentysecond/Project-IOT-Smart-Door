-- Falzen: skema SQLite. TODO: sesuaikan dengan revisi_schema.sql lama.
CREATE TABLE IF NOT EXISTS KARTU (
    uid TEXT PRIMARY KEY,
    nama_pemilik TEXT NOT NULL,
    aktif INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS LOG_AKSES (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    uid TEXT NOT NULL,
    waktu TEXT NOT NULL DEFAULT (datetime('now','localtime')),
    hasil TEXT NOT NULL CHECK (hasil IN ('OPEN','DENY'))
);
