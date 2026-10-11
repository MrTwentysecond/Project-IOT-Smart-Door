namespace SmartDoor.Backend.Models;

// PINTU: data pintu + lama solenoid terbuka. PK = (CompanyCode, IdPintu).
// TotalBuka naik tiap tap diterima; BatasServis = servis tiap N buka,
// ServisTiapHari = servis tiap N hari meski jarang dipakai.
public sealed record Pintu(
    string CompanyCode,
    int IdPintu,
    string NamaPintu,
    int DurasiBuka,
    int TotalBuka,
    int? BatasServis,
    int? ServisTiapHari,
    DateTime? ServisTerakhir,
    byte? Status,
    byte? IsDeleted,
    string? CreatedBy,
    DateTime? CreatedDate,
    string? LastUpdatedBy,
    DateTime? LastUpdatedDate)
    : BaseEntity(CompanyCode, Status, IsDeleted, CreatedBy, CreatedDate, LastUpdatedBy, LastUpdatedDate);
