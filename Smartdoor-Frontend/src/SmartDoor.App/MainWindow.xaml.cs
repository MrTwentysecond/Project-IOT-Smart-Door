using System.ComponentModel;
using System.Windows;
using SmartDoor.App.ViewModels;
using SmartDoor.App.Views;
using SmartDoor.Backend.Services;

namespace SmartDoor.App;

// Window shell: DataContext dari DI (lihat App.xaml.cs).
// Ganti konten (User/Login/Admin) mengikuti MainViewModel.Screen.
public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;
    private readonly UserView _user = new();
    private readonly LoginView _login = new();
    private readonly AdminView _admin = new();

    public MainWindow(MainViewModel vm, SerialService serial)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = vm;
        _vm.PropertyChanged += OnVmChanged;
        ShowScreen(_vm.Screen);
        // TODO: serial.Start("COM3");
        _ = serial;
    }

    private void OnVmChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.Screen))
            Dispatcher.Invoke(() => ShowScreen(_vm.Screen));
    }

    private void ShowScreen(string screen)
    {
        ScreenHost.Content = screen switch
        {
            "Login" => _login,
            "Admin" => _admin,
            _ => _user,
        };
    }
}
