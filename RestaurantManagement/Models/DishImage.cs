namespace RestaurantManagement.Models;

public class DishImage
{
    public int ImageID { get; set; }
    public int DishID { get; set; }
    public string ImageUrl { get; set; }

    public Dish Dish { get; set; }
}

