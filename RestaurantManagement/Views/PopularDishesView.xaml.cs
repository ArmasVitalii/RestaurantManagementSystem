using System.Windows.Controls;
using RestaurantManagement.ViewModels;

namespace RestaurantManagement.Views;

public partial class PopularDishesView : UserControl
{
    public PopularDishesView()
    {
        InitializeComponent();
        
        // Set DataContext if not already set
        if (DataContext == null)
        {
            DataContext = new PopularDishesViewModel();
        }
    }
} 