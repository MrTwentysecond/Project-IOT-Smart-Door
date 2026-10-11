namespace SmartDoor.Backend.Models;

// PENGGUNA: pemilik kartu (nama, NIM/NIP). PK = (CompanyCode, IdPengguna).
public sealed record Pengguna(
    string CompanyCode,
    int IdPengguna,
    int IdAdmin,
    string Nama,
    string? NimNip,
    byte? Status,
    byte? IsDeleted,
    string? CreatedBy,
    DateTime? CreatedDate,
    string? LastUpdatedBy,
    DateTime? LastUpdatedDate)
    : BaseEntity(CompanyCode, Status, IsDeleted, CreatedBy, CreatedDate, LastUpdatedBy, LastUpdatedDate);
