namespace SmartDoor.Backend.Models;

// LOG_AKSES: catatan tiap tap. IdKartu = null untuk kartu asing.
// Hasil = "diterima" atau "ditolak". PK = (CompanyCode, IdLog).
public sealed record LogAkses(
    string CompanyCode,
    int IdLog,
    int? IdKartu,
    int IdPintu,
    string UidDipindai,
    DateTime Waktu,
    string Hasil,
    byte? Status,
    byte? IsDeleted,
    string? CreatedBy,
    DateTime? CreatedDate,
    string? LastUpdatedBy,
    DateTime? LastUpdatedDate)
    : BaseEntity(CompanyCode, Status, IsDeleted, CreatedBy, CreatedDate, LastUpdatedBy, LastUpdatedDate);
