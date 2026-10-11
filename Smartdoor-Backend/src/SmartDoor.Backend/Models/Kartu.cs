namespace SmartDoor.Backend.Models;

// Kartu RFID terdaftar. Uid = string hex dari Arduino (tanpa prefix "UID:").
public sealed record Kartu(string Uid, string NamaPemilik, bool Aktif);
