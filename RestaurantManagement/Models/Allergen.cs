namespace RestaurantManagement.Models;

public class Allergen
{
    public int AllergenID { get; set; }
    public string Name { get; set; }

    public ICollection<DishAllergen> DishAllergens { get; set; }
}

