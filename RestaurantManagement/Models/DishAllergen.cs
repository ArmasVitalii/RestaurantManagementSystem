namespace RestaurantManagement.Models;

public class DishAllergen
{
    public int DishID { get; set; }
    public int AllergenID { get; set; }

    public Dish Dish { get; set; }
    public Allergen Allergen { get; set; }
}
