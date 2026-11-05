using System.Windows;
using Cicalany.Wpf.ViewModels;

namespace Cicalany.Wpf;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new ShellViewModel();
    }
}
