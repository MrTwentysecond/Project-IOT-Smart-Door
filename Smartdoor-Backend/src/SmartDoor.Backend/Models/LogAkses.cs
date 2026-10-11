namespace SmartDoor.Backend.Models;

// LOG_AKSES: catatan tiap tap. IdKartu = null untuk kartu asing.
// Hasil = "diterima" atau "ditolak". PK = (CompanyCode, IdLog).
// Alasan: "tanpa_akses"/"kartu_asing"/"expired"/"kuota_habis" saat ditolak,
// "perlu_servis" saat diterima tapi pintu butuh servis berkala.
public sealed record LogAkses(
    string CompanyCode,
    int IdLog,
    int? IdKartu,
    int IdPintu,
    string UidDipindai,
    DateTime Waktu,
    string Hasil,
    string? Alasan,
    byte? Status,
    byte? IsDeleted,
    string? CreatedBy,
    DateTime? CreatedDate,
    string? LastUpdatedBy,
    DateTime? LastUpdatedDate)
    : BaseEntity(CompanyCode, Status, IsDeleted, CreatedBy, CreatedDate, LastUpdatedBy, LastUpdatedDate);
