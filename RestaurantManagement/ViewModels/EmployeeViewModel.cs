using System.Collections.ObjectModel;
using System.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using RestaurantManagement.Commands;
using RestaurantManagement.Models;
using RestaurantManagement.Services;

namespace RestaurantManagement.ViewModels;

public class EmployeeViewModel : BaseViewModel
{
    private readonly OrderService _orderService;
    private readonly StoredProcedureService _storedProcedureService;
    private EmployeeViewMode _currentMode = EmployeeViewMode.AllOrders;
    private OrderDetailViewModel? _selectedOrder;
    private EmployeeCategoryViewModel? _selectedCategory;
    private DishViewModel? _selectedDish;
    private MenuViewModel? _selectedMenu;
    private AllergenViewModel? _selectedAllergen;
    private bool _isLoading;
    private bool _showReports = false;
    private int _lowStockThreshold = 1000; // Default threshold value

    // Colecții pentru datele din interfață
    public ObservableCollection<OrderDetailViewModel> Orders { get; } = new();
    public ObservableCollection<DishViewModel> LowStockDishes { get; } = new();
    public ObservableCollection<EmployeeCategoryViewModel> Categories { get; } = new();
    public ObservableCollection<DishViewModel> Dishes { get; } = new();
    public ObservableCollection<MenuViewModel> Menus { get; } = new();
    public ObservableCollection<AllergenViewModel> Allergens { get; } = new();
    public ObservableCollection<AvailableDishViewModel> AvailableDishes { get; } = new();
    public ObservableCollection<FrequentClientViewModel> FrequentClients { get; } = new();
    public ObservableCollection<EmployeeOrderItemViewModel> OrderDetails { get; } = new();

    // Proprietăți pentru mode
    public EmployeeViewMode CurrentMode
    {
        get => _currentMode;
        set => SetProperty(ref _currentMode, value);
    }

    // Proprietăți pentru selecții
    public OrderDetailViewModel? SelectedOrder
    {
        get => _selectedOrder;
        set
        {
            if (SetProperty(ref _selectedOrder, value) && value != null)
            {
                _ = LoadOrderDetailsAsync(value.OrderId);
            }
        }
    }

    public EmployeeCategoryViewModel? SelectedCategory
    {
        get => _selectedCategory;
        set => SetProperty(ref _selectedCategory, value);
    }

    public DishViewModel? SelectedDish
    {
        get => _selectedDish;
        set => SetProperty(ref _selectedDish, value);
    }

    public MenuViewModel? SelectedMenu
    {
        get => _selectedMenu;
        set => SetProperty(ref _selectedMenu, value);
    }

    public AllergenViewModel? SelectedAllergen
    {
        get => _selectedAllergen;
        set => SetProperty(ref _selectedAllergen, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool ShowReports
    {
        get => _showReports;
        set => SetProperty(ref _showReports, value);
    }
    
    public int LowStockThreshold
    {
        get => _lowStockThreshold;
        set => SetProperty(ref _lowStockThreshold, value);
    }

    public ICommand LoadAllOrdersCommand { get; }
    public ICommand LoadActiveOrdersCommand { get; }
    public ICommand LoadLowStockCommand { get; }
    public ICommand LoadCategoriesCommand { get; }
    public ICommand LoadDishesCommand { get; }
    public ICommand LoadMenusCommand { get; }
    public ICommand LoadAllergensCommand { get; }
    public ICommand ShowPopularDishesReportCommand { get; }
    public ICommand ShowSalesReportCommand { get; }
    public ICommand ShowLoyalCustomersCommand { get; }
    public ICommand UpdateOrderStatusCommand { get; }
    public ICommand AddCategoryCommand { get; }
    public ICommand UpdateCategoryCommand { get; }
    public ICommand DeleteCategoryCommand { get; }
    public ICommand AddDishCommand { get; }
    public ICommand UpdateDishCommand { get; }
    public ICommand DeleteDishCommand { get; }
    public ICommand AddMenuCommand { get; }
    public ICommand UpdateMenuCommand { get; }
    public ICommand DeleteMenuCommand { get; }
    public ICommand AddAllergenCommand { get; }
    public ICommand UpdateAllergenCommand { get; }
    public ICommand DeleteAllergenCommand { get; }
    
    public ICommand LoadAvailableDishesCommand { get; }
    public ICommand LoadFrequentClientsCommand { get; }
    public ICommand UpdateDishAvailabilityCommand { get; }
    public ICommand UpdateStockCommand { get; }

    public EmployeeViewModel()
    {
        _orderService = new OrderService();
        _storedProcedureService = new StoredProcedureService();

        LoadAllOrdersCommand = new RelayCommand(_ => {
            _ = LoadAllOrdersAsync();
        });
        LoadActiveOrdersCommand = new RelayCommand(_ => {
            _ = LoadActiveOrdersAsync();
        });
        LoadLowStockCommand = new RelayCommand(_ => {
            _ = LoadLowStockDishesAsync();
        });
        LoadCategoriesCommand = new RelayCommand(_ => {
            _ = LoadCategoriesAsync();
        });
        LoadDishesCommand = new RelayCommand(_ => {
            _ = LoadDishesAsync();
        });
        LoadMenusCommand = new RelayCommand(_ => {
            _ = LoadMenusAsync();
        });
        LoadAllergensCommand = new RelayCommand(_ => {
            _ = LoadAllergensAsync();
        });
        
        // Comenzi pentru rapoarte
        ShowPopularDishesReportCommand = new RelayCommand(_ => {
            CurrentMode = EmployeeViewMode.PopularDishesReport;
        });
        ShowSalesReportCommand = new RelayCommand(_ => {
            CurrentMode = EmployeeViewMode.SalesReport;
        });
        ShowLoyalCustomersCommand = new RelayCommand(_ => {
            CurrentMode = EmployeeViewMode.LoyalCustomers;
        });

        UpdateOrderStatusCommand = new RelayCommand(param => {
            if (param is string status)
                _ = UpdateOrderStatusAsync(status);
        }, _ => SelectedOrder != null);

        AddCategoryCommand = new RelayCommand(param => {
            if (param is EmployeeCategoryViewModel vm && vm.CategoryName != null && vm.CategoryName.Length > 0)
                _ = AddCategoryAsync(vm);
            else
                _ = AddCategoryAsync(new EmployeeCategoryViewModel { CategoryName = "Nou" });
        });
        
        UpdateCategoryCommand = new RelayCommand(param => {
            if (param is EmployeeCategoryViewModel vm)
                _ = UpdateCategoryAsync(vm);
            else if (SelectedCategory != null)
                _ = UpdateCategoryAsync(SelectedCategory);
        }, _ => SelectedCategory != null);
        
        DeleteCategoryCommand = new RelayCommand(param => {
            if (param is EmployeeCategoryViewModel vm)
                _ = DeleteCategoryAsync(vm);
            else if (SelectedCategory != null)
                _ = DeleteCategoryAsync(SelectedCategory);
        }, _ => SelectedCategory != null);

        AddDishCommand = new RelayCommand(param => {
            if (param is DishViewModel vm)
                _ = AddDishAsync(vm);
        });
        
        UpdateDishCommand = new RelayCommand(param => {
            if (param is DishViewModel vm)
                _ = UpdateDishAsync(vm);
        }, param => param is DishViewModel);
        
        DeleteDishCommand = new RelayCommand(param => {
            if (param is DishViewModel vm)
                _ = DeleteDishAsync(vm);
        }, param => param is DishViewModel);

        AddMenuCommand = new RelayCommand(param => {
            if (param is MenuViewModel vm)
                _ = AddMenuAsync(vm);
        });
        
        UpdateMenuCommand = new RelayCommand(param => {
            if (param is MenuViewModel vm)
                _ = UpdateMenuAsync(vm);
        }, param => param is MenuViewModel);
        
        DeleteMenuCommand = new RelayCommand(param => {
            if (param is MenuViewModel vm)
                _ = DeleteMenuAsync(vm);
        }, param => param is MenuViewModel);

        AddAllergenCommand = new RelayCommand(param => {
            if (param is AllergenViewModel vm)
                _ = AddAllergenAsync(vm);
        });
        
        UpdateAllergenCommand = new RelayCommand(param => {
            if (param is AllergenViewModel vm)
                _ = UpdateAllergenAsync(vm);
        }, param => param is AllergenViewModel);
        
        DeleteAllergenCommand = new RelayCommand(param => {
            if (param is AllergenViewModel vm)
                _ = DeleteAllergenAsync(vm);
        }, param => param is AllergenViewModel);
        
        // Comenzi noi pentru funcționalitățile adăugate
        LoadAvailableDishesCommand = new RelayCommand(_ => {
            _ = LoadAvailableDishesAsync();
        });
        
        LoadFrequentClientsCommand = new RelayCommand(_ => {
            _ = LoadFrequentClientsAsync();
        });
        
        UpdateDishAvailabilityCommand = new RelayCommand(param => {
            if (param is object[] args && args.Length == 2 && 
                args[0] is int dishId && args[1] is bool isAvailable)
            {
                _ = UpdateDishAvailabilityAsync(dishId, isAvailable);
            }
        });
        
        // Comanda pentru actualizarea stocului
        UpdateStockCommand = new RelayCommand(param => {
            if (param is DishViewModel dish)
            {
                _ = UpdateStockAsync(dish.Id, dish.TotalQuantity);
            }
        });
    }

    private async Task LoadAllOrdersAsync()
    {
        Orders.Clear();
        CurrentMode = EmployeeViewMode.AllOrders;

        try
        {
            var orders = await _orderService.GetAllOrdersAsync();
            foreach (var order in orders)
            {
                Orders.Add(new OrderDetailViewModel(order));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading all orders: {ex.Message}");
        }
    }

    private async Task LoadActiveOrdersAsync()
    {
        Orders.Clear();
        CurrentMode = EmployeeViewMode.ActiveOrders;

        try
        {
            var orders = await _orderService.GetAllActiveOrdersAsync();
            foreach (var order in orders)
            {
                Orders.Add(new OrderDetailViewModel(order));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading active orders: {ex.Message}");
        }
    }

    private async Task LoadLowStockDishesAsync()
    {
        LowStockDishes.Clear();
        CurrentMode = EmployeeViewMode.LowStock;
        IsLoading = true;

        try
        {
            System.Diagnostics.Debug.WriteLine($"Loading low stock dishes with threshold: {LowStockThreshold} grams");
            var dishes = await _orderService.GetLowStockDishesAsync(LowStockThreshold);
            
            foreach (var dish in dishes)
            {
                LowStockDishes.Add(new DishViewModel(dish));
            }
            
            System.Diagnostics.Debug.WriteLine($"Found {LowStockDishes.Count} dishes with low stock");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading low stock dishes: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadCategoriesAsync()
    {
        Categories.Clear();
        CurrentMode = EmployeeViewMode.Categories;

        try
        {
            var categories = await _orderService.GetCategoriesWithItemsAsync();
            foreach (var category in categories)
            {
                var categoryViewModel = new EmployeeCategoryViewModel(category);
                Categories.Add(categoryViewModel);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading categories: {ex.Message}");
        }
    }

    private async Task LoadDishesAsync()
    {
        Dishes.Clear();
        CurrentMode = EmployeeViewMode.Dishes;

        try
        {
            var categories = await _orderService.GetCategoriesWithItemsAsync();
            foreach (var category in categories)
            {
                foreach (var dish in category.Dishes)
                {
                    Dishes.Add(new DishViewModel(dish));
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading dishes: {ex.Message}");
        }
    }

    private async Task LoadMenusAsync()
    {
        Menus.Clear();
        CurrentMode = EmployeeViewMode.Menus;

        try
        {
            var categories = await _orderService.GetCategoriesWithItemsAsync();
            foreach (var category in categories)
            {
                foreach (var menu in category.Menus)
                {
                    Menus.Add(new MenuViewModel(menu));
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading menus: {ex.Message}");
        }
    }

    private async Task LoadAllergensAsync()
    {
        Allergens.Clear();
        CurrentMode = EmployeeViewMode.Allergens;

        try
        {
            var allergens = await _orderService.GetAllergensAsync();
            foreach (var allergen in allergens)
            {
                Allergens.Add(new AllergenViewModel(allergen));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading allergens: {ex.Message}");
        }
    }

    private async Task UpdateOrderStatusAsync(string newStatus)
    {
        if (SelectedOrder == null) return;

        try
        {
            var success = await _orderService.UpdateOrderStatusAsync(SelectedOrder.OrderId, newStatus);
            if (success)
            {
                // Reload the current view
                if (CurrentMode == EmployeeViewMode.AllOrders)
                {
                    await LoadAllOrdersAsync();
                }
                else if (CurrentMode == EmployeeViewMode.ActiveOrders)
                {
                    await LoadActiveOrdersAsync();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating order status: {ex.Message}");
        }
    }

    // CRUD operations for categories, dishes, menus, and allergens
    // These would connect to dialog windows or forms for adding/editing

    private async Task AddCategoryAsync(EmployeeCategoryViewModel vm)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"Adding category: Name={vm.CategoryName}");
            var category = new Category { Name = vm.CategoryName };
            var addedCategory = await _orderService.AddCategoryAsync(category);
            
            // Update the view model with the database-generated ID
            vm.Id = addedCategory.CategoryID;
            System.Diagnostics.Debug.WriteLine($"Category added with ID={addedCategory.CategoryID}");
            
            // Optionally refresh the full list
            await LoadCategoriesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding category: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    private async Task UpdateCategoryAsync(EmployeeCategoryViewModel vm)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"Updating category: ID={vm.Id}, Name={vm.CategoryName}");
            
            // Extra validation to make sure we have a valid ID
            if (vm.Id <= 0)
            {
                System.Diagnostics.Debug.WriteLine($"Cannot update category with invalid ID: {vm.Id}");
                // This might be a new category that wasn't properly saved
                // Try to add it instead
                await AddCategoryAsync(vm);
                return;
            }
            
            var category = new Category { CategoryID = vm.Id, Name = vm.CategoryName };
            bool success = await _orderService.UpdateCategoryAsync(category);
            System.Diagnostics.Debug.WriteLine($"Category update result: {success}");
            
            if (success)
            {
                System.Diagnostics.Debug.WriteLine("Reloading categories after successful update");
                await LoadCategoriesAsync();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Category update failed");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating category: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    private async Task DeleteCategoryAsync(EmployeeCategoryViewModel vm)
    {
        try
        {
            await _orderService.DeleteCategoryAsync(vm.Id);
            await LoadCategoriesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting category: {ex.Message}");
        }
    }

    private async Task AddDishAsync(DishViewModel vm)
    {
        try
        {
            var dish = new Dish
            {
                Name = vm.Name,
                Description = vm.Description,
                Price = vm.Price,
                PortionQuantityGrams = vm.PortionSize,
                TotalQuantityGrams = vm.TotalQuantity,
                CategoryID = vm.CategoryId,
                IsAvailable = vm.IsAvailable
            };
            
            await _orderService.AddDishAsync(dish);
            await LoadDishesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding dish: {ex.Message}");
        }
    }

    private async Task UpdateDishAsync(DishViewModel vm)
    {
        try
        {
            var dish = new Dish
            {
                DishID = vm.Id,
                Name = vm.Name,
                Description = vm.Description,
                Price = vm.Price,
                PortionQuantityGrams = vm.PortionSize,
                TotalQuantityGrams = vm.TotalQuantity,
                CategoryID = vm.CategoryId,
                IsAvailable = vm.IsAvailable
            };
            
            await _orderService.UpdateDishAsync(dish);
            await LoadDishesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating dish: {ex.Message}");
        }
    }

    private async Task DeleteDishAsync(DishViewModel vm)
    {
        try
        {
            await _orderService.DeleteDishAsync(vm.Id);
            await LoadDishesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting dish: {ex.Message}");
        }
    }

    private async Task AddMenuAsync(MenuViewModel vm)
    {
        try
        {
            var menu = new Menu
            {
                Name = vm.Name,
                Description = vm.Description,
                Price = vm.Price,
                DiscountPercent = vm.DiscountPercent,
                CategoryID = vm.CategoryId,
                IsAvailable = vm.IsAvailable
            };
            
            await _orderService.AddMenuAsync(menu);
            await LoadMenusAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding menu: {ex.Message}");
        }
    }

    private async Task UpdateMenuAsync(MenuViewModel vm)
    {
        try
        {
            var menu = new Menu
            {
                MenuID = vm.Id,
                Name = vm.Name,
                Description = vm.Description,
                Price = vm.Price,
                DiscountPercent = vm.DiscountPercent,
                CategoryID = vm.CategoryId,
                IsAvailable = vm.IsAvailable
            };
            
            await _orderService.UpdateMenuAsync(menu);
            await LoadMenusAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating menu: {ex.Message}");
        }
    }

    private async Task DeleteMenuAsync(MenuViewModel vm)
    {
        try
        {
            await _orderService.DeleteMenuAsync(vm.Id);
            await LoadMenusAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting menu: {ex.Message}");
        }
    }

    private async Task AddAllergenAsync(AllergenViewModel vm)
    {
        try
        {
            var allergen = new Allergen
            {
                Name = vm.Name
            };
            
            await _orderService.AddAllergenAsync(allergen);
            await LoadAllergensAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding allergen: {ex.Message}");
        }
    }

    private async Task UpdateAllergenAsync(AllergenViewModel vm)
    {
        try
        {
            var allergen = new Allergen
            {
                AllergenID = vm.Id,
                Name = vm.Name
            };
            
            await _orderService.UpdateAllergenAsync(allergen);
            await LoadAllergensAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating allergen: {ex.Message}");
        }
    }

    private async Task DeleteAllergenAsync(AllergenViewModel vm)
    {
        try
        {
            await _orderService.DeleteAllergenAsync(vm.Id);
            await LoadAllergensAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting allergen: {ex.Message}");
        }
    }
    
    // Metode noi pentru noile proceduri stocate
    
    /// <summary>
    /// Încarcă preparatele disponibile cu categoria lor
    /// </summary>
    private async Task LoadAvailableDishesAsync()
    {
        AvailableDishes.Clear();
        CurrentMode = EmployeeViewMode.AvailableDishes;
        IsLoading = true;
        
        try
        {
            var dataTable = await _storedProcedureService.GetAvailableDishesWithCategoryAsync();
            System.Diagnostics.Debug.WriteLine($"Available dishes: Found {dataTable.Rows.Count} rows");
            
            foreach (DataRow row in dataTable.Rows)
            {
                AvailableDishes.Add(new AvailableDishViewModel
                {
                    DishId = Convert.ToInt32(row["DishID"]),
                    DishName = row["DishName"].ToString(),
                    Price = Convert.ToDecimal(row["Price"]),
                    PortionQuantityGrams = Convert.ToInt32(row["PortionQuantityGrams"]),
                    CategoryName = row["CategoryName"].ToString()
                });
            }
            System.Diagnostics.Debug.WriteLine($"AvailableDishes collection count: {AvailableDishes.Count}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading available dishes: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Încarcă clienții frecvenți
    /// </summary>
    private async Task LoadFrequentClientsAsync()
    {
        FrequentClients.Clear();
        CurrentMode = EmployeeViewMode.FrequentClients;
        IsLoading = true;
        
        try
        {
            var dataTable = await _storedProcedureService.GetFrequentClientsAsync();
            System.Diagnostics.Debug.WriteLine($"Frequent clients: Found {dataTable.Rows.Count} rows");
            
            foreach (DataRow row in dataTable.Rows)
            {
                FrequentClients.Add(new FrequentClientViewModel
                {
                    UserId = Convert.ToInt32(row["UserID"]),
                    FirstName = row["FirstName"].ToString(),
                    LastName = row["LastName"].ToString(),
                    Email = row["Email"].ToString(),
                    Phone = row["Phone"].ToString(),
                    OrdersCount = Convert.ToInt32(row["OrdersCount"]),
                    TotalSpent = Convert.ToDecimal(row["TotalSpent"]),
                    AverageOrderValue = Convert.ToDecimal(row["AverageOrderValue"])
                });
            }
            System.Diagnostics.Debug.WriteLine($"FrequentClients collection count: {FrequentClients.Count}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading frequent clients: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Actualizează disponibilitatea unui preparat
    /// </summary>
    private async Task UpdateDishAvailabilityAsync(int dishId, bool isAvailable)
    {
        IsLoading = true;
        
        try
        {
            var result = await _storedProcedureService.UpdateDishAvailabilityAsync(dishId, isAvailable);
            
            // Reîncărcăm datele în funcție de modul curent
            if (CurrentMode == EmployeeViewMode.Dishes)
            {
                await LoadDishesAsync();
            }
            else if (CurrentMode == EmployeeViewMode.AvailableDishes)
            {
                await LoadAvailableDishesAsync();
            }
            else if (CurrentMode == EmployeeViewMode.LowStock)
            {
                await LoadLowStockDishesAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating dish availability: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Încarcă detaliile unei comenzi
    /// </summary>
    private async Task LoadOrderDetailsAsync(int orderId)
    {
        OrderDetails.Clear();
        IsLoading = true;
        
        try
        {
            var (orderInfo, dishItems, menuItems) = await _storedProcedureService.GetOrderDetailsByIdAsync(orderId);
            
            // Procesăm preparatele
            foreach (DataRow row in dishItems.Rows)
            {
                OrderDetails.Add(new EmployeeOrderItemViewModel
                {
                    ItemType = row["ItemType"].ToString(),
                    ItemName = row["ItemName"].ToString(),
                    Quantity = Convert.ToInt32(row["Quantity"]),
                    UnitPrice = Convert.ToDecimal(row["UnitPrice"]),
                    TotalPrice = Convert.ToDecimal(row["TotalPrice"])
                });
            }
            
            // Procesăm meniurile
            foreach (DataRow row in menuItems.Rows)
            {
                OrderDetails.Add(new EmployeeOrderItemViewModel
                {
                    ItemType = row["ItemType"].ToString(),
                    ItemName = row["ItemName"].ToString(),
                    Quantity = Convert.ToInt32(row["Quantity"]),
                    UnitPrice = Convert.ToDecimal(row["UnitPrice"]),
                    TotalPrice = Convert.ToDecimal(row["TotalPrice"])
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading order details: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task UpdateStockAsync(int dishId, int newStock)
    {
        IsLoading = true;
        
        try
        {
            System.Diagnostics.Debug.WriteLine($"Updating stock for dish ID {dishId} to {newStock} grams");
            bool success = await _orderService.UpdateStockAsync(dishId, newStock);
            
            if (success)
            {
                System.Diagnostics.Debug.WriteLine("Stock updated successfully");
                // Refresh the list to show the updated quantities and availability
                await LoadLowStockDishesAsync();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Failed to update stock");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating stock: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public enum EmployeeViewMode
{
    AllOrders,
    ActiveOrders,
    LowStock,
    Categories,
    Dishes,
    Menus,
    Allergens,
    PopularDishesReport,
    SalesReport,
    LoyalCustomers,
    AvailableDishes,
    FrequentClients,
    OrderDetails
}

// Model-uri pentru elementele din interfață

public class OrderDetailViewModel : BaseViewModel
{
    public int OrderId { get; }
    public string OrderCode { get; }
    public DateTime OrderDate { get; }
    public string Status { get; }
    public DateTime? EstimatedDeliveryTime { get; }
    public decimal FoodCost { get; }
    public decimal DeliveryCost { get; }
    public decimal TotalCost { get; }
    public string CustomerName { get; }
    public string CustomerPhone { get; }
    public string DeliveryAddress { get; }
    public ObservableCollection<EmployeeOrderItemViewModel> Items { get; } = new();

    public OrderDetailViewModel(Order order)
    {
        OrderId = order.OrderID;
        OrderCode = order.OrderCode;
        OrderDate = order.OrderDate;
        Status = order.Status;
        EstimatedDeliveryTime = order.EstimatedDeliveryTime;
        FoodCost = order.FoodCost;
        DeliveryCost = order.DeliveryCost;
        TotalCost = order.TotalCost;
        CustomerName = order.User.FirstName + " " + order.User.LastName;
        CustomerPhone = order.User.Phone;
        DeliveryAddress = order.User.DeliveryAddress;

        if (order.OrderDishes != null)
        {
            foreach (var dishOrder in order.OrderDishes)
            {
                Items.Add(new EmployeeOrderItemViewModel
                {
                    ItemType = "Dish",
                    ItemName = dishOrder.Dish.Name,
                    Quantity = dishOrder.Quantity,
                    UnitPrice = dishOrder.Dish.Price,
                    TotalPrice = dishOrder.Quantity * dishOrder.Dish.Price
                });
            }
        }

        if (order.OrderMenus != null)
        {
            foreach (var menuOrder in order.OrderMenus)
            {
                Items.Add(new EmployeeOrderItemViewModel
                {
                    ItemType = "Menu",
                    ItemName = menuOrder.Menu.Name,
                    Quantity = menuOrder.Quantity,
                    UnitPrice = menuOrder.Menu.Price,
                    TotalPrice = menuOrder.Quantity * menuOrder.Menu.Price
                });
            }
        }
    }
}



public partial class DishViewModel
{
    public int Id { get; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int PortionSize { get; set; }
    public int TotalQuantity { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
    public bool IsAvailable { get; set; }

    public DishViewModel(Dish dish)
    {
        Id = dish.DishID;
        Name = dish.Name;
        Description = dish.Description ?? "";
        Price = dish.Price;
        PortionSize = dish.PortionQuantityGrams;
        TotalQuantity = dish.TotalQuantityGrams;
        CategoryId = dish.CategoryID;
        CategoryName = dish.Category?.Name ?? "";
        IsAvailable = dish.IsAvailable;
    }

    public DishViewModel()
    {
        Name = "";
        Description = "";
        Price = 0;
        PortionSize = 0;
        TotalQuantity = 0;
        CategoryId = 0;
        CategoryName = "";
        IsAvailable = true;
    }
}

public partial class MenuViewModel
{
    public int Id { get; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public decimal DiscountPercent { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
    public bool IsAvailable { get; set; }
    public ObservableCollection<DishViewModel> Components { get; } = new();

    public MenuViewModel(Menu menu)
    {
        Id = menu.MenuID;
        Name = menu.Name;
        Description = menu.Description ?? "";
        Price = menu.Price;
        DiscountPercent = menu.DiscountPercent;
        CategoryId = menu.CategoryID;
        CategoryName = menu.Category?.Name ?? "";
        IsAvailable = menu.IsAvailable;

        // Add component dishes if available
        if (menu.Components != null)
        {
            foreach (var component in menu.Components)
            {
                if (component.Dish != null)
                {
                    Components.Add(new DishViewModel(component.Dish));
                }
            }
        }
    }

    public MenuViewModel()
    {
        Name = "";
        Description = "";
        Price = 0;
        DiscountPercent = 0;
        CategoryId = 0;
        CategoryName = "";
        IsAvailable = true;
    }
}

public partial class AllergenViewModel
{
    public int Id { get; }
    public string Name { get; set; }

    public AllergenViewModel(Allergen allergen)
    {
        Id = allergen.AllergenID;
        Name = allergen.Name;
    }

    public AllergenViewModel()
    {
        Name = "";
    }
}

// Modele noi pentru noile funcționalități

public class AvailableDishViewModel : BaseViewModel
{
    public int DishId { get; set; }
    public string DishName { get; set; } = "";
    public decimal Price { get; set; }
    public int PortionQuantityGrams { get; set; }
    public string CategoryName { get; set; } = "";
}

public class FrequentClientViewModel : BaseViewModel
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public int OrdersCount { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal AverageOrderValue { get; set; }
}

 