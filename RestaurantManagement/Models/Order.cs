namespace RestaurantManagement.Models;

public class Order
{
    public int OrderID { get; set; }
    public int UserID { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; }
    public DateTime? EstimatedDeliveryTime { get; set; }
    public decimal FoodCost { get; set; }
    public decimal DeliveryCost { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal TotalCost { get; set; }
    public string OrderCode { get; set; }

    public User User { get; set; }
    public ICollection<OrderDish> OrderDishes { get; set; }
    public ICollection<OrderMenu> OrderMenus { get; set; }
}

