using SmartDoor.Backend.Data;

namespace SmartDoor.Backend.Services;

// Cocokkan UID + hak akses pintu -> "diterima"/"ditolak", tulis log.
// Kartu expired / kuota habis -> "ditolak" (alasan di LOG_AKSES).
// Pintu perlu servis -> tetap "diterima" + alasan "perlu_servis" (info admin).
// Arduino menerjemahkan ke OPEN/DENY (LED, buzzer, solenoid).
public sealed class AuthService(SmartDoorDb db, int idPintu = 1, string actor = "app")
{
    public async Task<string> CheckAsync(string uid, CancellationToken ct = default)
    {
        var akses = await db.FindAksesAsync(uid, idPintu, ct).ConfigureAwait(false);
        if (akses is null)
        {
            await db.WriteLogAsync(uid, idPintu, "ditolak", null, actor, "kartu_asing", ct)
                .ConfigureAwait(false);
            return "ditolak";
        }
        if (akses.Expired)
        {
            await db.WriteLogAsync(uid, idPintu, "ditolak", akses.Kartu.IdKartu, actor, "expired", ct)
                .ConfigureAwait(false);
            return "ditolak";
        }
        if (akses.KuotaHabis)
        {
            await db.WriteLogAsync(uid, idPintu, "ditolak", akses.Kartu.IdKartu, actor, "kuota_habis", ct)
                .ConfigureAwait(false);
            return "ditolak";
        }
        var alasan = akses.PerluServis ? "perlu_servis" : null;
        await db.WriteLogAsync(uid, idPintu, "diterima", akses.Kartu.IdKartu, actor, alasan, ct)
            .ConfigureAwait(false);
        await db.TambahPakaiKartuAsync(akses.Kartu.IdKartu, ct).ConfigureAwait(false);
        await db.TambahBukaPintuAsync(idPintu, ct).ConfigureAwait(false);
        return "diterima";
    }
}
