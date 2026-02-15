using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RestaurantManagement.ViewModels;

namespace RestaurantManagement.Views;

public partial class RestaurantMenuView : UserControl
{
    public RestaurantMenuView()
    {
        InitializeComponent();
    }
    
    private async void RestaurantMenuView_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is RestaurantMenuViewModel viewModel)
        {
            await viewModel.LoadMenuAsync();
        }
    }
    
    private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is RestaurantMenuViewModel viewModel && viewModel.SearchCommand.CanExecute(null))
        {
            viewModel.SearchCommand.Execute(null);
            e.Handled = true;
        }
    }
} 