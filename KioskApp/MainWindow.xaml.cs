using System.Windows;

namespace KioskApp;

using KioskApp.ViewModels;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        KioskApp.Services.DbInitializer.Initialize();
        DataContext = new MainWindowViewModel();
    }
}