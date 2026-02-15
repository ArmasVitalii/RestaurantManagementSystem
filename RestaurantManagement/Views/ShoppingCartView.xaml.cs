using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using RestaurantManagement.Services;
using RestaurantManagement.ViewModels;

namespace RestaurantManagement.Views;

public partial class ShoppingCartView : UserControl
{
    public ShoppingCartView()
    {
        InitializeComponent();
        
        // Set DataContext if not already set
        if (DataContext == null)
        {
            DataContext = new ShoppingCartViewModel();
        }
    }
    
    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        NavigateToMenu();
        e.Handled = true;
    }
    
    private void ContinueShopping_Click(object sender, RoutedEventArgs e)
    {
        NavigateToMenu();
    }
    
    private void NavigateToMenu()
    {
        try
        {
            // Find MainWindow
            MainWindow mainWindow = null;
            foreach (Window window in Application.Current.Windows)
            {
                if (window is MainWindow mw)
                {
                    mainWindow = mw;
                    break;
                }
            }
            
            if (mainWindow != null)
            {
                mainWindow.NavigateToMenu();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Nu s-a putut găsi fereastra principală");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Eroare la navigare: {ex.Message}");
        }
    }
    
    private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is CartItemViewModel cartItem)
        {
            cartItem.Quantity += 1;
            
            // Update the cart service
            var cartService = ShoppingCartService.Instance;
            cartService.UpdateItemQuantity(cartItem.Type, cartItem.Id, cartItem.Quantity);
        }
    }
    
    private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is CartItemViewModel cartItem && cartItem.Quantity > 1)
        {
            cartItem.Quantity -= 1;
            
            // Update the cart service
            var cartService = ShoppingCartService.Instance;
            cartService.UpdateItemQuantity(cartItem.Type, cartItem.Id, cartItem.Quantity);
        }
    }
} 