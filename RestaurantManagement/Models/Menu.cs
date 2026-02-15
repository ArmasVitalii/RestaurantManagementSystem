namespace RestaurantManagement.Models;

public class Menu
{
    public int MenuID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int CategoryID { get; set; }
    public decimal DiscountPercent { get; set; }
    public bool IsAvailable { get; set; }

    public Category Category { get; set; }
    public ICollection<MenuComponent> Components { get; set; }
    public ICollection<OrderMenu> OrderMenus { get; set; }
}

