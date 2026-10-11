namespace SmartDoor.Backend.Models;

// Satu baris tap kartu. Hasil = "OPEN" atau "DENY".
public sealed record LogAkses(int Id, string Uid, DateTime Waktu, string Hasil);
