using System.Windows;
using System.Windows.Controls;
using Cicalany.Wpf.ViewModels;

namespace Cicalany.Wpf.Views;

public partial class AuthView : UserControl
{
    public AuthView() => InitializeComponent();

    private void PasswordInput_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is AuthViewModel vm)
            vm.Password = PasswordInput.Password;
    }
}
