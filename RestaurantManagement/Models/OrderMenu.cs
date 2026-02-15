namespace RestaurantManagement.Models;

public class OrderMenu
{
    public int OrderID { get; set; }
    public int MenuID { get; set; }
    public int Quantity { get; set; }

    public Order Order { get; set; }
    public Menu Menu { get; set; }
}

