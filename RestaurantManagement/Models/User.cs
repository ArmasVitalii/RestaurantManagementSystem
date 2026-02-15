namespace RestaurantManagement.Models;

public class User
{
    public int UserID { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string DeliveryAddress { get; set; }
    public string PasswordHash { get; set; }
    public string UserType { get; set; } // 'client' sau 'employee'

    public ICollection<Order> Orders { get; set; }
}

