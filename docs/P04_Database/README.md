# Pertemuan 4: Database, Model Data & Default Field

## 7 Default Field (wajib di setiap tabel)
| Field | Spesifikasi dosen | Tipe di SQLite | Contoh isi |
|---|---|---|---|
| CompanyCode | PK, varchar(32), not null | TEXT NOT NULL | KEL4 |
| Status | tinyint, null | INTEGER DEFAULT 1 | 1 = aktif, 0 = nonaktif |
| IsDeleted | tinyint, null | INTEGER DEFAULT 0 | 1 = sudah dihapus |
| CreatedBy | varchar(32), null | TEXT | admin |
| CreatedDate | datetime, null | TEXT (ISO 8601) | 2026-10-07 10:21:00 |
| LastUpdatedBy | varchar(32), null | TEXT | admin |
| LastUpdatedDate | datetime, null | TEXT (ISO 8601) | 2026-10-07 11:05:00 |

Primary key setiap tabel: gabungan CompanyCode + ID tabel.

## Tabel dan relasi
- ADMIN (id_admin, username, password_hash)
- PENGGUNA (id_pengguna, id_admin, nama, nim_nip)
- KARTU_RFID (id_kartu, id_pengguna, uid_kartu)
- PINTU (id_pintu, nama_pintu, durasi_buka)
- HAK_AKSES (id_akses, id_kartu, id_pintu)
- LOG_AKSES (id_log, id_kartu, id_pintu, uid_dipindai, waktu, hasil)

Relasi (semua 1 ke N): ADMIN-PENGGUNA, PENGGUNA-KARTU_RFID, KARTU_RFID-HAK_AKSES, PINTU-HAK_AKSES, KARTU_RFID-LOG_AKSES, PINTU-LOG_AKSES.
id_kartu di LOG_AKSES boleh kosong untuk kartu asing.

## Aturan pengisian default field
- Tambah data: CompanyCode, Status=1, IsDeleted=0, CreatedBy, CreatedDate.
- Ubah data: LastUpdatedBy, LastUpdatedDate.
- Hapus data: soft delete (IsDeleted=1), data tidak dihapus permanen.

## Bukti
- bukti_struktur_db.png: struktur tabel di DB Browser for SQLite
- bukti_data_admin.png: data tabel ADMIN di SQLite Viewer
