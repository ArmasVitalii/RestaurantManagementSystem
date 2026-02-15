using System.Windows;
using System.Windows.Controls;
using RestaurantManagement.Services;
using RestaurantManagement.ViewModels;

namespace RestaurantManagement.Views;

public partial class MenuItemView : UserControl
{
    public MenuItemView()
    {
        InitializeComponent();
        
        // Update button visibility based on current user
        UpdateButtonVisibility();
        
        // Find MainWindow to handle login navigation
        var mainWindow = Application.Current.MainWindow;
    }
    
    private void UpdateButtonVisibility()
    {
        var currentUserService = CurrentUserService.Instance;
        bool isGuest = currentUserService.CurrentUser == null || currentUserService.CurrentUser.UserType == "Guest";
        
        if (AddToCartButton != null && LoginButton != null)
        {
            AddToCartButton.Visibility = isGuest ? Visibility.Collapsed : Visibility.Visible;
            LoginButton.Visibility = isGuest ? Visibility.Visible : Visibility.Collapsed;
        }
    }
    
    private async void AddToCart_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MenuItemViewModel menuItem)
        {
            var cartService = ShoppingCartService.Instance;
            bool success = false;
            
            if (menuItem.Type == MenuItemViewModel.MenuItemType.Dish)
            {
                success = await cartService.AddDish(menuItem.Id, 1);
            }
            else if (menuItem.Type == MenuItemViewModel.MenuItemType.Menu)
            {
                success = await cartService.AddMenu(menuItem.Id, 1);
            }
            
            // Show a temporary notification
            var button = sender as Button;
            if (button != null)
            {
                string originalContent = button.Content?.ToString();
                if (success)
                {
                    button.Content = "Adăugat ✓";
                }
                else
                {
                    button.Content = "Eroare ⚠";
                }
                button.IsEnabled = false;
                
                // Reset button after 1.5 seconds
                var timer = new System.Threading.Timer(_ =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        button.Content = originalContent;
                        button.IsEnabled = true;
                    });
                }, null, 1500, System.Threading.Timeout.Infinite);
            }
        }
    }
    
    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        // Navigate to login view
        var mainWindow = Application.Current.MainWindow as MainWindow;
        if (mainWindow != null)
        {
            mainWindow.NavigateToLogin();
        }
    }
} 