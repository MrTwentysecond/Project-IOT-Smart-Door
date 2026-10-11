namespace SmartDoor.Backend.Models;

// PENGGUNA: pemilik kartu. Kontak wajib diisi saat pendaftaran (validasi di backend).
public sealed record Pengguna(
    string CompanyCode,
    int IdPengguna,
    int IdAdmin,
    string Nama,
    string? NimNip,
    string? JenisKelamin,
    int? Umur,
    string? Kontak,
    byte? Status,
    byte? IsDeleted,
    string? CreatedBy,
    DateTime? CreatedDate,
    string? LastUpdatedBy,
    DateTime? LastUpdatedDate)
    : BaseEntity(CompanyCode, Status, IsDeleted, CreatedBy, CreatedDate, LastUpdatedBy, LastUpdatedDate);
