using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RestaurantManagement.Services;
using RestaurantManagement.ViewModels;
using RestaurantManagement.Views;

namespace RestaurantManagement;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    private string _userName;
    
    public string UserName 
    { 
        get => _userName;
        set 
        {
            if (_userName != value)
            {
                _userName = value;
                OnPropertyChanged();
            }
        }
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
    
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        
        // Set user info
        var currentUser = CurrentUserService.Instance.CurrentUser;
        if (currentUser != null)
        {
            if (currentUser.UserType == "Guest")
            {
                UserName = "Utilizator Guest";
            }
            else
            {
                UserName = $"{currentUser.FirstName} {currentUser.LastName}";
            }
            
            // Setup navigation based on user role
            SetupNavigationBasedOnRole();
        }
    }
    
    private void SetupNavigationBasedOnRole()
    {
        var currentUser = CurrentUserService.Instance.CurrentUser;
        if (currentUser != null)
        {
            if (currentUser.UserType == "Employee")
            {
                // Show employee navigation options
                EmployeeMenuButton.Visibility = Visibility.Visible;
                
                // Navigate to employee dashboard by default
                NavigateToEmployeeDashboard();
            }
            else
            {
                // For customers/guests, hide employee features
                EmployeeMenuButton.Visibility = Visibility.Collapsed;
                
                // Navigate to menu
                NavigateToMenu();
            }
        }
    }
    
    private void MenuButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateToMenu();
    }
    
    private void EmployeeMenuButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateToEmployeeDashboard();
    }
    
    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        // Clear current user
        CurrentUserService.Instance.LogOut();
        
        // Navigate back to login
        var loginView = new LoginView();
        loginView.Show();
        Close();
    }
    
    private void CartButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateToCart();
    }
    
    private void OrdersButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateToOrders();
    }
    
    public void NavigateToLogin()
    {
        // Create an Action that will refresh the main window after successful login
        Action navigateToMainWindow = () => 
        {
            // Update the user info display
            var currentUser = CurrentUserService.Instance.CurrentUser;
            if (currentUser != null)
            {
                if (currentUser.UserType == "Guest")
                {
                    UserName = "Utilizator Guest";
                }
                else
                {
                    UserName = $"{currentUser.FirstName} {currentUser.LastName}";
                }
                
                // Setup navigation based on user role
                SetupNavigationBasedOnRole();
            }
        };
        
        // Open login view with the action
        var loginViewModel = new LoginViewModel(navigateToMainWindow);
        var loginView = new LoginView
        {
            DataContext = loginViewModel
        };
        
        ContentFrame.Navigate(loginView);
    }

    public void NavigateToMenu()
    {
        try 
        {
            // Make sure the content frame is available
            if (ContentFrame == null)
            {
                System.Diagnostics.Debug.WriteLine("ContentFrame nu este disponibil pentru navigare!");
                return;
            }

            var restaurantMenuViewModel = new RestaurantMenuViewModel();
            var restaurantMenuView = new RestaurantMenuView
            {
                DataContext = restaurantMenuViewModel
            };
            
            // Navigate and trigger loading of menu data
            ContentFrame.Navigate(restaurantMenuView);
            _ = restaurantMenuViewModel.LoadMenuAsync();
            
            // Debug message to confirm navigation was successful
            System.Diagnostics.Debug.WriteLine("Navigated to menu successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation error: {ex}");
        }
    }
    
    private void NavigateToEmployeeDashboard()
    {
        var employeeViewModel = new EmployeeViewModel();
        var employeeView = new EmployeeView
        {
            DataContext = employeeViewModel
        };
        
        ContentFrame.Navigate(employeeView);
    }
    
    private void NavigateToCart()
    {
        var shoppingCartViewModel = new ShoppingCartViewModel();
        var shoppingCartView = new ShoppingCartView
        {
            DataContext = shoppingCartViewModel
        };
        
        ContentFrame.Navigate(shoppingCartView);
    }
    
    private void NavigateToOrders()
    {
        var customerOrdersViewModel = new CustomerOrdersViewModel();
        var customerOrdersView = new CustomerOrdersView
        {
            DataContext = customerOrdersViewModel
        };
        
        ContentFrame.Navigate(customerOrdersView);
    }
}