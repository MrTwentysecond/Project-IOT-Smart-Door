namespace SmartDoor.Backend.Models;

// PINTU: data pintu + lama solenoid terbuka. PK = (CompanyCode, IdPintu).
public sealed record Pintu(
    string CompanyCode,
    int IdPintu,
    string NamaPintu,
    int DurasiBuka,
    byte? Status,
    byte? IsDeleted,
    string? CreatedBy,
    DateTime? CreatedDate,
    string? LastUpdatedBy,
    DateTime? LastUpdatedDate)
    : BaseEntity(CompanyCode, Status, IsDeleted, CreatedBy, CreatedDate, LastUpdatedBy, LastUpdatedDate);
