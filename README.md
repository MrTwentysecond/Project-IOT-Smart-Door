# Project IOT Smart Door

Sistem pintu pintar (Smart Door Access) yang dibuka dengan kartu RFID.
Tugas kelompok mata kuliah Internet of Things (Kelompok 4).

## Cara kerja singkat
1. Kartu RFID di-tap pada modul RC522.
2. Arduino Uno mengirim UID ke komputer lewat Serial (`UID:xxxx`).
3. Aplikasi desktop mencocokkan UID dengan database SQLite dan membalas `OPEN` atau `DENY`.
4. Arduino menjalankan aksi: LED hijau, buzzer 1x, solenoid terbuka 5 detik (diterima) atau LED merah dan buzzer 3x (ditolak).
5. Setiap tap tercatat di tabel `LOG_AKSES`.

## Perangkat
Arduino Uno, modul RFID RC522, buzzer, LED merah/hijau, relay 1 channel, solenoid door lock (catu daya terpisah + dioda flyback).

## Struktur folder
| Folder | Isi | Penanggung jawab |
|---|---|---|
| `firmware/` | Kode Arduino (PlatformIO) | Bayu |
| `app/` | Kode aplikasi desktop (Visual Studio 2026) | Ibnu (UI), Falzen (backend) |
| `database/` | Script SQL dan skema database SQLite | Falzen |
| `installer/` | Script Inno Setup (`.iss`) | Suripto |
| `docs/` | Dokumentasi, ERD, panduan, laporan pengujian | Wahyu, Aria |

## Tim
| Nama | Peran |
|---|---|
| Bayu Putra Setia Eka Premana | Project Manager & Embedded Developer |
| Aria Julianto Wasnita | Hardware / IoT Engineer |
| Ibnu Fatur Rahman | Frontend Developer |
| Muhamad Falzen Ridwan | Backend Developer & Database Admin |
| Suripto | DevOps / Deployment Engineer |
| Wahyu Pahrudin | QA Tester & Technical Writer |

## Aturan kerja
- Jangan commit hasil build (`*.exe`, `bin/`, `obj/`) atau password. File `.gitignore` sudah menanganinya.
- Setup.exe diunggah lewat fitur **Releases**, bukan dimasukkan ke kode.
- Setiap anggota bekerja di branch sendiri (misalnya `falzen/database`), lalu dibuat Pull Request ke `main`.
- Pesan commit singkat dan jelas, misalnya `database: tambah default field`.
