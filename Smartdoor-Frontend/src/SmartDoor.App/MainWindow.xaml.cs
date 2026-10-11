using System.Windows;
using SmartDoor.App.ViewModels;
using SmartDoor.Backend.Data;
using SmartDoor.Backend.Services;

namespace SmartDoor.App;

// Wiring: serial -> viewmodel -> auth -> db. Tanpa DI framework (cukup untuk 1 window).
public partial class MainWindow : Window
{
    private readonly SerialService _serial = new();

    public MainWindow()
    {
        InitializeComponent();
        var vm = new MainViewModel();
        DataContext = vm;
        // Falzen: ganti connection string sesuai SQL Server (localdb/SQLEXPRESS).
        const string cs = "Server=localhost\\SQLEXPRESS;Database=SmartDoor;Trusted_Connection=True;TrustServerCertificate=True";
        var db = new SmartDoorDb(cs);
        var auth = new AuthService(db);
        _serial.UidReceived += uid => _ = vm.OnUidAsync(uid, auth, _serial);
        // TODO: _serial.Start("COM3");
    }
}