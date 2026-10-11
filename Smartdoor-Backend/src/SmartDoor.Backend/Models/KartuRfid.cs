namespace SmartDoor.Backend.Models;

// KARTU_RFID: kartu + UID hex dari RC522. PK = (CompanyCode, IdKartu).
// BerlakuSampai = NULL berarti tanpa batas waktu; MaksPakai = NULL berarti tanpa kuota.
public sealed record KartuRfid(
    string CompanyCode,
    int IdKartu,
    int IdPengguna,
    string UidKartu,
    DateTime? BerlakuSampai,
    int? MaksPakai,
    int TotalPakai,
    byte? Status,
    byte? IsDeleted,
    string? CreatedBy,
    DateTime? CreatedDate,
    string? LastUpdatedBy,
    DateTime? LastUpdatedDate)
    : BaseEntity(CompanyCode, Status, IsDeleted, CreatedBy, CreatedDate, LastUpdatedBy, LastUpdatedDate);
