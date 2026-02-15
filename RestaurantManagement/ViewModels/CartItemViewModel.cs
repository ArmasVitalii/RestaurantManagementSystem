using RestaurantManagement.Services;

namespace RestaurantManagement.ViewModels;

public class CartItemViewModel : BaseViewModel
{
    private readonly CartItem _item;
    private int _quantity;

    public CartItem.ItemType Type => _item.Type;
    public int Id => _item.Id;
    public string Name => _item.Name;
    public string CategoryName => _item.CategoryName;
    public decimal Price => _item.Price;

    public int Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity != value && value > 0)
            {
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPrice));
            }
        }
    }

    public decimal TotalPrice => Price * Quantity;

    public CartItemViewModel(CartItem item)
    {
        _item = item;
        _quantity = item.Quantity;
    }
} 