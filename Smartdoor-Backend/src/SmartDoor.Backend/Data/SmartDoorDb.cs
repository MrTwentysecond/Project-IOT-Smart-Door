using Dapper;
using Microsoft.Data.SqlClient;
using SmartDoor.Backend.Models;

namespace SmartDoor.Backend.Data;

// Akses SQL Server via Dapper. CompanyCode = 'KEL4' (satu kelompok, satu database).
public sealed class SmartDoorDb(string connectionString)
{
    public const string Company = "KEL4";

    // Cari kartu aktif + punya hak akses ke pintu. Null = tolak.
    public async Task<(KartuRfid Kartu, int IdPintu)?> FindAksesAsync(
        string uid, int idPintu, CancellationToken ct = default)
    {
        const string sql = """
            SELECT k.CompanyCode, k.id_kartu AS IdKartu, k.id_pengguna AS IdPengguna,
                   k.uid_kartu AS UidKartu, k.Status, k.IsDeleted, k.CreatedBy,
                   k.CreatedDate, k.LastUpdatedBy, k.LastUpdatedDate,
                   h.id_pintu AS IdPintu
            FROM dbo.KARTU_RFID k
            JOIN dbo.HAK_AKSES h
              ON h.CompanyCode = k.CompanyCode AND h.id_kartu = k.id_kartu
            WHERE k.CompanyCode = @co AND k.uid_kartu = @uid
              AND h.id_pintu = @pintu
              AND ISNULL(k.Status, 1) = 1 AND ISNULL(k.IsDeleted, 0) = 0
              AND ISNULL(h.Status, 1) = 1 AND ISNULL(h.IsDeleted, 0) = 0
            """;
        await using var con = new SqlConnection(connectionString);
        await con.OpenAsync(ct).ConfigureAwait(false);
        var cmd = new CommandDefinition(sql,
            new { co = Company, uid, pintu = idPintu },
            cancellationToken: ct);
        var row = await con.QueryFirstOrDefaultAsync<KartuRow>(cmd).ConfigureAwait(false);
        if (row is null) return null;
        var kartu = new KartuRfid(
            row.CompanyCode, row.IdKartu, row.IdPengguna, row.UidKartu,
            row.Status, row.IsDeleted, row.CreatedBy, row.CreatedDate,
            row.LastUpdatedBy, row.LastUpdatedDate);
        return (kartu, row.IdPintu);
    }

    // Tulis tiap tap ke LOG_AKSES. idKartu = null untuk kartu asing.
    public async Task WriteLogAsync(
        string uid, int idPintu, string hasil, int? idKartu,
        string actor, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO dbo.LOG_AKSES
                (CompanyCode, id_kartu, id_pintu, uid_dipindai, hasil, CreatedBy)
            VALUES (@co, @kartu, @pintu, @uid, @hasil, @actor)
            """;
        await using var con = new SqlConnection(connectionString);
        await con.OpenAsync(ct).ConfigureAwait(false);
        var cmd = new CommandDefinition(sql,
            new { co = Company, kartu = idKartu, pintu = idPintu, uid, hasil, actor },
            cancellationToken: ct);
        await con.ExecuteAsync(cmd).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<LogAkses>> ReadLogAsync(
        int limit = 100, CancellationToken ct = default)
    {
        const string sql = """
            SELECT TOP (@n) CompanyCode, id_log AS IdLog, id_kartu AS IdKartu,
                   id_pintu AS IdPintu, uid_dipindai AS UidDipindai,
                   waktu AS Waktu, hasil AS Hasil, Status, IsDeleted, CreatedBy,
                   CreatedDate, LastUpdatedBy, LastUpdatedDate
            FROM dbo.LOG_AKSES
            WHERE CompanyCode = @co AND ISNULL(IsDeleted, 0) = 0
            ORDER BY waktu DESC
            """;
        await using var con = new SqlConnection(connectionString);
        await con.OpenAsync(ct).ConfigureAwait(false);
        var cmd = new CommandDefinition(sql,
            new { n = limit, co = Company },
            cancellationToken: ct);
        var rows = await con.QueryAsync<LogRow>(cmd).ConfigureAwait(false);
        var list = new List<LogAkses>();
        foreach (var r in rows)
            list.Add(new LogAkses(
                r.CompanyCode, r.IdLog, r.IdKartu, r.IdPintu, r.UidDipindai,
                r.Waktu, r.Hasil, r.Status, r.IsDeleted, r.CreatedBy,
                r.CreatedDate, r.LastUpdatedBy, r.LastUpdatedDate));
        return list;
    }

    // DTO internal untuk Dapper (nama properti = alias kolom SQL).
    private sealed class KartuRow
    {
        public string CompanyCode { get; set; } = "";
        public int IdKartu { get; set; }
        public int IdPengguna { get; set; }
        public string UidKartu { get; set; } = "";
        public byte? Status { get; set; }
        public byte? IsDeleted { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public int IdPintu { get; set; }
    }

    private sealed class LogRow
    {
        public string CompanyCode { get; set; } = "";
        public int IdLog { get; set; }
        public int? IdKartu { get; set; }
        public int IdPintu { get; set; }
        public string UidDipindai { get; set; } = "";
        public DateTime Waktu { get; set; }
        public string Hasil { get; set; } = "";
        public byte? Status { get; set; }
        public byte? IsDeleted { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
    }
}
