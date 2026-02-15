using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;

namespace RestaurantManagement.Services;

public class OrderService
{
    private readonly RestaurantDbContext _dbContext;

    public OrderService()
    {
        _dbContext = new RestaurantDbContext();
    }

    // Customer Methods
    
    public async Task<List<Order>> GetOrdersByUserIdAsync(int userId)
    {
        return await _dbContext.Orders
            .Include(o => o.OrderDishes)
                .ThenInclude(od => od.Dish)
            .Include(o => o.OrderMenus)
                .ThenInclude(om => om.Menu)
                    .ThenInclude(m => m.Components)
                        .ThenInclude(mc => mc.Dish)
            .Where(o => o.UserID == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }
    
    public async Task<List<Order>> GetActiveOrdersByUserIdAsync(int userId)
    {
        return await _dbContext.Orders
            .Include(o => o.OrderDishes)
                .ThenInclude(od => od.Dish)
            .Include(o => o.OrderMenus)
                .ThenInclude(om => om.Menu)
                    .ThenInclude(m => m.Components)
                        .ThenInclude(mc => mc.Dish)
            .Where(o => o.UserID == userId && 
                        o.Status != "livrata" && 
                        o.Status != "anulata")
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }
    
    public async Task<Order> CreateOrderAsync(Order order)
    {
        // Check if all dishes are available
        foreach (var orderDish in order.OrderDishes)
        {
            var dish = await _dbContext.Dishes.FindAsync(orderDish.DishID);
            if (dish == null || !dish.IsAvailable)
            {
                throw new InvalidOperationException($"Dish with ID {orderDish.DishID} is not available");
            }
        }
        
        // Check if all menus are available
        foreach (var orderMenu in order.OrderMenus)
        {
            var menu = await _dbContext.Menus
                .Include(m => m.Components)
                    .ThenInclude(mc => mc.Dish)
                .FirstOrDefaultAsync(m => m.MenuID == orderMenu.MenuID);
                
            if (menu == null || !menu.IsAvailable || menu.Components.Any(mc => !mc.Dish.IsAvailable))
            {
                throw new InvalidOperationException($"Menu with ID {orderMenu.MenuID} is not available");
            }
        }
        
        // Set default values if not already set
        if (order.OrderDate == default)
        {
            order.OrderDate = DateTime.Now;
        }
        
        if (string.IsNullOrEmpty(order.Status))
        {
            order.Status = "inregistrata";
        }
        
        if (order.EstimatedDeliveryTime == null)
        {
            order.EstimatedDeliveryTime = DateTime.Now.AddHours(1); // Default 1 hour delivery time
        }
        
        // Generate unique order code if not provided
        if (string.IsNullOrEmpty(order.OrderCode))
        {
            order.OrderCode = $"CMD-{DateTime.Now.ToString("yyMMddHHmm")}-{new Random().Next(1000, 9999)}";
        }
        
        // Calculate order costs if not set
        if (order.FoodCost == 0)
        {
            decimal foodCost = 0;
            
            // Add dish costs
            foreach (var orderDish in order.OrderDishes)
            {
                var dish = await _dbContext.Dishes.FindAsync(orderDish.DishID);
                foodCost += dish.Price * orderDish.Quantity;
            }
            
            // Add menu costs
            foreach (var orderMenu in order.OrderMenus)
            {
                var menu = await _dbContext.Menus.FindAsync(orderMenu.MenuID);
                foodCost += menu.Price * orderMenu.Quantity;
            }
            
            order.FoodCost = foodCost;
        }
        
        // Get settings for delivery cost calculation
        var settingsService = new SettingsService();
        
        // Apply delivery costs based on settings
        if (order.DeliveryCost == 0)
        {
            decimal minimumOrderForFreeDelivery = settingsService.GetMinimumOrderAmountForFreeDelivery();
            decimal deliveryCost = settingsService.GetDeliveryCost();
            
            order.DeliveryCost = order.FoodCost >= minimumOrderForFreeDelivery ? 0 : deliveryCost;
        }
        
        // Update total cost
        order.TotalCost = order.FoodCost + order.DeliveryCost - order.DiscountValue;
        
        // Save order
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();
        
        // After the order has been saved and received an ID, update dish quantities
        if (order.Status == "in_preparare" || order.Status == "inregistrata")
        {
            // Update dish quantities for dishes ordered directly
            foreach (var orderDish in order.OrderDishes)
            {
                var dish = await _dbContext.Dishes.FindAsync(orderDish.DishID);
                if (dish != null)
                {
                    dish.TotalQuantityGrams -= dish.PortionQuantityGrams * orderDish.Quantity;
                    
                    // Mark as unavailable if below threshold
                    if (dish.TotalQuantityGrams < dish.PortionQuantityGrams)
                    {
                        dish.IsAvailable = false;
                    }
                    
                    _dbContext.Dishes.Update(dish);
                }
            }
            
            // Update dish quantities for dishes in menus
            foreach (var orderMenu in order.OrderMenus)
            {
                var menu = await _dbContext.Menus
                    .Include(m => m.Components)
                        .ThenInclude(mc => mc.Dish)
                    .FirstOrDefaultAsync(m => m.MenuID == orderMenu.MenuID);
                    
                if (menu != null)
                {
                    foreach (var component in menu.Components)
                    {
                        var dish = component.Dish;
                        if (dish != null)
                        {
                            // Each menu ordered consumes one portion of each component dish
                            dish.TotalQuantityGrams -= dish.PortionQuantityGrams * orderMenu.Quantity;
                            
                            // Mark as unavailable if below threshold
                            if (dish.TotalQuantityGrams < dish.PortionQuantityGrams)
                            {
                                dish.IsAvailable = false;
                            }
                            
                            _dbContext.Dishes.Update(dish);
                        }
                    }
                }
            }
            
            await _dbContext.SaveChangesAsync();
        }
        
        return order;
    }
    
    public async Task<bool> CancelOrderAsync(int orderId, int userId)
    {
        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(o => o.OrderID == orderId && o.UserID == userId);
            
        if (order == null || order.Status == "livrata" || order.Status == "anulata")
        {
            return false;
        }
        
        order.Status = "anulata";
        
        // Return quantities to inventory (only for orders that were in preparation)
        if (order.Status == "in_preparare")
        {
            var orderDishes = await _dbContext.OrderDishes
                .Include(od => od.Dish)
                .Where(od => od.OrderID == orderId)
                .ToListAsync();
                
            foreach (var orderDish in orderDishes)
            {
                orderDish.Dish.TotalQuantityGrams += orderDish.Dish.PortionQuantityGrams * orderDish.Quantity;
                
                // Mark as available again if it has enough quantity
                if (orderDish.Dish.TotalQuantityGrams >= orderDish.Dish.PortionQuantityGrams)
                {
                    orderDish.Dish.IsAvailable = true;
                }
                
                _dbContext.Dishes.Update(orderDish.Dish);
            }
        }
        
        _dbContext.Orders.Update(order);
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
    
    // Employee Methods
    
    public async Task<List<Order>> GetAllOrdersAsync()
    {
        return await _dbContext.Orders
            .Include(o => o.User)
            .Include(o => o.OrderDishes)
                .ThenInclude(od => od.Dish)
            .Include(o => o.OrderMenus)
                .ThenInclude(om => om.Menu)
                    .ThenInclude(m => m.Components)
                        .ThenInclude(mc => mc.Dish)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }
    
    public async Task<List<Order>> GetAllActiveOrdersAsync()
    {
        return await _dbContext.Orders
            .Include(o => o.User)
            .Include(o => o.OrderDishes)
                .ThenInclude(od => od.Dish)
            .Include(o => o.OrderMenus)
                .ThenInclude(om => om.Menu)
                    .ThenInclude(m => m.Components)
                        .ThenInclude(mc => mc.Dish)
            .Where(o => o.Status != "livrata" && o.Status != "anulata")
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }
    
    public async Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus)
    {
        var validStatuses = new[] { "inregistrata", "in_preparare", "in_livrare", "livrata", "anulata" };
        
        if (!validStatuses.Contains(newStatus))
        {
            return false;
        }
        
        var order = await _dbContext.Orders.FindAsync(orderId);
        
        if (order == null || order.Status == "livrata" || order.Status == "anulata")
        {
            return false;
        }
        
        // Handle cancellation
        if (newStatus == "anulata")
        {
            return await CancelOrderAsync(orderId, order.UserID);
        }
        
        // Update delivery estimation based on status
        if (newStatus == "in_livrare")
        {
            // If order is now being delivered, update estimated time to 30 min from now
            order.EstimatedDeliveryTime = DateTime.Now.AddMinutes(30);
        }
        
        order.Status = newStatus;
        _dbContext.Orders.Update(order);
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<List<Dish>> GetLowStockDishesAsync(int thresholdQuantity)
    {
        return await _dbContext.Dishes
            .Where(d => d.TotalQuantityGrams <= thresholdQuantity)
            .OrderBy(d => d.TotalQuantityGrams)
            .ToListAsync();
    }
    
    public async Task<List<Category>> GetCategoriesWithItemsAsync()
    {
        return await _dbContext.Categories
            .Include(c => c.Dishes)
            .Include(c => c.Menus)
                .ThenInclude(m => m.Components)
                    .ThenInclude(mc => mc.Dish)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
    
    public async Task<List<Allergen>> GetAllergensAsync()
    {
        return await _dbContext.Allergens
            .OrderBy(a => a.Name)
            .ToListAsync();
    }
    
    // Category, Menu, Dish, Allergen Management
    
    public async Task<Category> AddCategoryAsync(Category category)
    {
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();
        return category;
    }
    
    public async Task<bool> UpdateCategoryAsync(Category category)
    {
        System.Diagnostics.Debug.WriteLine($"OrderService: UpdateCategoryAsync called with ID={category.CategoryID}, Name={category.Name}");
        
        try
        {
            var existingCategory = await _dbContext.Categories.FindAsync(category.CategoryID);
            if (existingCategory == null)
            {
                System.Diagnostics.Debug.WriteLine($"OrderService: Category with ID {category.CategoryID} not found");
                return false;
            }
            
            System.Diagnostics.Debug.WriteLine($"OrderService: Found existing category: ID={existingCategory.CategoryID}, Old Name={existingCategory.Name}, New Name={category.Name}");
            existingCategory.Name = category.Name;
            
            _dbContext.Categories.Update(existingCategory);
            int affected = await _dbContext.SaveChangesAsync();
            
            System.Diagnostics.Debug.WriteLine($"OrderService: SaveChangesAsync completed, affected rows: {affected}");
            return affected > 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OrderService: Error updating category: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"OrderService: Stack trace: {ex.StackTrace}");
            return false;
        }
    }
    
    public async Task<bool> DeleteCategoryAsync(int categoryId)
    {
        var category = await _dbContext.Categories.FindAsync(categoryId);
        if (category == null) return false;
        
        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync();
        return true;
    }
    
    public async Task<Dish> AddDishAsync(Dish dish)
    {
        _dbContext.Dishes.Add(dish);
        await _dbContext.SaveChangesAsync();
        return dish;
    }
    
    public async Task<bool> UpdateDishAsync(Dish dish)
    {
        var existingDish = await _dbContext.Dishes.FindAsync(dish.DishID);
        if (existingDish == null) return false;
        
        existingDish.Name = dish.Name;
        existingDish.Description = dish.Description;
        existingDish.Price = dish.Price;
        existingDish.PortionQuantityGrams = dish.PortionQuantityGrams;
        existingDish.TotalQuantityGrams = dish.TotalQuantityGrams;
        existingDish.CategoryID = dish.CategoryID;
        existingDish.IsAvailable = dish.IsAvailable;
        
        _dbContext.Dishes.Update(existingDish);
        await _dbContext.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> DeleteDishAsync(int dishId)
    {
        var dish = await _dbContext.Dishes.FindAsync(dishId);
        if (dish == null) return false;
        
        _dbContext.Dishes.Remove(dish);
        await _dbContext.SaveChangesAsync();
        return true;
    }
    
    public async Task<Menu> AddMenuAsync(Menu menu)
    {
        _dbContext.Menus.Add(menu);
        await _dbContext.SaveChangesAsync();
        return menu;
    }
    
    public async Task<bool> UpdateMenuAsync(Menu menu)
    {
        var existingMenu = await _dbContext.Menus.FindAsync(menu.MenuID);
        if (existingMenu == null) return false;
        
        existingMenu.Name = menu.Name;
        existingMenu.Description = menu.Description;
        existingMenu.Price = menu.Price;
        existingMenu.DiscountPercent = menu.DiscountPercent;
        existingMenu.CategoryID = menu.CategoryID;
        existingMenu.IsAvailable = menu.IsAvailable;
        
        _dbContext.Menus.Update(existingMenu);
        await _dbContext.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> DeleteMenuAsync(int menuId)
    {
        var menu = await _dbContext.Menus.FindAsync(menuId);
        if (menu == null) return false;
        
        _dbContext.Menus.Remove(menu);
        await _dbContext.SaveChangesAsync();
        return true;
    }
    
    public async Task<Allergen> AddAllergenAsync(Allergen allergen)
    {
        _dbContext.Allergens.Add(allergen);
        await _dbContext.SaveChangesAsync();
        return allergen;
    }
    
    public async Task<bool> UpdateAllergenAsync(Allergen allergen)
    {
        var existingAllergen = await _dbContext.Allergens.FindAsync(allergen.AllergenID);
        if (existingAllergen == null) return false;
        
        existingAllergen.Name = allergen.Name;
        
        _dbContext.Allergens.Update(existingAllergen);
        await _dbContext.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> DeleteAllergenAsync(int allergenId)
    {
        var allergen = await _dbContext.Allergens.FindAsync(allergenId);
        if (allergen == null) return false;
        
        _dbContext.Allergens.Remove(allergen);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<List<Order>> GetOrdersByUserIdSinceDateAsync(int userId, DateTime sinceDate)
    {
        return await _dbContext.Orders
            .Where(o => o.UserID == userId && o.OrderDate >= sinceDate && o.Status != "anulata")
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }
    
    /// <summary>
    /// Updates the stock quantity for a dish
    /// </summary>
    /// <param name="dishId">The ID of the dish to update</param>
    /// <param name="newStock">New total quantity in grams</param>
    /// <returns>True if the update was successful, false otherwise</returns>
    public async Task<bool> UpdateStockAsync(int dishId, int newStock)
    {
        System.Diagnostics.Debug.WriteLine($"OrderService: UpdateStockAsync called with DishID={dishId}, NewStock={newStock}");
        
        try
        {
            var dish = await _dbContext.Dishes.FindAsync(dishId);
            if (dish == null)
            {
                System.Diagnostics.Debug.WriteLine($"OrderService: Dish with ID {dishId} not found");
                return false;
            }
            
            System.Diagnostics.Debug.WriteLine($"OrderService: Found dish: ID={dish.DishID}, Name={dish.Name}, OldStock={dish.TotalQuantityGrams}, NewStock={newStock}");
            dish.TotalQuantityGrams = newStock;
            
            // Update availability based on if the dish has enough stock for at least one portion
            bool shouldBeAvailable = newStock >= dish.PortionQuantityGrams;
            System.Diagnostics.Debug.WriteLine($"OrderService: Dish availability should be {shouldBeAvailable} (stock={newStock} >= portion={dish.PortionQuantityGrams})");
            
            // Only change availability if needed to avoid unnecessary updates
            if (dish.IsAvailable != shouldBeAvailable)
            {
                dish.IsAvailable = shouldBeAvailable;
                System.Diagnostics.Debug.WriteLine($"OrderService: Dish availability updated to {dish.IsAvailable}");
            }
            
            _dbContext.Dishes.Update(dish);
            int affected = await _dbContext.SaveChangesAsync();
            
            System.Diagnostics.Debug.WriteLine($"OrderService: SaveChangesAsync completed, affected rows: {affected}");
            return affected > 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OrderService: Error updating stock: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"OrderService: Stack trace: {ex.StackTrace}");
            return false;
        }
    }
} 