using System.Windows;
using System.Windows.Controls;
using RestaurantManagement.ViewModels;

namespace RestaurantManagement.Views;

public partial class CustomerOrdersView : UserControl
{
    public CustomerOrdersView()
    {
        InitializeComponent();
    }
    
    private async void CustomerOrdersView_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is CustomerOrdersViewModel viewModel)
        {
            await viewModel.LoadOrdersAsync();
        }
    }
} 