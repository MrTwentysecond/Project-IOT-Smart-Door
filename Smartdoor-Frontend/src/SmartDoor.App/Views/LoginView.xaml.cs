using System.Windows;
using System.Windows.Controls;
using SmartDoor.App.ViewModels;

namespace SmartDoor.App.Views;

// Form login admin. Password diambil dari PasswordBox (tidak bisa binding).
public partial class LoginView : UserControl
{
    public LoginView() => InitializeComponent();

    private async void OnLogin(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            await vm.LoginAsync(vm.Username, PasswordBox.Password);
    }
}
