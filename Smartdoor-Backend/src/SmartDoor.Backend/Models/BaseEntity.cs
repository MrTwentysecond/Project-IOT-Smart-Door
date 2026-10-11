namespace SmartDoor.Backend.Models;

// 7 default field wajib (ketentuan dosen, slide Pertemuan 4).
// CompanyCode = bagian PK gabungan tiap tabel (contoh: "KEL4").
public abstract record BaseEntity(
    string CompanyCode,
    byte? Status,
    byte? IsDeleted,
    string? CreatedBy,
    DateTime? CreatedDate,
    string? LastUpdatedBy,
    DateTime? LastUpdatedDate);
