namespace SmartDoor.Backend.Models;

// ADMIN: akun yang login ke aplikasi. PK = (CompanyCode, IdAdmin).
public sealed record Admin(
    string CompanyCode,
    int IdAdmin,
    string Username,
    string PasswordHash,
    byte? Status,
    byte? IsDeleted,
    string? CreatedBy,
    DateTime? CreatedDate,
    string? LastUpdatedBy,
    DateTime? LastUpdatedDate)
    : BaseEntity(CompanyCode, Status, IsDeleted, CreatedBy, CreatedDate, LastUpdatedBy, LastUpdatedDate);
