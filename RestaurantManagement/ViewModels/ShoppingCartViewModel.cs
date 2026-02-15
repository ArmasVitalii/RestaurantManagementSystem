using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using RestaurantManagement.Commands;
using RestaurantManagement.Models;
using RestaurantManagement.Services;
using System.Windows;

namespace RestaurantManagement.ViewModels;

public class ShoppingCartViewModel : BaseViewModel
{
    private readonly ShoppingCartService _cartService;
    private readonly CurrentUserService _currentUserService;
    private bool _isLoading;
    private bool _isOrderPlaced;
    private string _successMessage;
    private string _errorMessage;
    private decimal _discountPercentage;
    private bool _isDiscountEligible;

    public ObservableCollection<CartItemViewModel> CartItems { get; } = new();

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsOrderPlaced
    {
        get => _isOrderPlaced;
        set => SetProperty(ref _isOrderPlaced, value);
    }

    public string SuccessMessage
    {
        get => _successMessage;
        set => SetProperty(ref _successMessage, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsDiscountEligible
    {
        get => _isDiscountEligible;
        set => SetProperty(ref _isDiscountEligible, value);
    }

    public decimal DiscountPercentage
    {
        get => _discountPercentage;
        set => SetProperty(ref _discountPercentage, value);
    }

    public int ItemCount => _cartService.ItemCount;
    public decimal SubTotal => _cartService.SubTotal;
    public decimal DeliveryCost => _cartService.DeliveryCost;

    public decimal DiscountValue => _isDiscountEligible ? _cartService.CalculateDiscountValue(DiscountPercentage) : 0;

    public decimal Total => _cartService.TotalWithDiscount(DiscountValue);

    public bool IsEmpty => ItemCount == 0;
    public bool IsNotEmpty => ItemCount > 0;
    public bool IsUserLoggedIn => _currentUserService.CurrentUser != null;

    public ICommand UpdateQuantityCommand { get; }
    public ICommand RemoveItemCommand { get; }
    public ICommand ClearCartCommand { get; }
    public ICommand PlaceOrderCommand { get; }
    public ICommand ContinueShoppingCommand { get; }
    public ICommand LoginCommand { get; }

    public ShoppingCartViewModel()
    {
        _cartService = ShoppingCartService.Instance;
        _currentUserService = CurrentUserService.Instance;

        // Use the generic RelayCommand for item operations
        UpdateQuantityCommand = new RelayCommand<CartItemViewModel>(UpdateItemQuantity);
        RemoveItemCommand = new RelayCommand<CartItemViewModel>(RemoveItem);
        
        // Use non-generic RelayCommand for other operations
        ClearCartCommand = new RelayCommand(_ => ClearCart());
        PlaceOrderCommand = new RelayCommand(_ => _ = PlaceOrderAsync(), _ => CanPlaceOrder());
        ContinueShoppingCommand = new RelayCommand(_ => ContinueShopping());
        LoginCommand = new RelayCommand(_ => NavigateToLogin());

        // Subscribe to cart changes
        _cartService.CartChanged += OnCartChanged;
        RefreshCartItems();

        // Check discount eligibility
        _ = CheckDiscountEligibilityAsync();
    }

    private void OnCartChanged(object? sender, EventArgs e)
    {
        RefreshCartItems();
        _ = CheckDiscountEligibilityAsync();
        OnPropertyChanged(nameof(ItemCount));
        OnPropertyChanged(nameof(SubTotal));
        OnPropertyChanged(nameof(DeliveryCost));
        OnPropertyChanged(nameof(DiscountValue));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(IsNotEmpty));
    }

    private void RefreshCartItems()
    {
        CartItems.Clear();
        foreach (var item in _cartService.GetItems())
        {
            CartItems.Add(new CartItemViewModel(item));
        }
    }

    private async Task CheckDiscountEligibilityAsync()
    {
        if (IsEmpty)
        {
            IsDiscountEligible = false;
            DiscountPercentage = 0;
            return;
        }

        var (eligible, percentage) = await _cartService.CheckOrderDiscountEligibilityAsync();
        IsDiscountEligible = eligible;
        DiscountPercentage = percentage;
        OnPropertyChanged(nameof(DiscountValue));
        OnPropertyChanged(nameof(Total));
    }

    private void UpdateItemQuantity(CartItemViewModel item)
    {
        if (item != null)
        {
            _cartService.UpdateItemQuantity(item.Type, item.Id, item.Quantity);
        }
    }

    private void RemoveItem(CartItemViewModel item)
    {
        if (item != null)
        {
            _cartService.RemoveItem(item.Type, item.Id);
        }
    }

    private void ClearCart()
    {
        _cartService.ClearCart();
    }

    private async Task PlaceOrderAsync()
    {
        if (!IsUserLoggedIn)
        {
            ErrorMessage = "Trebuie să vă autentificați pentru a plasa o comandă";
            return;
        }

        if (IsEmpty)
        {
            ErrorMessage = "Nu puteți plasa o comandă goală";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var order = await _cartService.PlaceOrderAsync();
            IsOrderPlaced = true;
            SuccessMessage = $"Comanda dvs. cu codul {order.OrderCode} a fost plasată cu succes! Vă mulțumim!";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"A apărut o eroare la plasarea comenzii: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanPlaceOrder()
    {
        return IsNotEmpty && IsUserLoggedIn && !IsLoading;
    }

    private void ContinueShopping()
    {
        // Clear any error or success messages
        IsOrderPlaced = false;
        SuccessMessage = null;
        ErrorMessage = null;

        // Navigation will be handled by the view using the MainWindow navigation methods
        if (Application.Current.MainWindow is MainWindow mainWindow)
        {
            mainWindow.NavigateToMenu();
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("MainWindow not found or not the correct type");
        }
    }

    private void NavigateToLogin()
    {
        // Navigation will be handled by the view using the MainWindow navigation methods
        if (Application.Current.MainWindow is MainWindow mainWindow)
        {
            mainWindow.NavigateToLogin();
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("MainWindow not found or not the correct type");
        }
    }

    // Clean up
    ~ShoppingCartViewModel()
    {
        _cartService.CartChanged -= OnCartChanged;
    }
} 