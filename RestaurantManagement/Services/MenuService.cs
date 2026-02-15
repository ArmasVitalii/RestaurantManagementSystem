using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;

namespace RestaurantManagement.Services;

public class MenuService
{
    private readonly RestaurantDbContext _dbContext;
    private readonly StoredProcedureService _storedProcedureService;

    public MenuService()
    {
        _dbContext = new RestaurantDbContext();
        _storedProcedureService = new StoredProcedureService();
    }

    public async Task<List<Category>> GetCategoriesWithItemsAsync()
    {
        return await _dbContext.Categories
            .Include(c => c.Dishes)
                .ThenInclude(d => d.Images)
            .Include(c => c.Dishes)
                .ThenInclude(d => d.DishAllergens)
                    .ThenInclude(da => da.Allergen)
            .Include(c => c.Menus)
                .ThenInclude(m => m.Components)
                    .ThenInclude(mc => mc.Dish)
                        .ThenInclude(d => d.Images)
            .Include(c => c.Menus)
                .ThenInclude(m => m.Components)
                    .ThenInclude(mc => mc.Dish)
                        .ThenInclude(d => d.DishAllergens)
                            .ThenInclude(da => da.Allergen)
            .ToListAsync();
    }

    public async Task<List<Dish>> SearchDishesByNameAsync(string keyword, bool exactMatch = false)
    {
        IQueryable<Dish> query = _dbContext.Dishes
            .Include(d => d.Category)
            .Include(d => d.Images)
            .Include(d => d.DishAllergens)
                .ThenInclude(da => da.Allergen);

        if (exactMatch)
        {
            query = query.Where(d => d.Name.Equals(keyword));
        }
        else
        {
            query = query.Where(d => d.Name.Contains(keyword));
        }

        return await query.ToListAsync();
    }

    public async Task<List<Menu>> SearchMenusByNameAsync(string keyword, bool exactMatch = false)
    {
        IQueryable<Menu> query = _dbContext.Menus
            .Include(m => m.Category)
            .Include(m => m.Components)
                .ThenInclude(mc => mc.Dish)
                    .ThenInclude(d => d.Images)
            .Include(m => m.Components)
                .ThenInclude(mc => mc.Dish)
                    .ThenInclude(d => d.DishAllergens)
                        .ThenInclude(da => da.Allergen);

        if (exactMatch)
        {
            query = query.Where(m => m.Name.Equals(keyword));
        }
        else
        {
            query = query.Where(m => m.Name.Contains(keyword));
        }

        return await query.ToListAsync();
    }

    public async Task<(List<Dish>, List<Menu>)> SearchByAllergenAsync(string allergenName, bool exclude)
    {
        var allergen = await _dbContext.Allergens
            .FirstOrDefaultAsync(a => a.Name.Contains(allergenName));

        if (allergen == null)
        {
            return (new List<Dish>(), new List<Menu>());
        }

        // Get dishes with/without the allergen
        var dishesWithAllergenIds = await _dbContext.DishAllergens
            .Where(da => da.AllergenID == allergen.AllergenID)
            .Select(da => da.DishID)
            .ToListAsync();

        IQueryable<Dish> dishesQuery = _dbContext.Dishes
            .Include(d => d.Category)
            .Include(d => d.Images)
            .Include(d => d.DishAllergens)
                .ThenInclude(da => da.Allergen);

        if (exclude)
        {
            // Dishes WITHOUT the allergen
            dishesQuery = dishesQuery.Where(d => !dishesWithAllergenIds.Contains(d.DishID));
        }
        else
        {
            // Dishes WITH the allergen
            dishesQuery = dishesQuery.Where(d => dishesWithAllergenIds.Contains(d.DishID));
        }

        var dishes = await dishesQuery.ToListAsync();

        // Get menus with/without dishes containing the allergen
        var dishIds = await _dbContext.MenuComponents
            .Select(mc => mc.DishID)
            .ToListAsync();

        var dishesFetched = await _dbContext.Dishes
            .Where(d => dishIds.Contains(d.DishID))
            .Include(d => d.DishAllergens)
            .ToListAsync();

        var menuComponents = await _dbContext.MenuComponents
            .Include(mc => mc.Menu)
                .ThenInclude(m => m.Category)
            .ToListAsync();

        var dishAllergens = await _dbContext.DishAllergens
            .Where(da => da.AllergenID == allergen.AllergenID)
            .ToListAsync();

        var dishesWithAllergen = dishesFetched
            .Where(d => d.DishAllergens.Any(da => da.AllergenID == allergen.AllergenID))
            .Select(d => d.DishID)
            .ToList();

        var menusWithAllergen = menuComponents
            .Where(mc => dishesWithAllergen.Contains(mc.DishID))
            .Select(mc => mc.MenuID)
            .Distinct()
            .ToList();

        IQueryable<Menu> menusQuery = _dbContext.Menus
            .Include(m => m.Category)
            .Include(m => m.Components)
                .ThenInclude(mc => mc.Dish)
                    .ThenInclude(d => d.Images)
            .Include(m => m.Components)
                .ThenInclude(mc => mc.Dish)
                    .ThenInclude(d => d.DishAllergens)
                        .ThenInclude(da => da.Allergen);

        if (exclude)
        {
            // Menus WITHOUT the allergen
            menusQuery = menusQuery.Where(m => !menusWithAllergen.Contains(m.MenuID));
        }
        else
        {
            // Menus WITH the allergen
            menusQuery = menusQuery.Where(m => menusWithAllergen.Contains(m.MenuID));
        }

        var menus = await menusQuery.ToListAsync();

        return (dishes, menus);
    }

    public async Task<Dish> GetDishByIdAsync(int dishId)
    {
        return await _dbContext.Dishes
            .Include(d => d.Category)
            .Include(d => d.Images)
            .Include(d => d.DishAllergens)
                .ThenInclude(da => da.Allergen)
            .FirstOrDefaultAsync(d => d.DishID == dishId);
    }

    public async Task<Menu> GetMenuByIdAsync(int menuId)
    {
        return await _dbContext.Menus
            .Include(m => m.Category)
            .Include(m => m.Components)
                .ThenInclude(mc => mc.Dish)
                    .ThenInclude(d => d.Images)
            .Include(m => m.Components)
                .ThenInclude(mc => mc.Dish)
                    .ThenInclude(d => d.DishAllergens)
                        .ThenInclude(da => da.Allergen)
            .FirstOrDefaultAsync(m => m.MenuID == menuId);
    }

    /// <summary>
    /// Actualizează stocul unui preparat și disponibilitatea acestuia folosind procedura stocată
    /// </summary>
    public async Task<bool> UpdateDishStockAsync(int dishId, int newTotalQuantity, bool? isAvailable = null)
    {
        try
        {
            var result = await _storedProcedureService.UpdateDishStockAsync(dishId, newTotalQuantity, isAvailable);
            return result.Rows.Count > 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating dish stock: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Verifică disponibilitatea unui preparat
    /// </summary>
    public async Task<bool> CheckDishAvailabilityAsync(int dishId)
    {
        var dish = await _dbContext.Dishes.FindAsync(dishId);
        return dish?.IsAvailable ?? false;
    }
    
    /// <summary>
    /// Verifică disponibilitatea unui meniu (toate preparatele componente sunt disponibile)
    /// </summary>
    public async Task<bool> CheckMenuAvailabilityAsync(int menuId)
    {
        var menu = await _dbContext.Menus
            .Include(m => m.Components)
                .ThenInclude(mc => mc.Dish)
            .FirstOrDefaultAsync(m => m.MenuID == menuId);
            
        return menu?.IsAvailable == true && 
               menu.Components?.All(mc => mc.Dish?.IsAvailable == true) == true;
    }
} 