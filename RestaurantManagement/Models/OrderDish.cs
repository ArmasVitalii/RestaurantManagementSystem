namespace RestaurantManagement.Models;

public class OrderDish
{
    public int OrderID { get; set; }
    public int DishID { get; set; }
    public int Quantity { get; set; }

    public Order Order { get; set; }
    public Dish Dish { get; set; }
}

