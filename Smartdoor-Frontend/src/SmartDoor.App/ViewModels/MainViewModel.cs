using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartDoor.Backend.Services;

namespace SmartDoor.App.ViewModels;

// ViewModel utama: status pintu + log tap terakhir. Digerakkan event SerialService.
// hasil DB = "diterima"/"ditolak"; ke Arduino dikirim "OPEN"/"DENY".
public sealed partial class MainViewModel : ObservableObject
{
    private readonly AuthService _auth;
    private readonly SerialService _serial;

    [ObservableProperty]
    private string _status = "Menunggu kartu...";

    public ObservableCollection<string> LogTerakhir { get; } = [];

    // DI: auth + serial dari container (lihat App.xaml.cs).
    public MainViewModel(AuthService auth, SerialService serial)
    {
        _auth = auth;
        _serial = serial;
        _serial.UidReceived += uid => _ = OnUidAsync(uid);
    }

    // Dipanggil tiap UID masuk dari Arduino.
    [RelayCommand]
    private async Task OnUidAsync(string uid)
    {
        var hasil = await _auth.CheckAsync(uid).ConfigureAwait(true);
        _serial.Reply(hasil == "diterima" ? "OPEN" : "DENY");
        Status = hasil == "diterima" ? $"Terbuka — {uid}" : $"Ditolak — {uid}";
        LogTerakhir.Insert(0, $"{DateTime.Now:HH:mm:ss} {uid} -> {hasil}");
    }
}
