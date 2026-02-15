using System.Configuration;
using System.Data;
using System.Windows;
using RestaurantManagement.Views;

namespace RestaurantManagement;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Show login window instead of the default MainWindow
        var loginView = new LoginView();
        loginView.Show();
    }
}