using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartDoor.App.Themes;
using SmartDoor.Backend.Data;
using SmartDoor.Backend.Models;
using SmartDoor.Backend.Services;

namespace SmartDoor.App.ViewModels;

// ViewModel utama. Layar: "User" (status+tap), "Login" (form admin),
// "Admin" (tabel pengguna + log akses). Tema: Terang/Gelap/Custom.
// hasil DB = "diterima"/"ditolak"; ke Arduino dikirim "OPEN"/"DENY".
public sealed partial class MainViewModel : ObservableObject
{
    private readonly SmartDoorDb _db;
    private readonly AuthService _auth;
    private readonly SerialService _serial;

    [ObservableProperty]
    private string _status = "Menunggu kartu...";

    [ObservableProperty]
    private string _screen = "User";

    [ObservableProperty]
    private string _username = "";

    [ObservableProperty]
    private string _loginError = "";

    [ObservableProperty]
    private string _theme = "Terang";

    public ObservableCollection<string> LogTerakhir { get; } = [];
    public ObservableCollection<Pengguna> Users { get; } = [];
    public ObservableCollection<LogAkses> Logs { get; } = [];

    // DI: db/auth/serial dari container (lihat App.xaml.cs).
    public MainViewModel(SmartDoorDb db, AuthService auth, SerialService serial)
    {
        _db = db;
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

    // --- Navigasi layar ---

    [RelayCommand]
    private void ShowUser()
    {
        Screen = "User";
        LoginError = "";
    }

    [RelayCommand]
    private void ShowAdmin()
    {
        Username = "";
        LoginError = "";
        Screen = "Login";
    }

    [RelayCommand]
    private async Task Logout()
    {
        Screen = "User";
        LoginError = "";
        Users.Clear();
        Logs.Clear();
    }

    // --- Login admin ---

    public async Task LoginAsync(string username, string password)
    {
        LoginError = "";
        var admin = await _db.LoginAsync(username, password).ConfigureAwait(true);
        if (admin is null)
        {
            LoginError = "Username atau password salah.";
            return;
        }
        await LoadAdminDataAsync();
        Screen = "Admin";
    }

    // --- Data admin ---

    [RelayCommand]
    private async Task LoadAdminDataAsync()
    {
        Users.Clear();
        foreach (var u in await _db.ReadPenggunaAsync().ConfigureAwait(true))
            Users.Add(u);
        Logs.Clear();
        foreach (var l in await _db.ReadLogAsync(100).ConfigureAwait(true))
            Logs.Add(l);
    }

    // --- Tema ---

    [RelayCommand]
    private void SetTheme(string name)
    {
        Theme = name;
        ThemeHelper.Apply(name);
    }
}
