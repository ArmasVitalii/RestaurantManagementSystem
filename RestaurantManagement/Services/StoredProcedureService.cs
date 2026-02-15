using System.Data;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;

namespace RestaurantManagement.Services;

/// <summary>
/// Serviciu pentru utilizarea procedurilor stocate create în baza de date
/// </summary>
public class StoredProcedureService
{
    private readonly string _connectionString;

    public StoredProcedureService()
    {
        _connectionString = "Server=localhost;Database=RestaurantManagement;User Id=admin;Password=1234;TrustServerCertificate=True;";
    }

    /// <summary>
    /// Obține preparatele populare dintr-o anumită perioadă
    /// </summary>
    public async Task<DataTable> GetPopularDishesAsync(DateTime startDate, DateTime endDate, int topCount = 10)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            
            using (var command = new SqlCommand("sp_GetPopularDishes", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@StartDate", startDate);
                command.Parameters.AddWithValue("@EndDate", endDate);
                command.Parameters.AddWithValue("@TopCount", topCount);
                
                var dataAdapter = new SqlDataAdapter(command);
                var dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                
                return dataTable;
            }
        }
    }
    
    /// <summary>
    /// Generează un raport de vânzări pentru o perioadă specificată
    /// </summary>
    public async Task<(DataTable GeneralStats, DataTable CategoryStats)> GenerateSalesReportAsync(DateTime startDate, DateTime endDate)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            
            using (var command = new SqlCommand("sp_GenerateSalesReport", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@StartDate", startDate);
                command.Parameters.AddWithValue("@EndDate", endDate);
                
                var dataAdapter = new SqlDataAdapter(command);
                var generalStats = new DataTable();
                var categoryStats = new DataTable();
                
                dataAdapter.Fill(generalStats);
                dataAdapter.Fill(categoryStats);
                
                return (generalStats, categoryStats);
            }
        }
    }
    
    /// <summary>
    /// Actualizează stocul unui preparat și returnează informații actualizate
    /// </summary>
    public async Task<DataTable> UpdateDishStockAsync(int dishId, int newTotalQuantity, bool? isAvailable = null)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            
            using (var command = new SqlCommand("sp_UpdateDishStock", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@DishID", dishId);
                command.Parameters.AddWithValue("@NewTotalQuantity", newTotalQuantity);
                
                if (isAvailable.HasValue)
                    command.Parameters.AddWithValue("@IsAvailable", isAvailable.Value);
                else
                    command.Parameters.AddWithValue("@IsAvailable", DBNull.Value);
                    
                var dataAdapter = new SqlDataAdapter(command);
                var dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                
                return dataTable;
            }
        }
    }
    
    /// <summary>
    /// Obține detaliile comenzilor unui client
    /// </summary>
    public async Task<(DataTable OrderSummary, DataTable DishDetails, DataTable MenuDetails)> GetCustomerOrderDetailsAsync(int userId, string statusFilter = null)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            
            using (var command = new SqlCommand("sp_GetCustomerOrderDetails", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@UserID", userId);
                
                if (!string.IsNullOrEmpty(statusFilter))
                    command.Parameters.AddWithValue("@StatusFilter", statusFilter);
                else
                    command.Parameters.AddWithValue("@StatusFilter", DBNull.Value);
                
                var dataAdapter = new SqlDataAdapter(command);
                var orderSummary = new DataTable();
                var dishDetails = new DataTable();
                var menuDetails = new DataTable();
                
                dataAdapter.Fill(orderSummary);
                dataAdapter.Fill(dishDetails);
                dataAdapter.Fill(menuDetails);
                
                return (orderSummary, dishDetails, menuDetails);
            }
        }
    }
    
    /// <summary>
    /// Identifică clienții fideli bazat pe criteriile specificate
    /// </summary>
    public async Task<DataTable> GetLoyalCustomersAsync(int minOrderCount = 3, int daysPeriod = 30)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            
            using (var command = new SqlCommand("sp_GetLoyalCustomers", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@MinOrderCount", minOrderCount);
                command.Parameters.AddWithValue("@DaysPeriod", daysPeriod);
                
                var dataAdapter = new SqlDataAdapter(command);
                var dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                
                return dataTable;
            }
        }
    }
    
    /// <summary>
    /// Obține preparatele disponibile cu categoria lor
    /// </summary>
    public async Task<DataTable> GetAvailableDishesWithCategoryAsync()
    {
        System.Diagnostics.Debug.WriteLine($"Starting GetAvailableDishesWithCategoryAsync with connection string: {_connectionString}");
        
        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                System.Diagnostics.Debug.WriteLine("Database connection opened successfully");
                
                using (var command = new SqlCommand("sp_GetAvailableDishesWithCategory", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    System.Diagnostics.Debug.WriteLine("Executing sp_GetAvailableDishesWithCategory stored procedure");
                    
                    var dataAdapter = new SqlDataAdapter(command);
                    var dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);
                    
                    System.Diagnostics.Debug.WriteLine($"Stored procedure returned {dataTable.Rows.Count} rows");
                    
                    if (dataTable.Rows.Count > 0)
                    {
                        System.Diagnostics.Debug.WriteLine("Column names:");
                        foreach (DataColumn column in dataTable.Columns)
                        {
                            System.Diagnostics.Debug.WriteLine($"- {column.ColumnName}");
                        }
                        
                        System.Diagnostics.Debug.WriteLine("First row data:");
                        var firstRow = dataTable.Rows[0];
                        foreach (DataColumn column in dataTable.Columns)
                        {
                            System.Diagnostics.Debug.WriteLine($"- {column.ColumnName}: {firstRow[column]}");
                        }
                    }
                    
                    return dataTable;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in GetAvailableDishesWithCategoryAsync: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }
    
    /// <summary>
    /// Obține clienții frecvenți
    /// </summary>
    public async Task<DataTable> GetFrequentClientsAsync(int daysPeriod = 30, int minOrderCount = 3)
    {
        System.Diagnostics.Debug.WriteLine($"Starting GetFrequentClientsAsync with parameters: daysPeriod={daysPeriod}, minOrderCount={minOrderCount}");
        
        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                System.Diagnostics.Debug.WriteLine("Database connection opened successfully");
                
                using (var command = new SqlCommand("sp_GetFrequentClients", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@DaysPeriod", daysPeriod);
                    command.Parameters.AddWithValue("@MinOrderCount", minOrderCount);
                    System.Diagnostics.Debug.WriteLine($"Executing sp_GetFrequentClients with @DaysPeriod={daysPeriod}, @MinOrderCount={minOrderCount}");
                    
                    var dataAdapter = new SqlDataAdapter(command);
                    var dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);
                    
                    System.Diagnostics.Debug.WriteLine($"Stored procedure returned {dataTable.Rows.Count} rows");
                    
                    if (dataTable.Rows.Count > 0)
                    {
                        System.Diagnostics.Debug.WriteLine("Column names:");
                        foreach (DataColumn column in dataTable.Columns)
                        {
                            System.Diagnostics.Debug.WriteLine($"- {column.ColumnName}");
                        }
                        
                        System.Diagnostics.Debug.WriteLine("First row data:");
                        var firstRow = dataTable.Rows[0];
                        foreach (DataColumn column in dataTable.Columns)
                        {
                            System.Diagnostics.Debug.WriteLine($"- {column.ColumnName}: {firstRow[column]}");
                        }
                    }
                    
                    return dataTable;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in GetFrequentClientsAsync: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }
    
    /// <summary>
    /// Adaugă un preparat nou
    /// </summary>
    public async Task<int> AddNewDishAsync(string name, string description, decimal price, 
        int portionQuantityGrams, int totalQuantityGrams, int categoryId, bool isAvailable = true)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            
            using (var command = new SqlCommand("sp_AddNewDish", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(description) ? DBNull.Value : (object)description);
                command.Parameters.AddWithValue("@Price", price);
                command.Parameters.AddWithValue("@PortionQuantityGrams", portionQuantityGrams);
                command.Parameters.AddWithValue("@TotalQuantityGrams", totalQuantityGrams);
                command.Parameters.AddWithValue("@CategoryID", categoryId);
                command.Parameters.AddWithValue("@IsAvailable", isAvailable);
                
                // Folosim DataTable pentru a obține ID-ul nou
                var dataAdapter = new SqlDataAdapter(command);
                var dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                
                if (dataTable.Rows.Count > 0 && dataTable.Columns.Contains("NewDishID"))
                {
                    return Convert.ToInt32(dataTable.Rows[0]["NewDishID"]);
                }
                
                return -1; // Nu s-a putut obține ID-ul
            }
        }
    }
    
    /// <summary>
    /// Actualizează disponibilitatea unui preparat
    /// </summary>
    public async Task<DataTable> UpdateDishAvailabilityAsync(int dishId, bool isAvailable)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            
            using (var command = new SqlCommand("sp_UpdateDishAvailability", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@DishID", dishId);
                command.Parameters.AddWithValue("@IsAvailable", isAvailable);
                
                var dataAdapter = new SqlDataAdapter(command);
                var dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                
                return dataTable;
            }
        }
    }
    
    /// <summary>
    /// Obține detaliile unei comenzi după ID
    /// </summary>
    public async Task<(DataTable OrderInfo, DataTable DishItems, DataTable MenuItems)> GetOrderDetailsByIdAsync(int orderId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            
            using (var command = new SqlCommand("sp_GetOrderDetailsById", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@OrderID", orderId);
                
                var dataAdapter = new SqlDataAdapter(command);
                var orderInfo = new DataTable();
                var dishItems = new DataTable();
                var menuItems = new DataTable();
                
                dataAdapter.Fill(orderInfo);
                dataAdapter.Fill(dishItems);
                dataAdapter.Fill(menuItems);
                
                return (orderInfo, dishItems, menuItems);
            }
        }
    }
} 