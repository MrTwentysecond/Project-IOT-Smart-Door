namespace SmartDoor.Backend.Models;

// Input form pendaftaran user + kartu.
// Wajib (*): Nama, JenisKelamin ('L'/'P'), Umur (>= 0), Kontak, UidKartu.
// Opsional: NimNip, BerlakuSampai, MaksPakai, IdPintu (hak akses pintu).
public sealed record DaftarPengguna(
    int IdAdmin,
    string? Nama,
    string? NimNip,
    string? JenisKelamin,
    int? Umur,
    string? Kontak,
    string UidKartu,
    DateTime? BerlakuSampai,
    int? MaksPakai,
    int[] IdPintu)
{
    // Null = valid; string = pesan error field wajib yang kosong/salah.
    public string? CekWajib()
    {
        if (string.IsNullOrWhiteSpace(Nama)) return "Nama wajib diisi.";
        if (JenisKelamin is not ("L" or "P")) return "Jenis kelamin wajib dipilih (L/P).";
        if (Umur is null or < 0) return "Umur wajib diisi (>= 0).";
        if (string.IsNullOrWhiteSpace(Kontak)) return "Kontak wajib diisi.";
        if (string.IsNullOrWhiteSpace(UidKartu)) return "UID kartu wajib diisi.";
        if (MaksPakai is < 0) return "Maks pakai tidak boleh negatif.";
        if (BerlakuSampai is { } t && t <= DateTime.Now) return "Tanggal berlaku sudah lewat.";
        return null;
    }
}

// Hasil pendaftaran: Berhasil + Pesan + IdPengguna baru (null bila gagal).
public sealed record DaftarHasil(bool Berhasil, string Pesan, int? IdPengguna);
