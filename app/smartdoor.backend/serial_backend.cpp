#include <iostream>
#include <string>
#include <windows.h>
#include "sqlite3.h"

using namespace std;

// 1. Fungsi mencatat log ke database
void simpanLog(sqlite3* db, int idKartu, int idPintu, string uid, string hasil) {
    string sql = "INSERT INTO LOG_AKSES (id_kartu, id_pintu, uid_dipindai, hasil) VALUES (" +
                 (idKartu == 0 ? "NULL" : to_string(idKartu)) + ", " +
                 to_string(idPintu) + ", '" + uid + "', '" + hasil + "');";
    
    char* errMsg = 0;
    if (sqlite3_exec(db, sql.c_str(), 0, 0, &errMsg) != SQLITE_OK) {
        cerr << "[ERROR LOG]: " << errMsg << endl;
        sqlite3_free(errMsg);
    }
}

// 2. Fungsi verifikasi kartu RFID
string cekKartu(sqlite3* db, string uid, int idPintu) {
    string query = "SELECT K.id_kartu FROM KARTU_RFID K "
                   "JOIN HAK_AKSES H ON K.id_kartu = H.id_kartu "
                   "WHERE K.uid_kartu = '" + uid + "' "
                   "AND K.status = 'aktif' "
                   "AND H.id_pintu = " + to_string(idPintu) + " "
                   "AND H.status = 'izinkan';";

    sqlite3_stmt* stmt;
    int rc = sqlite3_prepare_v2(db, query.c_str(), -1, &stmt, 0);

    if (rc == SQLITE_OK && sqlite3_step(stmt) == SQLITE_ROW) {
        int idKartu = sqlite3_column_int(stmt, 0);
        sqlite3_finalize(stmt);
        
        simpanLog(db, idKartu, idPintu, uid, "OPEN");
        return "OPEN";
    }

    sqlite3_finalize(stmt);
    simpanLog(db, 0, idPintu, uid, "DENY");
    return "DENY";
}

int main() {
    sqlite3* db;
    if (sqlite3_open("smartdoor.db", &db)) {
        cerr << "Gagal membuka database!" << endl;
        return 1;
    }

    // Tentukan Nama Port COM tempat ESP32/Arduino colok (Contoh: COM3)
    string portName = "\\\\.\\COM3"; 
    
    cout << "Membuka Serial Port " << portName << "..." << endl;

    HANDLE hSerial = CreateFileA(portName.c_str(),
                                GENERIC_READ | GENERIC_WRITE,
                                0,
                                NULL,
                                OPEN_EXISTING,
                                FILE_ATTRIBUTE_NORMAL,
                                NULL);

    if (hSerial == INVALID_HANDLE_VALUE) {
        cerr << "[ERROR] Gagal membuka Port Serial! Pastikan ESP32 sudah tancap dan Port COM sesuai." << endl;
        sqlite3_close(db);
        return 1;
    }

    // Konfigurasi Baud Rate (9600 / 115200)
    DCB dcbSerialParams = {0};
    dcbSerialParams.DCBlength = sizeof(dcbSerialParams);

    if (!GetCommState(hSerial, &dcbSerialParams)) {
        cerr << "[ERROR] Gagal membaca state serial." << endl;
        CloseHandle(hSerial);
        return 1;
    }

    dcbSerialParams.BaudRate = CBR_9600; // Samakan dengan Serial.begin(9600) di Arduino/ESP32
    dcbSerialParams.ByteSize = 8;
    dcbSerialParams.StopBits = ONESTOPBIT;
    dcbSerialParams.Parity   = NOPARITY;

    SetCommState(hSerial, &dcbSerialParams);

    cout << "==================================================" << endl;
    cout << "     BACKEND SMART DOOR - SERIAL LISTENER ONLINE  " << endl;
    cout << "==================================================" << endl;
    cout << "Menunggu pembacaan Tap RFID dari Hardware...\n" << endl;

    char buffer[128];
    DWORD bytesRead, bytesWritten;
    string receivedData = "";

    while (true) {
        if (ReadFile(hSerial, buffer, sizeof(buffer) - 1, &bytesRead, NULL) && bytesRead > 0) {
            buffer[bytesRead] = '\0';
            for (int i = 0; i < bytesRead; i++) {
                if (buffer[i] == '\n' || buffer[i] == '\r') {
                    if (!receivedData.empty()) {
                        cout << "[HARDWARE IN] UID Terbaca: " << receivedData << endl;
                        
                        // Verifikasi ke Database
                        string respon = cekKartu(db, receivedData, 1);
                        cout << "[BACKEND OUT] Respon: " << respon << endl;

                        // Kirim balasan balik ke Hardware via Serial
                        string outMessage = respon + "\n";
                        WriteFile(hSerial, outMessage.c_str(), outMessage.length(), &bytesWritten, NULL);

                        receivedData = "";
                    }
                } else {
                    receivedData += buffer[i];
                }
            }
        }
        Sleep(50); // Mencegah CPU usage terlalu tinggi
    }

    CloseHandle(hSerial);
    sqlite3_close(db);
    return 0;
}