using Microsoft.Data.SqlClient;
using SmartDoor.Backend.Models;

namespace SmartDoor.Backend.Data;

// Akses SQL Server. CompanyCode = 'KEL4' (satu kelompok, satu database).
public sealed class SmartDoorDb(string connectionString)
{
    public const string Company = "KEL4";

    // Cari kartu aktif + punya hak akses ke pintu. Null = tolak.
    public async Task<(KartuRfid Kartu, int IdPintu)?> FindAksesAsync(
        string uid, int idPintu, CancellationToken ct = default)
    {
        const string sql = """
            SELECT k.CompanyCode, k.id_kartu, k.id_pengguna, k.uid_kartu,
                   k.Status, k.IsDeleted, k.CreatedBy, k.CreatedDate,
                   k.LastUpdatedBy, k.LastUpdatedDate, h.id_pintu
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
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@co", Company);
        cmd.Parameters.AddWithValue("@uid", uid);
        cmd.Parameters.AddWithValue("@pintu", idPintu);
        await using var r = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        if (!await r.ReadAsync(ct).ConfigureAwait(false)) return null;
        var kartu = new KartuRfid(
            r.GetString(0), r.GetInt32(1), r.GetInt32(2), r.GetString(3),
            r.IsDBNull(4) ? null : r.GetByte(4), r.IsDBNull(5) ? null : r.GetByte(5),
            r.IsDBNull(6) ? null : r.GetString(6), r.IsDBNull(7) ? null : r.GetDateTime(7),
            r.IsDBNull(8) ? null : r.GetString(8), r.IsDBNull(9) ? null : r.GetDateTime(9));
        return (kartu, r.GetInt32(10));
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
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@co", Company);
        cmd.Parameters.AddWithValue("@kartu", (object?)idKartu ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@pintu", idPintu);
        cmd.Parameters.AddWithValue("@uid", uid);
        cmd.Parameters.AddWithValue("@hasil", hasil);
        cmd.Parameters.AddWithValue("@actor", actor);
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<LogAkses>> ReadLogAsync(
        int limit = 100, CancellationToken ct = default)
    {
        const string sql = """
            SELECT TOP (@n) CompanyCode, id_log, id_kartu, id_pintu, uid_dipindai,
                   waktu, hasil, Status, IsDeleted, CreatedBy, CreatedDate,
                   LastUpdatedBy, LastUpdatedDate
            FROM dbo.LOG_AKSES
            WHERE CompanyCode = @co AND ISNULL(IsDeleted, 0) = 0
            ORDER BY waktu DESC
            """;
        var list = new List<LogAkses>();
        await using var con = new SqlConnection(connectionString);
        await con.OpenAsync(ct).ConfigureAwait(false);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@n", limit);
        cmd.Parameters.AddWithValue("@co", Company);
        await using var r = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        while (await r.ReadAsync(ct).ConfigureAwait(false))
            list.Add(new LogAkses(
                r.GetString(0), r.GetInt32(1),
                r.IsDBNull(2) ? null : r.GetInt32(2), r.GetInt32(3),
                r.GetString(4), r.GetDateTime(5), r.GetString(6),
                r.IsDBNull(7) ? null : r.GetByte(7), r.IsDBNull(8) ? null : r.GetByte(8),
                r.IsDBNull(9) ? null : r.GetString(9), r.IsDBNull(10) ? null : r.GetDateTime(10),
                r.IsDBNull(11) ? null : r.GetString(11), r.IsDBNull(12) ? null : r.GetDateTime(12)));
        return list;
    }
}
