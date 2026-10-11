using SmartDoor.Backend.Models;

namespace SmartDoor.Backend.Data;

// ponytail: ceiling = in-memory stub; upgrade path = tambah Microsoft.Data.Sqlite + tabel KARTU/LOG_AKSES.
public sealed class SmartDoorDb
{
    public Task<Kartu?> FindKartuAsync(string uid, CancellationToken ct = default)
        => Task.FromResult<Kartu?>(null); // TODO: query SQLite

    public Task WriteLogAsync(string uid, string hasil, CancellationToken ct = default)
        => Task.CompletedTask; // TODO: insert LOG_AKSES

    public Task<IReadOnlyList<LogAkses>> ReadLogAsync(int limit = 100, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<LogAkses>>([]); // TODO: select LOG_AKSES
}
