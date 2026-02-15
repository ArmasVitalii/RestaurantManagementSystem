namespace RestaurantManagement.Models;

public class Dish
{
    public int DishID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int PortionQuantityGrams { get; set; }
    public int TotalQuantityGrams { get; set; }
    public int CategoryID { get; set; }
    public bool IsAvailable { get; set; }

    public Category Category { get; set; }
    public ICollection<DishImage> Images { get; set; }
    public ICollection<DishAllergen> DishAllergens { get; set; }
    public ICollection<MenuComponent> MenuComponents { get; set; }
    public ICollection<OrderDish> OrderDishes { get; set; }
}

