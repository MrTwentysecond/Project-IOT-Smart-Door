using System.Security.Cryptography;
using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using SmartDoor.Backend.Models;

namespace SmartDoor.Backend.Data;

// Akses SQL Server via Dapper. CompanyCode = 'KEL4' (satu kelompok, satu database).
public sealed class SmartDoorDb(string connectionString)
{
    public const string Company = "KEL4";

    // Cari kartu aktif + punya hak akses ke pintu, beserta info expired/kuota/servis.
    // Null = kartu asing atau tanpa hak akses ke pintu ini.
    public async Task<AksesInfo?> FindAksesAsync(
        string uid, int idPintu, CancellationToken ct = default)
    {
        const string sql = """
            SELECT k.CompanyCode, k.id_kartu AS IdKartu, k.id_pengguna AS IdPengguna,
                   k.uid_kartu AS UidKartu, k.berlaku_sampai AS BerlakuSampai,
                   k.maks_pakai AS MaksPakai, k.total_pakai AS TotalPakai,
                   k.Status, k.IsDeleted, k.CreatedBy,
                   k.CreatedDate, k.LastUpdatedBy, k.LastUpdatedDate,
                   h.id_pintu AS IdPintu,
                   p.nama_pintu AS NamaPintu, p.total_buka AS TotalBuka,
                   p.batas_servis AS BatasServis,
                   p.servis_tiap_hari AS ServisTiapHari,
                   p.servis_terakhir AS ServisTerakhir
            FROM dbo.KARTU_RFID k
            JOIN dbo.HAK_AKSES h
              ON h.CompanyCode = k.CompanyCode AND h.id_kartu = k.id_kartu
            JOIN dbo.PINTU p
              ON p.CompanyCode = k.CompanyCode AND p.id_pintu = h.id_pintu
            WHERE k.CompanyCode = @co AND k.uid_kartu = @uid
              AND h.id_pintu = @pintu
              AND ISNULL(k.Status, 1) = 1 AND ISNULL(k.IsDeleted, 0) = 0
              AND ISNULL(h.Status, 1) = 1 AND ISNULL(h.IsDeleted, 0) = 0
              AND ISNULL(p.Status, 1) = 1 AND ISNULL(p.IsDeleted, 0) = 0
            """;
        await using var con = new SqlConnection(connectionString);
        await con.OpenAsync(ct).ConfigureAwait(false);
        var cmd = new CommandDefinition(sql,
            new { co = Company, uid, pintu = idPintu },
            cancellationToken: ct);
        var row = await con.QueryFirstOrDefaultAsync<AksesRow>(cmd).ConfigureAwait(false);
        if (row is null) return null;
        var kartu = new KartuRfid(
            row.CompanyCode, row.IdKartu, row.IdPengguna, row.UidKartu,
            row.BerlakuSampai, row.MaksPakai, row.TotalPakai,
            row.Status, row.IsDeleted, row.CreatedBy, row.CreatedDate,
            row.LastUpdatedBy, row.LastUpdatedDate);
        return new AksesInfo(kartu, row.IdPintu, row.NamaPintu,
            row.TotalBuka, row.BatasServis, row.ServisTiapHari, row.ServisTerakhir);
    }

    // Tulis tiap tap ke LOG_AKSES. idKartu = null untuk kartu asing.
    // alasan: "tanpa_akses"/"kartu_asing"/"expired"/"kuota_habis", atau
    // "perlu_servis"/null pada tap diterima.
    public async Task WriteLogAsync(
        string uid, int idPintu, string hasil, int? idKartu,
        string actor, string? alasan = null, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO dbo.LOG_AKSES
                (CompanyCode, id_kartu, id_pintu, uid_dipindai, hasil, alasan, CreatedBy)
            VALUES (@co, @kartu, @pintu, @uid, @hasil, @alasan, @actor)
            """;
        await using var con = new SqlConnection(connectionString);
        await con.OpenAsync(ct).ConfigureAwait(false);
        var cmd = new CommandDefinition(sql,
            new { co = Company, kartu = idKartu, pintu = idPintu, uid, hasil, alasan, actor },
            cancellationToken: ct);
        await con.ExecuteAsync(cmd).ConfigureAwait(false);
    }

    // Counter kartu naik tiap tap DITERIMA (dipakai kuota).
    public async Task TambahPakaiKartuAsync(int idKartu, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE dbo.KARTU_RFID
            SET total_pakai = total_pakai + 1, LastUpdatedDate = SYSDATETIME()
            WHERE CompanyCode = @co AND id_kartu = @kartu
            """;
        await using var con = new SqlConnection(connectionString);
        await con.OpenAsync(ct).ConfigureAwait(false);
        await con.ExecuteAsync(
            new CommandDefinition(sql, new { co = Company, kartu = idKartu },
            cancellationToken: ct)).ConfigureAwait(false);
    }

    // Counter pintu naik tiap tap DITERIMA (pintu dibuka).
    public async Task TambahBukaPintuAsync(int idPintu, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE dbo.PINTU
            SET total_buka = total_buka + 1, LastUpdatedDate = SYSDATETIME()
            WHERE CompanyCode = @co AND id_pintu = @pintu
            """;
        await using var con = new SqlConnection(connectionString);
        await con.OpenAsync(ct).ConfigureAwait(false);
        await con.ExecuteAsync(
            new CommandDefinition(sql, new { co = Company, pintu = idPintu },
            cancellationToken: ct)).ConfigureAwait(false);
    }

    // Tandai pintu sudah diservis (reset jadwal hari; counter buka jalan terus).
    public async Task TandaiServisAsync(int idPintu, string actor, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE dbo.PINTU
            SET servis_terakhir = SYSDATETIME(),
                LastUpdatedBy = @actor, LastUpdatedDate = SYSDATETIME()
            WHERE CompanyCode = @co AND id_pintu = @pintu
            """;
        await using var con = new SqlConnection(connectionString);
        await con.OpenAsync(ct).ConfigureAwait(false);
        await con.ExecuteAsync(
            new CommandDefinition(sql, new { co = Company, pintu = idPintu, actor },
            cancellationToken: ct)).ConfigureAwait(false);
    }

    // Status servis semua pintu: butuh servis jika counter >= batas
    // ATAU sudah lewat N hari sejak servis terakhir (untuk pintu jarang dipakai).
    public async Task<IReadOnlyList<StatusServis>> ReadStatusServisAsync(CancellationToken ct = default)
    {
        const string sql = """
            SELECT CompanyCode, id_pintu AS IdPintu, nama_pintu AS NamaPintu,
                   total_buka AS TotalBuka, batas_servis AS BatasServis,
                   servis_tiap_hari AS ServisTiapHari, servis_terakhir AS ServisTerakhir
            FROM dbo.PINTU
            WHERE CompanyCode = @co AND ISNULL(IsDeleted, 0) = 0
            ORDER BY nama_pintu
            """;
        await using var con = new SqlConnection(connectionString);
        await con.OpenAsync(ct).ConfigureAwait(false);
        var rows = await con.QueryAsync<PintuRow>(
            new CommandDefinition(sql, new { co = Company },
            cancellationToken: ct)).ConfigureAwait(false);
        var now = DateTime.Now;
        var list = new List<StatusServis>();
        foreach (var r in rows)
        {
            var lewatPakai = r.BatasServis is { } b && b >= 0 && r.TotalBuka >= b;
            var lewatHari = r.ServisTiapHari is { } h && h >= 0
                && (r.ServisTerakhir is null || (now - r.ServisTerakhir.Value).TotalDays >= h);
            list.Add(new StatusServis(r.CompanyCode, r.IdPintu, r.NamaPintu,
                r.TotalBuka, r.BatasServis, r.ServisTiapHari, r.ServisTerakhir,
                lewatPakai || lewatHari, lewatPakai, lewatHari));
        }
        return list;
    }

    public async Task<IReadOnlyList<LogAkses>> ReadLogAsync(
        int limit = 100, CancellationToken ct = default)
    {
        const string sql = """
            SELECT TOP (@n) CompanyCode, id_log AS IdLog, id_kartu AS IdKartu,
                   id_pintu AS IdPintu, uid_dipindai AS UidDipindai,
                   waktu AS Waktu, hasil AS Hasil, alasan AS Alasan,
                   Status, IsDeleted, CreatedBy,
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
                r.Waktu, r.Hasil, r.Alasan, r.Status, r.IsDeleted, r.CreatedBy,
                r.CreatedDate, r.LastUpdatedBy, r.LastUpdatedDate));
        return list;
    }

    // Daftar pengguna (admin): nama, NIM/NIP, gender, umur, kontak.
    public async Task<IReadOnlyList<Pengguna>> ReadPenggunaAsync(CancellationToken ct = default)
    {
        const string sql = """
            SELECT CompanyCode, id_pengguna AS IdPengguna, id_admin AS IdAdmin,
                   nama AS Nama, nim_nip AS NimNip, jenis_kelamin AS JenisKelamin,
                   umur AS Umur, kontak AS Kontak, Status, IsDeleted, CreatedBy, CreatedDate,
                   LastUpdatedBy, LastUpdatedDate
            FROM dbo.PENGGUNA
            WHERE CompanyCode = @co AND ISNULL(IsDeleted, 0) = 0
            ORDER BY nama
            """;
        await using var con = new SqlConnection(connectionString);
        await con.OpenAsync(ct).ConfigureAwait(false);
        var cmd = new CommandDefinition(sql, new { co = Company }, cancellationToken: ct);
        var rows = await con.QueryAsync<PenggunaRow>(cmd).ConfigureAwait(false);
        var list = new List<Pengguna>();
        foreach (var r in rows)
            list.Add(new Pengguna(
                r.CompanyCode, r.IdPengguna, r.IdAdmin, r.Nama, r.NimNip,
                r.JenisKelamin, r.Umur, r.Kontak, r.Status, r.IsDeleted, r.CreatedBy,
                r.CreatedDate, r.LastUpdatedBy, r.LastUpdatedDate));
        return list;
    }

    // Daftarkan pengguna + kartu RFID dalam satu transaksi.
    // Wajib: nama, jenis_kelamin ('L'/'P'), umur (>= 0), kontak, uid_kartu.
    // Opsional: nim_nip, berlaku_sampai, maks_pakai, id_pintu[] (hak akses pintu).
    // Berhasil -> (true, "Terdaftar.", idPengguna). Gagal -> (false, pesan, null).
    public async Task<DaftarHasil> DaftarkanPenggunaAsync(
        DaftarPengguna input, string actor, CancellationToken ct = default)
    {
        var err = input.CekWajib();
        if (err is not null) return new DaftarHasil(false, err, null);
        const string cekUid = """
            SELECT COUNT(1) FROM dbo.KARTU_RFID
            WHERE CompanyCode = @co AND uid_kartu = @uid
            """;
        const string nextId = """
            SELECT ISNULL(MAX(id_pengguna), 0) + 1 FROM dbo.PENGGUNA WHERE CompanyCode = @co
            """;
        const string nextKartu = """
            SELECT ISNULL(MAX(id_kartu), 0) + 1 FROM dbo.KARTU_RFID WHERE CompanyCode = @co
            """;
        const string nextAkses = """
            SELECT ISNULL(MAX(id_akses), 0) FROM dbo.HAK_AKSES WHERE CompanyCode = @co
            """;
        const string insPengguna = """
            INSERT INTO dbo.PENGGUNA
                (CompanyCode, id_pengguna, id_admin, nama, nim_nip, jenis_kelamin, umur, kontak, CreatedBy)
            VALUES (@co, @id, @admin, @nama, @nim, @gender, @umur, @kontak, @actor)
            """;
        const string insKartu = """
            INSERT INTO dbo.KARTU_RFID
                (CompanyCode, id_kartu, id_pengguna, uid_kartu, berlaku_sampai, maks_pakai, CreatedBy)
            VALUES (@co, @kartu, @id, @uid, @laku, @maks, @actor)
            """;
        const string insHak = """
            INSERT INTO dbo.HAK_AKSES (CompanyCode, id_akses, id_kartu, id_pintu, CreatedBy)
            VALUES (@co, @akses, @kartu, @pintu, @actor)
            """;
        await using var con = new SqlConnection(connectionString);
        await con.OpenAsync(ct).ConfigureAwait(false);
        await using var tx =
            await con.BeginTransactionAsync(ct).ConfigureAwait(false);
        try
        {
            var duplikat = await con.QueryFirstAsync<int>(
                new CommandDefinition(cekUid, new { co = Company, uid = input.UidKartu },
                transaction: tx,
                cancellationToken: ct)).ConfigureAwait(false);
            if (duplikat > 0) return new DaftarHasil(false, "UID kartu sudah terdaftar.", null);
            var id = await con.QueryFirstAsync<int>(
                new CommandDefinition(nextId, new { co = Company },
                transaction: tx,
                cancellationToken: ct)).ConfigureAwait(false);
            var idKartu = await con.QueryFirstAsync<int>(
                new CommandDefinition(nextKartu, new { co = Company },
                transaction: tx,
                cancellationToken: ct)).ConfigureAwait(false);
            await con.ExecuteAsync(new CommandDefinition(insPengguna,
                new
                {
                    co = Company, id, admin = input.IdAdmin, nama = input.Nama!.Trim(),
                    nim = string.IsNullOrWhiteSpace(input.NimNip) ? null : input.NimNip.Trim(),
                    gender = input.JenisKelamin, umur = input.Umur,
                    kontak = input.Kontak!.Trim(), actor
                },
                transaction: tx,
                cancellationToken: ct)).ConfigureAwait(false);
            await con.ExecuteAsync(new CommandDefinition(insKartu,
                new
                {
                    co = Company, kartu = idKartu, id, uid = input.UidKartu.Trim(),
                    laku = input.BerlakuSampai, maks = input.MaksPakai, actor
                },
                transaction: tx,
                cancellationToken: ct)).ConfigureAwait(false);
            var noAkses = await con.QueryFirstAsync<int>(
                new CommandDefinition(nextAkses, new { co = Company },
                transaction: tx,
                cancellationToken: ct)).ConfigureAwait(false);
            foreach (var pintu in input.IdPintu.Distinct())
            {
                noAkses++;
                await con.ExecuteAsync(new CommandDefinition(insHak,
                    new { co = Company, akses = noAkses, kartu = idKartu, pintu, actor },
                    transaction: tx,
                    cancellationToken: ct)).ConfigureAwait(false);
            }
            await tx.CommitAsync(ct).ConfigureAwait(false);
            return new DaftarHasil(true, "Terdaftar.", id);
        }
        catch
        {
            await tx.RollbackAsync(ct).ConfigureAwait(false);
            throw;
        }
    }

    // Daftar kartu + info expired/kuota untuk layar admin.
    public async Task<IReadOnlyList<KartuRfid>> ReadKartuAsync(CancellationToken ct = default)
    {
        const string sql = """
            SELECT CompanyCode, id_kartu AS IdKartu, id_pengguna AS IdPengguna,
                   uid_kartu AS UidKartu, berlaku_sampai AS BerlakuSampai,
                   maks_pakai AS MaksPakai, total_pakai AS TotalPakai,
                   Status, IsDeleted, CreatedBy, CreatedDate,
                   LastUpdatedBy, LastUpdatedDate
            FROM dbo.KARTU_RFID
            WHERE CompanyCode = @co AND ISNULL(IsDeleted, 0) = 0
            ORDER BY uid_kartu
            """;
        await using var con = new SqlConnection(connectionString);
        await con.OpenAsync(ct).ConfigureAwait(false);
        var rows = await con.QueryAsync<KartuRow>(
            new CommandDefinition(sql, new { co = Company },
            cancellationToken: ct)).ConfigureAwait(false);
        var list = new List<KartuRfid>();
        foreach (var r in rows)
            list.Add(new KartuRfid(
                r.CompanyCode, r.IdKartu, r.IdPengguna, r.UidKartu,
                r.BerlakuSampai, r.MaksPakai, r.TotalPakai,
                r.Status, r.IsDeleted, r.CreatedBy, r.CreatedDate,
                r.LastUpdatedBy, r.LastUpdatedDate));
        return list;
    }

    // Login admin: cocokkan hash SHA-256 password. Null = gagal.
    public async Task<Admin?> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        const string sql = """
            SELECT CompanyCode, id_admin AS IdAdmin, username AS Username,
                   password_hash AS PasswordHash, Status, IsDeleted, CreatedBy,
                   CreatedDate, LastUpdatedBy, LastUpdatedDate
            FROM dbo.ADMIN
            WHERE CompanyCode = @co AND username = @user
              AND ISNULL(Status, 1) = 1 AND ISNULL(IsDeleted, 0) = 0
            """;
        await using var con = new SqlConnection(connectionString);
        await con.OpenAsync(ct).ConfigureAwait(false);
        var cmd = new CommandDefinition(sql,
            new { co = Company, user = username },
            cancellationToken: ct);
        var row = await con.QueryFirstOrDefaultAsync<AdminRow>(cmd).ConfigureAwait(false);
        if (row is null) return null;
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password))).ToLowerInvariant();
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(hash),
                Encoding.UTF8.GetBytes(row.PasswordHash.ToLowerInvariant())))
            return null;
        return new Admin(
            row.CompanyCode, row.IdAdmin, row.Username, row.PasswordHash,
            row.Status, row.IsDeleted, row.CreatedBy, row.CreatedDate,
            row.LastUpdatedBy, row.LastUpdatedDate);
    }

    // Hasil gabungan kartu + pintu untuk satu tap.
    public sealed record AksesInfo(
        KartuRfid Kartu, int IdPintu, string NamaPintu,
        int TotalBuka, int? BatasServis, int? ServisTiapHari, DateTime? ServisTerakhir)
    {
        // Kartu lewat tanggal berlaku?
        public bool Expired =>
            Kartu.BerlakuSampai is { } t && DateTime.Now > t;

        // Kuota tap habis?
        public bool KuotaHabis =>
            Kartu.MaksPakai is { } m && m >= 0 && Kartu.TotalPakai >= m;

        // Pintu butuh servis berkala? (counter ATAU sudah N hari tak diservis)
        public bool PerluServis =>
            (BatasServis is { } b && b >= 0 && TotalBuka >= b)
            || (ServisTiapHari is { } h && h >= 0
                && (ServisTerakhir is null
                    || (DateTime.Now - ServisTerakhir.Value).TotalDays >= h));
    }

    // Satu baris status servis pintu (untuk layar admin).
    public sealed record StatusServis(
        string CompanyCode, int IdPintu, string NamaPintu,
        int TotalBuka, int? BatasServis, int? ServisTiapHari, DateTime? ServisTerakhir,
        bool PerluServis, bool LewatPakai, bool LewatHari);

    // DTO internal untuk Dapper (nama properti = alias kolom SQL).
    private sealed class AksesRow
    {
        public string CompanyCode { get; set; } = "";
        public int IdKartu { get; set; }
        public int IdPengguna { get; set; }
        public string UidKartu { get; set; } = "";
        public DateTime? BerlakuSampai { get; set; }
        public int? MaksPakai { get; set; }
        public int TotalPakai { get; set; }
        public byte? Status { get; set; }
        public byte? IsDeleted { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public int IdPintu { get; set; }
        public string NamaPintu { get; set; } = "";
        public int TotalBuka { get; set; }
        public int? BatasServis { get; set; }
        public int? ServisTiapHari { get; set; }
        public DateTime? ServisTerakhir { get; set; }
    }

    private sealed class PintuRow
    {
        public string CompanyCode { get; set; } = "";
        public int IdPintu { get; set; }
        public string NamaPintu { get; set; } = "";
        public int TotalBuka { get; set; }
        public int? BatasServis { get; set; }
        public int? ServisTiapHari { get; set; }
        public DateTime? ServisTerakhir { get; set; }
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
        public string? Alasan { get; set; }
        public byte? Status { get; set; }
        public byte? IsDeleted { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
    }

    private sealed class PenggunaRow
    {
        public string CompanyCode { get; set; } = "";
        public int IdPengguna { get; set; }
        public int IdAdmin { get; set; }
        public string Nama { get; set; } = "";
        public string? NimNip { get; set; }
        public string? JenisKelamin { get; set; }
        public int? Umur { get; set; }
        public string? Kontak { get; set; }
        public byte? Status { get; set; }
        public byte? IsDeleted { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
    }

    private sealed class KartuRow
    {
        public string CompanyCode { get; set; } = "";
        public int IdKartu { get; set; }
        public int IdPengguna { get; set; }
        public string UidKartu { get; set; } = "";
        public DateTime? BerlakuSampai { get; set; }
        public int? MaksPakai { get; set; }
        public int TotalPakai { get; set; }
        public byte? Status { get; set; }
        public byte? IsDeleted { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
    }

    private sealed class AdminRow
    {
        public string CompanyCode { get; set; } = "";
        public int IdAdmin { get; set; }
        public string Username { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public byte? Status { get; set; }
        public byte? IsDeleted { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
    }
}
