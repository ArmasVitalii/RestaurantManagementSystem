using RestaurantManagement.Models;

namespace RestaurantManagement.ViewModels
{
    public class EmployeeCategoryViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }

        public EmployeeCategoryViewModel(Category category)
        {
            Id = category.CategoryID;
            CategoryName = category.Name;
        }

        public EmployeeCategoryViewModel()
        {
            Id = 0;
            CategoryName = "";
        }
    }
} 