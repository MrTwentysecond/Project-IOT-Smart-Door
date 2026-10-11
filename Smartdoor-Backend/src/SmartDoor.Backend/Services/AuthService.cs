using SmartDoor.Backend.Data;

namespace SmartDoor.Backend.Services;

// Cocokkan UID -> OPEN/DENY + tulis log.
public sealed class AuthService(SmartDoorDb db)
{
    public async Task<string> CheckAsync(string uid, CancellationToken ct = default)
    {
        var kartu = await db.FindKartuAsync(uid, ct).ConfigureAwait(false);
        var hasil = kartu is { Aktif: true } ? "OPEN" : "DENY";
        await db.WriteLogAsync(uid, hasil, ct).ConfigureAwait(false);
        return hasil; // dikirim balik ke Arduino lewat serial
    }
}
