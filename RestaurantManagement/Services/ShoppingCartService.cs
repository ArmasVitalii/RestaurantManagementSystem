using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using RestaurantManagement.Models;

namespace RestaurantManagement.Services
{
    public class CartItem
    {
        public enum ItemType
        {
            Dish,
            Menu
        }

        public int Id { get; set; }
        public ItemType Type { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => Price * Quantity;
        
        // Reference to the actual items
        public Dish Dish { get; set; }
        public Menu Menu { get; set; }
    }

    public class ShoppingCartService
    {
        // Store carts by user ID - use -1 for guest cart
        private readonly Dictionary<int, List<CartItem>> _userCarts = new Dictionary<int, List<CartItem>>();
        private readonly MenuService _menuService;
        private readonly OrderService _orderService;
        private readonly SettingsService _settingsService;
        private readonly CurrentUserService _currentUserService;

        // Singleton instance
        private static ShoppingCartService _instance;
        public static ShoppingCartService Instance => _instance ??= new ShoppingCartService();

        public event EventHandler CartChanged;

        private ShoppingCartService()
        {
            _menuService = new MenuService();
            _orderService = new OrderService();
            _settingsService = new SettingsService();
            _currentUserService = CurrentUserService.Instance;
            
            // Subscribe to user changes to handle cart transfers/resets
            _currentUserService.UserChanged += OnUserChanged;
        }
        
        private void OnUserChanged(object? sender, EventArgs e)
        {
            // Notify that cart might have changed when user changes
            NotifyCartChanged();
        }

        // Get the current user's cart list, creating it if it doesn't exist
        private List<CartItem> GetCurrentCart()
        {
            int userId = _currentUserService.CurrentUser?.UserID ?? -1;
            if (!_userCarts.ContainsKey(userId))
            {
                _userCarts[userId] = new List<CartItem>();
            }
            return _userCarts[userId];
        }

        public ReadOnlyCollection<CartItem> GetItems()
        {
            return GetCurrentCart().AsReadOnly();
        }

        public int ItemCount => GetCurrentCart().Sum(i => i.Quantity);

        public decimal SubTotal => GetCurrentCart().Sum(i => i.TotalPrice);

        public decimal DeliveryCost
        {
            get
            {
                if (SubTotal >= _settingsService.GetMinimumOrderAmountForFreeDelivery())
                {
                    return 0;
                }
                return _settingsService.GetDeliveryCost();
            }
        }

        public async Task<(bool IsEligible, decimal DiscountPercentage)> CheckOrderDiscountEligibilityAsync()
        {
            decimal discountPercentage = 0;
            bool isEligible = false;

            // Check if order exceeds threshold amount
            if (SubTotal >= _settingsService.GetDiscountThresholdAmount())
            {
                isEligible = true;
                discountPercentage = _settingsService.GetDiscountPercentage();
            }

            // Check loyalty discount (multiple orders in a time period)
            if (_currentUserService.CurrentUser != null)
            {
                int loyaltyTimePeriod = _settingsService.GetLoyaltyTimePeriodDays();
                int loyaltyOrderCount = _settingsService.GetLoyaltyOrderCount();
                decimal loyaltyDiscountPercentage = _settingsService.GetLoyaltyDiscountPercentage();

                DateTime cutoffDate = DateTime.Now.AddDays(-loyaltyTimePeriod);
                var recentOrders = await _orderService.GetOrdersByUserIdSinceDateAsync(
                    _currentUserService.CurrentUser.UserID, cutoffDate);

                if (recentOrders.Count >= loyaltyOrderCount)
                {
                    // Use the higher discount percentage
                    if (loyaltyDiscountPercentage > discountPercentage)
                    {
                        isEligible = true;
                        discountPercentage = loyaltyDiscountPercentage;
                    }
                }
            }

            return (isEligible, discountPercentage);
        }

        public decimal CalculateDiscountValue(decimal discountPercentage)
        {
            return Math.Round(SubTotal * (discountPercentage / 100), 2);
        }

        public decimal TotalWithDiscount(decimal discountValue)
        {
            return SubTotal - discountValue + DeliveryCost;
        }

        public void AddDish(Dish dish, int quantity = 1)
        {
            // Only logged in users can add items to cart
            if (_currentUserService.CurrentUser == null || _currentUserService.CurrentUser.UserType == "Guest")
                return;
                
            if (dish == null || quantity <= 0 || !dish.IsAvailable)
                return;

            var currentCart = GetCurrentCart();
            var existingItem = currentCart.FirstOrDefault(i => 
                i.Type == CartItem.ItemType.Dish && i.Id == dish.DishID);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                currentCart.Add(new CartItem
                {
                    Id = dish.DishID,
                    Type = CartItem.ItemType.Dish,
                    Name = dish.Name,
                    CategoryName = dish.Category?.Name ?? "Necategorisit",
                    Price = dish.Price,
                    Quantity = quantity,
                    Dish = dish
                });
            }

            NotifyCartChanged();
        }

        public void AddMenu(Menu menu, int quantity = 1)
        {
            // Only logged in users can add items to cart
            if (_currentUserService.CurrentUser == null || _currentUserService.CurrentUser.UserType == "Guest")
                return;
                
            if (menu == null || quantity <= 0)
                return;
                
            // Check if all components are available
            bool allComponentsAvailable = menu.Components?.All(c => c.Dish.IsAvailable) ?? true;
            if (!allComponentsAvailable)
                return;

            var currentCart = GetCurrentCart();
            var existingItem = currentCart.FirstOrDefault(i => 
                i.Type == CartItem.ItemType.Menu && i.Id == menu.MenuID);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                currentCart.Add(new CartItem
                {
                    Id = menu.MenuID,
                    Type = CartItem.ItemType.Menu,
                    Name = menu.Name,
                    CategoryName = menu.Category?.Name ?? "Necategorisit",
                    Price = menu.Price,
                    Quantity = quantity,
                    Menu = menu
                });
            }

            NotifyCartChanged();
        }

        public async Task<bool> AddDish(int dishId, int quantity = 1)
        {
            // Only logged in users can add items to cart
            if (_currentUserService.CurrentUser == null || _currentUserService.CurrentUser.UserType == "Guest")
                return false;
                
            if (quantity <= 0)
                return false;
                
            try
            {
                var dish = await _menuService.GetDishByIdAsync(dishId);
                if (dish == null || !dish.IsAvailable)
                    return false;
                    
                AddDish(dish, quantity);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding dish to cart: {ex.Message}");
                return false;
            }
        }
        
        public async Task<bool> AddMenu(int menuId, int quantity = 1)
        {
            // Only logged in users can add items to cart
            if (_currentUserService.CurrentUser == null || _currentUserService.CurrentUser.UserType == "Guest")
                return false;
                
            if (quantity <= 0)
                return false;
                
            try
            {
                var menu = await _menuService.GetMenuByIdAsync(menuId);
                if (menu == null || !menu.IsAvailable)
                    return false;
                    
                AddMenu(menu, quantity);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding menu to cart: {ex.Message}");
                return false;
            }
        }

        public void UpdateItemQuantity(CartItem.ItemType type, int id, int quantity)
        {
            if (_currentUserService.CurrentUser == null || _currentUserService.CurrentUser.UserType == "Guest")
                return;
                
            var currentCart = GetCurrentCart();
            var item = currentCart.FirstOrDefault(i => i.Type == type && i.Id == id);
            if (item != null)
            {
                if (quantity <= 0)
                {
                    currentCart.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
                
                NotifyCartChanged();
            }
        }

        public void RemoveItem(CartItem.ItemType type, int id)
        {
            if (_currentUserService.CurrentUser == null || _currentUserService.CurrentUser.UserType == "Guest")
                return;
                
            var currentCart = GetCurrentCart();
            var item = currentCart.FirstOrDefault(i => i.Type == type && i.Id == id);
            if (item != null)
            {
                currentCart.Remove(item);
                NotifyCartChanged();
            }
        }

        public void ClearCart()
        {
            if (_currentUserService.CurrentUser == null || _currentUserService.CurrentUser.UserType == "Guest")
                return;
                
            GetCurrentCart().Clear();
            NotifyCartChanged();
        }

        private void NotifyCartChanged()
        {
            CartChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task<Order> PlaceOrderAsync()
        {
            if (_currentUserService.CurrentUser == null || _currentUserService.CurrentUser.UserType == "Guest")
            {
                throw new InvalidOperationException("Trebuie să fiți autentificat pentru a plasa o comandă");
            }

            var currentCart = GetCurrentCart();
            if (currentCart.Count == 0)
            {
                throw new InvalidOperationException("Nu se poate plasa o comandă goală");
            }

            // Check order discount eligibility
            var (isDiscountEligible, discountPercentage) = await CheckOrderDiscountEligibilityAsync();
            decimal discountValue = isDiscountEligible ? CalculateDiscountValue(discountPercentage) : 0;

            // Create new order
            var order = new Order
            {
                UserID = _currentUserService.CurrentUser.UserID,
                Status = "inregistrata",
                OrderDate = DateTime.Now,
                EstimatedDeliveryTime = DateTime.Now.AddHours(1),
                FoodCost = SubTotal,
                DeliveryCost = DeliveryCost,
                DiscountValue = discountValue,
                TotalCost = TotalWithDiscount(discountValue),
                OrderCode = $"CMD-{DateTime.Now.ToString("yyMMddHHmm")}-{new Random().Next(1000, 9999)}",
                OrderDishes = new List<OrderDish>(),
                OrderMenus = new List<OrderMenu>()
            };

            // Add dishes
            foreach (var item in currentCart.Where(i => i.Type == CartItem.ItemType.Dish))
            {
                order.OrderDishes.Add(new OrderDish
                {
                    DishID = item.Id,
                    Quantity = item.Quantity
                });
            }

            // Add menus
            foreach (var item in currentCart.Where(i => i.Type == CartItem.ItemType.Menu))
            {
                order.OrderMenus.Add(new OrderMenu
                {
                    MenuID = item.Id,
                    Quantity = item.Quantity
                });
            }

            // Save order to database
            var createdOrder = await _orderService.CreateOrderAsync(order);

            // Clear cart after successful order
            ClearCart();

            return createdOrder;
        }
    }
} 