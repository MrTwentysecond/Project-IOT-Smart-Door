#include <iostream>
#include <string>
#include <ctime>
#include "sqlite3.h"

using namespace std;

// Helper untuk format waktu ISO 8601
string getWaktuISO() {
    time_t now = time(0);
    tm *ltm = localtime(&now);
    char buf[30];
    strftime(buf, sizeof(buf), "%Y-%m-%dT%H:%M:%SZ", ltm);
    return string(buf);
}

// 1. Mencatat Log Akses (Mendukung 7 Default Fields)
void simpanLog(sqlite3* db, int idKartu, int idPintu, string uid, string hasil) {
    string waktuSekarang = getWaktuISO();
    
    // Cari id_log berikutnya
    string sqlMax = "SELECT COALESCE(MAX(id_log), 0) + 1 FROM LOG_AKSES WHERE CompanyCode = 'KEL4';";
    sqlite3_stmt* stmt;
    int idLogBaru = 1;
    if (sqlite3_prepare_v2(db, sqlMax.c_str(), -1, &stmt, 0) == SQLITE_OK) {
        if (sqlite3_step(stmt) == SQLITE_ROW) {
            idLogBaru = sqlite3_column_int(stmt, 0);
        }
    }
    sqlite3_finalize(stmt);

    string sql = "INSERT INTO LOG_AKSES (CompanyCode, id_log, id_kartu, id_pintu, uid_dipindai, waktu, hasil, Status, IsDeleted, CreatedBy, CreatedDate) VALUES ('KEL4', " +
                 to_string(idLogBaru) + ", " +
                 (idKartu == 0 ? "NULL" : to_string(idKartu)) + ", " +
                 to_string(idPintu) + ", '" + uid + "', '" + waktuSekarang + "', '" + hasil + "', 1, 0, 'system', '" + waktuSekarang + "');";
    
    char* errMsg = 0;
    if (sqlite3_exec(db, sql.c_str(), 0, 0, &errMsg) != SQLITE_OK) {
        cerr << "[ERROR LOG]: " << errMsg << endl;
        sqlite3_free(errMsg);
    }
}

// 2. Verifikasi Kartu (Memeriksa Status = 1 dan IsDeleted = 0)
string cekKartu(sqlite3* db, string uid, int idPintu) {
    string query = "SELECT K.id_kartu FROM KARTU_RFID K "
                   "JOIN HAK_AKSES H ON K.id_kartu = H.id_kartu AND K.CompanyCode = H.CompanyCode "
                   "WHERE K.CompanyCode = 'KEL4' "
                   "AND K.uid_kartu = '" + uid + "' "
                   "AND K.Status = 1 AND K.IsDeleted = 0 "
                   "AND H.id_pintu = " + to_string(idPintu) + " "
                   "AND H.Status = 1 AND H.IsDeleted = 0;";

    sqlite3_stmt* stmt;
    int rc = sqlite3_prepare_v2(db, query.c_str(), -1, &stmt, 0);

    if (rc == SQLITE_OK && sqlite3_step(stmt) == SQLITE_ROW) {
        int idKartu = sqlite3_column_int(stmt, 0);
        sqlite3_finalize(stmt);
        
        simpanLog(db, idKartu, idPintu, uid, "diterima");
        return "OPEN";
    }

    sqlite3_finalize(stmt);
    simpanLog(db, 0, idPintu, uid, "ditolak");
    return "DENY";
}

// 3. Tambah Kartu RFID Baru dengan Default Fields
void tambahKartu(sqlite3* db) {
    string uid, pemegang;
    cout << "\n--- TAMBAH KARTU BARU ---" << endl;
    cout << "Masukkan UID Kartu Baru : ";
    cin >> uid;
    cout << "Masukkan Nama Pemegang  : ";
    cin.ignore();
    getline(cin, pemegang);

    string waktuSekarang = getWaktuISO();

    // Hitung ID Pengguna Baru
    int idPenggunaBaru = 1;
    string sqlMaxPengguna = "SELECT COALESCE(MAX(id_pengguna), 0) + 1 FROM PENGGUNA WHERE CompanyCode = 'KEL4';";
    sqlite3_stmt* stmt;
    if (sqlite3_prepare_v2(db, sqlMaxPengguna.c_str(), -1, &stmt, 0) == SQLITE_OK) {
        if (sqlite3_step(stmt) == SQLITE_ROW) idPenggunaBaru = sqlite3_column_int(stmt, 0);
    }
    sqlite3_finalize(stmt);

    // Hitung ID Kartu Baru
    int idKartuBaru = 1;
    string sqlMaxKartu = "SELECT COALESCE(MAX(id_kartu), 0) + 1 FROM KARTU_RFID WHERE CompanyCode = 'KEL4';";
    if (sqlite3_prepare_v2(db, sqlMaxKartu.c_str(), -1, &stmt, 0) == SQLITE_OK) {
        if (sqlite3_step(stmt) == SQLITE_ROW) idKartuBaru = sqlite3_column_int(stmt, 0);
    }
    sqlite3_finalize(stmt);

    // Hitung ID Akses Baru
    int idAksesBaru = 1;
    string sqlMaxAkses = "SELECT COALESCE(MAX(id_akses), 0) + 1 FROM HAK_AKSES WHERE CompanyCode = 'KEL4';";
    if (sqlite3_prepare_v2(db, sqlMaxAkses.c_str(), -1, &stmt, 0) == SQLITE_OK) {
        if (sqlite3_step(stmt) == SQLITE_ROW) idAksesBaru = sqlite3_column_int(stmt, 0);
    }
    sqlite3_finalize(stmt);

    // Insert ke PENGGUNA
    string sqlPengguna = "INSERT INTO PENGGUNA VALUES ('KEL4', " + to_string(idPenggunaBaru) + ", 1, '" + pemegang + "', NULL, 1, 0, 'admin', '" + waktuSekarang + "', NULL, NULL);";
    // Insert ke KARTU_RFID
    string sqlKartu = "INSERT INTO KARTU_RFID VALUES ('KEL4', " + to_string(idKartuBaru) + ", " + to_string(idPenggunaBaru) + ", '" + uid + "', 1, 0, 'admin', '" + waktuSekarang + "', NULL, NULL);";
    // Insert ke HAK_AKSES
    string sqlAkses = "INSERT INTO HAK_AKSES VALUES ('KEL4', " + to_string(idAksesBaru) + ", " + to_string(idKartuBaru) + ", 1, 1, 0, 'admin', '" + waktuSekarang + "', NULL, NULL);";

    char* errMsg = 0;
    if (sqlite3_exec(db, sqlPengguna.c_str(), 0, 0, &errMsg) == SQLITE_OK &&
        sqlite3_exec(db, sqlKartu.c_str(), 0, 0, &errMsg) == SQLITE_OK &&
        sqlite3_exec(db, sqlAkses.c_str(), 0, 0, &errMsg) == SQLITE_OK) {
        cout << "[BERHASIL] Kartu " << uid << " (" << pemegang << ") terdaftar dengan Default Fields lengkap!\n" << endl;
    } else {
        cout << "[GAGAL] Terjadi kesalahan saat menambah data.\n" << endl;
    }
}

int main() {
    sqlite3* db;
    if (sqlite3_open("smartdoor.db", &db)) {
        cerr << "Gagal membuka database!" << endl;
        return 1;
    }

    string inputMenu;
    while (true) {
        cout << "==========================================" << endl;
        cout << "  SMART DOOR BACKEND  " << endl;
        cout << "==========================================" << endl;
        cout << "1. Simulasi Tap Kartu (Serial In)" << endl;
        cout << "2. Tambah Kartu RFID Baru" << endl;
        cout << "3. Keluar Program" << endl;
        cout << "Pilih menu (1-3): ";
        cin >> inputMenu;

        if (inputMenu == "1") {
            string inputUID;
            cout << "\n[SIMULASI TAP RFID] Ketik UID (atau 'back' untuk kembali):" << endl;
            while (true) {
                cout << "Tap RFID >> ";
                cin >> inputUID;
                if (inputUID == "back" || inputUID == "BACK") break;

                string respon = cekKartu(db, inputUID, 1);
                cout << ">>> Respon Hardware -> [" << respon << "]\n" << endl;
            }
        } 
        else if (inputMenu == "2") {
            tambahKartu(db);
        } 
        else if (inputMenu == "3") {
            cout << "\nProgram ditutup." << endl;
            break;
        }
    }

    sqlite3_close(db);
    return 0;
}