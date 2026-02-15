using System.Windows.Controls;
using RestaurantManagement.ViewModels;

namespace RestaurantManagement.Views;

public partial class EmployeeView : Page
{
    private readonly EmployeeViewModel _viewModel;
    
    public EmployeeView()
    {
        InitializeComponent();
        
        // If a ViewModel wasn't provided through DataContext, create one
        if (DataContext == null)
        {
            _viewModel = new EmployeeViewModel();
            DataContext = _viewModel;
        }
        else
        {
            _viewModel = (EmployeeViewModel)DataContext;
        }
        
        // Load orders by default
        _viewModel.LoadAllOrdersCommand.Execute(null);
    }
    
    private void CategoriesDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (e.EditAction == DataGridEditAction.Commit)
        {
            // Get the edited item
            if (e.Row.Item is EmployeeCategoryViewModel category)
            {
                // Save changes to DB by executing the update command
                _viewModel.UpdateCategoryCommand.Execute(category);
            }
        }
    }
} 