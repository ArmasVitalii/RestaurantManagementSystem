namespace RestaurantManagement.Models;

public class Category
{
    public int CategoryID { get; set; }
    public string Name { get; set; }
    public ICollection<Dish> Dishes { get; set; }
    public ICollection<Menu> Menus { get; set; }
}

