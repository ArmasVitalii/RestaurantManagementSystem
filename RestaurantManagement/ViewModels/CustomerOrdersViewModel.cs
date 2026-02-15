using System.Collections.ObjectModel;
using System.Windows.Input;
using RestaurantManagement.Commands;
using RestaurantManagement.Models;
using RestaurantManagement.Services;

namespace RestaurantManagement.ViewModels;

public class CustomerOrdersViewModel : BaseViewModel
{
    private readonly OrderService _orderService;
    private readonly CurrentUserService _currentUserService;
    private bool _isLoading;
    private OrderViewModel _selectedOrder;
    private bool _showActiveOnly;

    public ObservableCollection<OrderViewModel> Orders { get; } = new();
    
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }
    
    public OrderViewModel SelectedOrder
    {
        get => _selectedOrder;
        set => SetProperty(ref _selectedOrder, value);
    }
    
    public bool ShowActiveOnly
    {
        get => _showActiveOnly;
        set
        {
            if (SetProperty(ref _showActiveOnly, value))
            {
                LoadOrdersCommand.Execute(null);
            }
        }
    }
    
    public ICommand LoadOrdersCommand { get; }
    public ICommand CancelOrderCommand { get; }
    public ICommand CreateOrderCommand { get; }
    
    public CustomerOrdersViewModel()
    {
        _orderService = new OrderService();
        _currentUserService = CurrentUserService.Instance;
        
        LoadOrdersCommand = new RelayCommand(_ => {
            _ = LoadOrdersAsync();
        });
        CancelOrderCommand = new RelayCommand(_ => {
            _ = CancelSelectedOrderAsync();
        }, CanCancelOrder);
        CreateOrderCommand = new RelayCommand(_ => NavigateToCreateOrder());
    }
    
    public async Task LoadOrdersAsync()
    {
        if (_currentUserService.CurrentUser == null || _currentUserService.CurrentUser.UserID < 0)
        {
            // Guest users cannot view orders
            return;
        }
        
        IsLoading = true;
        Orders.Clear();
        
        try
        {
            List<Order> orders;
            if (ShowActiveOnly)
            {
                orders = await _orderService.GetActiveOrdersByUserIdAsync(_currentUserService.CurrentUser.UserID);
            }
            else
            {
                orders = await _orderService.GetOrdersByUserIdAsync(_currentUserService.CurrentUser.UserID);
            }
            
            foreach (var order in orders)
            {
                Orders.Add(new OrderViewModel(order));
            }
        }
        catch (Exception ex)
        {
            // Handle error
            System.Diagnostics.Debug.WriteLine($"Error loading orders: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private async Task CancelSelectedOrderAsync()
    {
        if (SelectedOrder == null || _currentUserService.CurrentUser == null)
            return;
            
        try
        {
            var result = await _orderService.CancelOrderAsync(SelectedOrder.OrderID, _currentUserService.CurrentUser.UserID);
            
            if (result)
            {
                // Refresh orders list
                await LoadOrdersAsync();
            }
        }
        catch (Exception ex)
        {
            // Handle error
            System.Diagnostics.Debug.WriteLine($"Error canceling order: {ex.Message}");
        }
    }
    
    private bool CanCancelOrder(object? obj)
    {
        return SelectedOrder != null && 
               SelectedOrder.Status != "livrata" && 
               SelectedOrder.Status != "anulata";
    }
    
    private void NavigateToCreateOrder()
    {
        // This will be implemented in the navigation system
        // Navigation to order creation page
    }
}

public class OrderViewModel : BaseViewModel
{
    public int OrderID { get; }
    public string OrderCode { get; }
    public DateTime OrderDate { get; }
    public string Status { get; }
    public DateTime? EstimatedDeliveryTime { get; }
    public decimal FoodCost { get; }
    public decimal DeliveryCost { get; }
    public decimal DiscountValue { get; }
    public decimal TotalCost { get; }
    public ObservableCollection<OrderItemViewModel> Items { get; } = new();
    
    public string FormattedStatus 
    { 
        get
        {
            return Status switch
            {
                "inregistrata" => "Înregistrată",
                "in_preparare" => "În preparare",
                "in_livrare" => "În livrare",
                "livrata" => "Livrată",
                "anulata" => "Anulată",
                _ => Status
            };
        }
    }
    
    public string FormattedEstimatedDelivery
    {
        get
        {
            return EstimatedDeliveryTime.HasValue 
                ? EstimatedDeliveryTime.Value.ToString("dd.MM.yyyy HH:mm") 
                : "Nedeterminat";
        }
    }
    
    public OrderViewModel(Order order)
    {
        OrderID = order.OrderID;
        OrderCode = order.OrderCode ?? $"CMD-{order.OrderID}";
        OrderDate = order.OrderDate;
        Status = order.Status;
        EstimatedDeliveryTime = order.EstimatedDeliveryTime;
        FoodCost = order.FoodCost;
        DeliveryCost = order.DeliveryCost;
        DiscountValue = order.DiscountValue;
        TotalCost = order.TotalCost;
        
        // Add dishes
        if (order.OrderDishes != null)
        {
            foreach (var orderDish in order.OrderDishes)
            {
                Items.Add(new OrderItemViewModel(
                    orderDish.Dish.Name,
                    orderDish.Quantity,
                    orderDish.Dish.Price,
                    orderDish.Quantity * orderDish.Dish.Price,
                    "Dish"
                ));
            }
        }
        
        // Add menus
        if (order.OrderMenus != null)
        {
            foreach (var orderMenu in order.OrderMenus)
            {
                Items.Add(new OrderItemViewModel(
                    orderMenu.Menu.Name,
                    orderMenu.Quantity,
                    orderMenu.Menu.Price,
                    orderMenu.Quantity * orderMenu.Menu.Price,
                    "Menu"
                ));
            }
        }
    }
}

public class OrderItemViewModel
{
    public string Name { get; }
    public int Quantity { get; }
    public decimal UnitPrice { get; }
    public decimal TotalPrice { get; }
    public string Type { get; }
    
    public OrderItemViewModel(string name, int quantity, decimal unitPrice, decimal totalPrice, string type)
    {
        Name = name;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalPrice = totalPrice;
        Type = type;
    }
} 