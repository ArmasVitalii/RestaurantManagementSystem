using System.Data;
using System.Collections.ObjectModel;
using System.Windows.Input;
using RestaurantManagement.Commands;
using RestaurantManagement.Services;

namespace RestaurantManagement.ViewModels;

public class PopularDishesReportItem : BaseViewModel
{
    public int DishID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public int TotalOrdered { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public string Availability { get; set; }
    public string Allergens { get; set; }
}

public class PopularDishesViewModel : BaseViewModel
{
    private readonly StoredProcedureService _storedProcedureService;
    private DateTime _startDate;
    private DateTime _endDate;
    private int _topCount = 10;
    private bool _isLoading;
    private string _errorMessage;
    private bool _hasData;

    public ObservableCollection<PopularDishesReportItem> PopularDishes { get; } = new();

    public DateTime StartDate
    {
        get => _startDate;
        set => SetProperty(ref _startDate, value);
    }

    public DateTime EndDate
    {
        get => _endDate;
        set => SetProperty(ref _endDate, value);
    }

    public int TopCount
    {
        get => _topCount;
        set => SetProperty(ref _topCount, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool HasData
    {
        get => _hasData;
        set => SetProperty(ref _hasData, value);
    }

    public ICommand LoadReportCommand { get; }
    public ICommand ExportReportCommand { get; }

    public PopularDishesViewModel()
    {
        _storedProcedureService = new StoredProcedureService();
        
        // Inițializează perioada implicită la ultima lună
        EndDate = DateTime.Now;
        StartDate = DateTime.Now.AddMonths(-1);
        
        LoadReportCommand = new RelayCommand(_ => {
            _ = LoadPopularDishesAsync();
        });
        
        ExportReportCommand = new RelayCommand(_ => {
            _ = ExportReportAsync();
        }, _ => HasData);
    }
    
    private async Task LoadPopularDishesAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            PopularDishes.Clear();
            
            var dataTable = await _storedProcedureService.GetPopularDishesAsync(StartDate, EndDate, TopCount);
            
            if (dataTable.Rows.Count > 0)
            {
                HasData = true;
                
                foreach (DataRow row in dataTable.Rows)
                {
                    PopularDishes.Add(new PopularDishesReportItem
                    {
                        DishID = Convert.ToInt32(row["DishID"]),
                        Name = Convert.ToString(row["Name"]),
                        Description = Convert.ToString(row["Description"]),
                        Price = Convert.ToDecimal(row["Price"]),
                        Category = Convert.ToString(row["Category"]),
                        TotalOrdered = Convert.ToInt32(row["TotalOrdered"]),
                        OrderCount = Convert.ToInt32(row["OrderCount"]),
                        TotalRevenue = Convert.ToDecimal(row["TotalRevenue"]),
                        Availability = Convert.ToString(row["Availability"]),
                        Allergens = row["Allergens"] == DBNull.Value ? "Fără alergeni" : Convert.ToString(row["Allergens"])
                    });
                }
            }
            else
            {
                HasData = false;
                ErrorMessage = "Nu s-au găsit date în perioada selectată";
            }
        }
        catch (Exception ex)
        {
            HasData = false;
            ErrorMessage = $"Eroare la încărcarea datelor: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error loading popular dishes: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private async Task ExportReportAsync()
    {
        // Implementarea exportului ca CSV sau Excel poate fi adăugată aici
        // Aceasta este doar o metodă demonstrativă
        System.Diagnostics.Debug.WriteLine("Exporting report...");
    }
} 