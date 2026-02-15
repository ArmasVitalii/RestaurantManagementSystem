using System.Windows;
using System.Windows.Input;
using RestaurantManagement.Commands;
using RestaurantManagement.Models;
using RestaurantManagement.Services;

namespace RestaurantManagement.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly AuthService _authService;
    private readonly Action _navigateToMainWindow;

    private string _email = "";
    private string _password = "";
    private string _firstName = "";
    private string _lastName = "";
    private string _phone = "";
    private string _address = "";
    private string _confirmPassword = "";
    private string _errorMessage = "";
    private bool _isRegistering = false;

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }
    
    public string FirstName
    {
        get => _firstName;
        set => SetProperty(ref _firstName, value);
    }
    
    public string LastName
    {
        get => _lastName;
        set => SetProperty(ref _lastName, value);
    }
    
    public string Phone
    {
        get => _phone;
        set => SetProperty(ref _phone, value);
    }
    
    public string Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }
    
    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsRegistering
    {
        get => _isRegistering;
        set => SetProperty(ref _isRegistering, value);
    }

    public ICommand LoginCommand { get; }
    public ICommand RegisterCommand { get; }
    public ICommand SwitchToRegisterCommand { get; }
    public ICommand SwitchToLoginCommand { get; }
    public ICommand GuestLoginCommand { get; }

    public LoginViewModel(Action navigateToMainWindow)
    {
        _authService = new AuthService();
        _navigateToMainWindow = navigateToMainWindow;

        LoginCommand = new RelayCommand(async param => await LoginAsync(), CanLogin);
        RegisterCommand = new RelayCommand(async param => await RegisterAsync(), CanRegister);
        SwitchToRegisterCommand = new RelayCommand(_ => SwitchToRegister());
        SwitchToLoginCommand = new RelayCommand(_ => SwitchToLogin());
        GuestLoginCommand = new RelayCommand(_ => LoginAsGuest());
    }

    private bool CanLogin(object? param)
    {
        return !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
    }

    private async Task LoginAsync()
    {
        try
        {
            var user = await _authService.LoginAsync(Email, Password);
            
            if (user != null)
            {
                CurrentUserService.Instance.SetCurrentUser(user);
                _navigateToMainWindow();
            }
            else
            {
                ErrorMessage = "Invalid email or password.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login failed: {ex.Message}";
        }
    }

    private bool CanRegister(object? param)
    {
        return !string.IsNullOrWhiteSpace(Email) 
               && !string.IsNullOrWhiteSpace(Password)
               && !string.IsNullOrWhiteSpace(ConfirmPassword)
               && !string.IsNullOrWhiteSpace(FirstName)
               && !string.IsNullOrWhiteSpace(LastName)
               && Password == ConfirmPassword;
    }

    private async Task RegisterAsync()
    {
        try
        {
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                return;
            }

            var user = new User
            {
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                Phone = Phone,
                DeliveryAddress = Address,
                UserType = "Customer" // Default role for registered users
            };

            var registeredUser = await _authService.RegisterAsync(user, Password);
            CurrentUserService.Instance.SetCurrentUser(registeredUser);
            _navigateToMainWindow();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Registration failed: {ex.Message}";
        }
    }

    private void SwitchToRegister()
    {
        IsRegistering = true;
        ErrorMessage = string.Empty;
    }

    private void SwitchToLogin()
    {
        IsRegistering = false;
        ErrorMessage = string.Empty;
    }

    private void LoginAsGuest()
    {
        var guestUser = _authService.CreateGuestUser();
        CurrentUserService.Instance.SetCurrentUser(guestUser);
        _navigateToMainWindow();
    }
} 