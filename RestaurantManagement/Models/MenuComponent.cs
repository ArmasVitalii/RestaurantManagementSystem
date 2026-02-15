namespace RestaurantManagement.Models;

public class MenuComponent
{
    public int MenuID { get; set; }
    public int DishID { get; set; }
    public int CustomPortionQuantityGrams { get; set; }

    public Menu Menu { get; set; }
    public Dish Dish { get; set; }
}

