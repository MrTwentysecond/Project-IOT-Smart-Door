using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SmartDoor.Backend.Services;

namespace SmartDoor.App.ViewModels;

// ViewModel utama: status pintu + log tap terakhir. Digerakkan event SerialService.
public sealed class MainViewModel : INotifyPropertyChanged
{
    private string _status = "Menunggu kartu...";

    public string Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); }
    }

    public ObservableCollection<string> LogTerakhir { get; } = [];

    public event PropertyChangedEventHandler? PropertyChanged;

    // Dipanggil tiap UID masuk dari Arduino. auth + serial di-wire di MainWindow.
    // hasil DB = "diterima"/"ditolak"; ke Arduino dikirim "OPEN"/"DENY".
    public async Task OnUidAsync(string uid, AuthService auth, SerialService serial)
    {
        var hasil = await auth.CheckAsync(uid).ConfigureAwait(true);
        serial.Reply(hasil == "diterima" ? "OPEN" : "DENY");
        Status = hasil == "diterima" ? $"Terbuka — {uid}" : $"Ditolak — {uid}";
        LogTerakhir.Insert(0, $"{DateTime.Now:HH:mm:ss} {uid} -> {hasil}");
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
