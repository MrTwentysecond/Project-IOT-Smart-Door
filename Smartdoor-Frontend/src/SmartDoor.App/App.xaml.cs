using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SmartDoor.App.ViewModels;
using SmartDoor.Backend.Data;
using SmartDoor.Backend.Services;

namespace SmartDoor.App;

/// <summary>
/// Interaction logic for App.xaml — DI container: db -> auth -> serial -> viewmodel.
/// </summary>
public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Falzen: LocalDB jalan di laptop ini. Kalau nanti pakai SQLEXPRESS, ganti ke Server=localhost\SQLEXPRESS.
        const string cs = @"Server=(localdb)\MSSQLLocalDB;Database=SmartDoor;Trusted_Connection=True;TrustServerCertificate=True";

        Services = new ServiceCollection()
            .AddSingleton(new SmartDoorDb(cs))
            .AddSingleton(sp => new AuthService(sp.GetRequiredService<SmartDoorDb>()))
            .AddSingleton<SerialService>()
            .AddSingleton<MainViewModel>()
            .AddSingleton<MainWindow>()
            .BuildServiceProvider();

        Services.GetRequiredService<MainWindow>().Show();
    }
}
