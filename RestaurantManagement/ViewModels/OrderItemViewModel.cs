using System;

namespace RestaurantManagement.ViewModels
{
    public class EmployeeOrderItemViewModel : BaseViewModel
    {
        public string ItemType { get; set; } = "";
        public string ItemName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
} 