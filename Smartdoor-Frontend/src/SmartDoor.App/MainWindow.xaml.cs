using System.Windows;
using SmartDoor.App.ViewModels;
using SmartDoor.Backend.Services;

namespace SmartDoor.App;

// Window shell: DataContext dari DI (lihat App.xaml.cs).
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel vm, SerialService serial)
    {
        InitializeComponent();
        DataContext = vm;
        // TODO: serial.Start("COM3");
        _ = serial;
    }
}
