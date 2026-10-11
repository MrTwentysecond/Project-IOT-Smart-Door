using SmartDoor.Backend.Data;

namespace SmartDoor.Backend.Services;

// Cocokkan UID + hak akses pintu -> "diterima"/"ditolak", tulis log.
// Arduino menerjemahkan ke OPEN/DENY (LED, buzzer, solenoid).
public sealed class AuthService(SmartDoorDb db, int idPintu = 1, string actor = "app")
{
    public async Task<string> CheckAsync(string uid, CancellationToken ct = default)
    {
        var akses = await db.FindAksesAsync(uid, idPintu, ct).ConfigureAwait(false);
        var hasil = akses is null ? "ditolak" : "diterima";
        await db.WriteLogAsync(uid, idPintu, hasil, akses?.Kartu.IdKartu, actor, ct).ConfigureAwait(false);
        return hasil;
    }
}
