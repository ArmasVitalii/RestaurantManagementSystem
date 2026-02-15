using System.Windows;
using RestaurantManagement.ViewModels;

namespace RestaurantManagement.Views;

public partial class LoginView : Window
{
    private readonly LoginViewModel _viewModel;
    
    public LoginView()
    {
        InitializeComponent();

        _viewModel = new LoginViewModel(NavigateToMainWindow);
        DataContext = _viewModel;

        // Wire up password changed events
        PasswordBox.PasswordChanged += (s, e) => _viewModel.Password = PasswordBox.Password;
        RegisterPasswordBox.PasswordChanged += (s, e) => _viewModel.Password = RegisterPasswordBox.Password;
        ConfirmPasswordBox.PasswordChanged += (s, e) => _viewModel.ConfirmPassword = ConfirmPasswordBox.Password;
    }

    private void NavigateToMainWindow()
    {
        var mainWindow = new MainWindow();
        mainWindow.Show();
        Close();
    }
} 