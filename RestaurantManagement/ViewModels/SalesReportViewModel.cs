using System.Data;
using System.Collections.ObjectModel;
using System.Windows.Input;
using RestaurantManagement.Commands;
using RestaurantManagement.Services;

namespace RestaurantManagement.ViewModels;

public class CategorySalesInfo : BaseViewModel
{
    public string CategoryName { get; set; }
    public int OrderCount { get; set; }
    public int ItemsSold { get; set; }
    public decimal Revenue { get; set; }
    public decimal PercentageOfSales { get; set; }
}

public class SalesReportViewModel : BaseViewModel
{
    private readonly StoredProcedureService _storedProcedureService;
    private DateTime _startDate;
    private DateTime _endDate;
    private bool _isLoading;
    private string _errorMessage;
    private bool _hasData;

    // Statistici generale
    private int _totalOrders;
    private int _uniqueCustomers;
    private decimal _totalSales;
    private decimal _averageOrderValue;
    private decimal _totalDiscountsGiven;
    private int _completedOrders;
    private int _cancelledOrders;
    private int _peakOrderHour;
    
    // Lista de categorii cu statistici
    public ObservableCollection<CategorySalesInfo> CategorySales { get; } = new();

    #region Properties
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
    
    public int TotalOrders
    {
        get => _totalOrders;
        set => SetProperty(ref _totalOrders, value);
    }
    
    public int UniqueCustomers
    {
        get => _uniqueCustomers;
        set => SetProperty(ref _uniqueCustomers, value);
    }
    
    public decimal TotalSales
    {
        get => _totalSales;
        set => SetProperty(ref _totalSales, value);
    }
    
    public decimal AverageOrderValue
    {
        get => _averageOrderValue;
        set => SetProperty(ref _averageOrderValue, value);
    }
    
    public decimal TotalDiscountsGiven
    {
        get => _totalDiscountsGiven;
        set => SetProperty(ref _totalDiscountsGiven, value);
    }
    
    public int CompletedOrders
    {
        get => _completedOrders;
        set => SetProperty(ref _completedOrders, value);
    }
    
    public int CancelledOrders
    {
        get => _cancelledOrders;
        set => SetProperty(ref _cancelledOrders, value);
    }
    
    public int PeakOrderHour
    {
        get => _peakOrderHour;
        set => SetProperty(ref _peakOrderHour, value);
    }
    #endregion

    public ICommand GenerateReportCommand { get; }
    public ICommand ExportReportCommand { get; }

    public SalesReportViewModel()
    {
        _storedProcedureService = new StoredProcedureService();
        
        // Inițializează perioada implicită la ultima lună
        EndDate = DateTime.Now;
        StartDate = DateTime.Now.AddMonths(-1);
        
        GenerateReportCommand = new RelayCommand(_ => {
            _ = GenerateReportAsync();
        });
        
        ExportReportCommand = new RelayCommand(_ => {
            _ = ExportReportAsync();
        }, _ => HasData);
    }
    
    private async Task GenerateReportAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            CategorySales.Clear();
            
            var (generalStats, categoryStats) = await _storedProcedureService.GenerateSalesReportAsync(StartDate, EndDate);
            
            if (generalStats.Rows.Count > 0 && categoryStats.Rows.Count > 0)
            {
                HasData = true;
                
                // Populează statisticile generale
                DataRow generalStatsRow = generalStats.Rows[0];
                TotalOrders = generalStatsRow["TotalOrders"] != DBNull.Value ? Convert.ToInt32(generalStatsRow["TotalOrders"]) : 0;
                UniqueCustomers = generalStatsRow["UniqueCustomers"] != DBNull.Value ? Convert.ToInt32(generalStatsRow["UniqueCustomers"]) : 0;
                TotalSales = generalStatsRow["TotalSales"] != DBNull.Value ? Convert.ToDecimal(generalStatsRow["TotalSales"]) : 0;
                AverageOrderValue = generalStatsRow["AverageOrderValue"] != DBNull.Value ? Convert.ToDecimal(generalStatsRow["AverageOrderValue"]) : 0;
                TotalDiscountsGiven = generalStatsRow["TotalDiscountsGiven"] != DBNull.Value ? Convert.ToDecimal(generalStatsRow["TotalDiscountsGiven"]) : 0;
                CompletedOrders = generalStatsRow["CompletedOrders"] != DBNull.Value ? Convert.ToInt32(generalStatsRow["CompletedOrders"]) : 0;
                CancelledOrders = generalStatsRow["CancelledOrders"] != DBNull.Value ? Convert.ToInt32(generalStatsRow["CancelledOrders"]) : 0;
                PeakOrderHour = generalStatsRow["PeakOrderHour"] != DBNull.Value ? Convert.ToInt32(generalStatsRow["PeakOrderHour"]) : 0;
                
                // Populează statisticile pe categorii
                foreach (DataRow row in categoryStats.Rows)
                {
                    CategorySales.Add(new CategorySalesInfo
                    {
                        CategoryName = Convert.ToString(row["CategoryName"]),
                        OrderCount = Convert.ToInt32(row["OrderCount"]),
                        ItemsSold = Convert.ToInt32(row["ItemsSold"]),
                        Revenue = Convert.ToDecimal(row["Revenue"]),
                        PercentageOfSales = Convert.ToDecimal(row["PercentageOfSales"])
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
            ErrorMessage = $"Eroare la generarea raportului: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error generating sales report: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private async Task ExportReportAsync()
    {
        // Implementarea exportului ca CSV sau Excel poate fi adăugată aici
        System.Diagnostics.Debug.WriteLine("Exporting sales report...");
    }
} 