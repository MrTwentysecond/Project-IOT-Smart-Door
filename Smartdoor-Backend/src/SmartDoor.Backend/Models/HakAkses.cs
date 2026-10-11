namespace SmartDoor.Backend.Models;

// HAK_AKSES: izin kartu untuk pintu tertentu. PK = (CompanyCode, IdAkses).
public sealed record HakAkses(
    string CompanyCode,
    int IdAkses,
    int IdKartu,
    int IdPintu,
    byte? Status,
    byte? IsDeleted,
    string? CreatedBy,
    DateTime? CreatedDate,
    string? LastUpdatedBy,
    DateTime? LastUpdatedDate)
    : BaseEntity(CompanyCode, Status, IsDeleted, CreatedBy, CreatedDate, LastUpdatedBy, LastUpdatedDate);
