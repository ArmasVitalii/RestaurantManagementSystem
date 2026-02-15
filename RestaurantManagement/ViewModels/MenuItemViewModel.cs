using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using RestaurantManagement.Models;

namespace RestaurantManagement.ViewModels;

public class MenuItemViewModel : BaseViewModel
{
    public enum MenuItemType
    {
        Dish,
        Menu
    }

    public MenuItemType Type { get; }
    public int Id { get; }
    public string Name { get; }
    public string CategoryName { get; } 
    public string Description { get; }
    public decimal Price { get; }
    public int PortionSize { get; }
    public string PortionUnit { get; }
    public bool IsAvailable { get; }
    public ObservableCollection<BitmapImage> Images { get; }
    public string AllergensList { get; }

    // Constructor for Dish
    public MenuItemViewModel(Dish dish)
    {
        Type = MenuItemType.Dish;
        Id = dish.DishID;
        Name = dish.Name;
        CategoryName = dish.Category?.Name ?? "Uncategorized";
        Description = dish.Description;
        Price = dish.Price;
        PortionSize = dish.PortionQuantityGrams;
        PortionUnit = "g";
        IsAvailable = dish.IsAvailable;
        
        // Load images
        Images = new ObservableCollection<BitmapImage>();
        if (dish.Images != null)
        {
            foreach (var image in dish.Images)
            {
                if (!string.IsNullOrEmpty(image.ImageUrl))
                {
                    try
                    {
                        // Fix path handling by determining if it's a relative or absolute path
                        var imageUri = GetImageUri(image.ImageUrl);
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.UriSource = imageUri;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // Loads the image immediately and releases the file
                        bitmapImage.EndInit();
                        bitmapImage.Freeze(); // Makes the image usable across threads
                        Images.Add(bitmapImage);
                    }
                    catch (Exception ex)
                    {
                        // Log error to help diagnose the issue
                        System.Diagnostics.Debug.WriteLine($"Failed to load image: {image.ImageUrl}. Error: {ex.Message}");
                    }
                }
            }
        }
        
        // Create allergens list
        var allergens = dish.DishAllergens?.Select(da => da.Allergen?.Name)
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList() ?? new List<string>();
        
        AllergensList = allergens.Any() ? 
            "Alergeni: " + string.Join(", ", allergens) : 
            "Fără alergeni";
    }
    
    // Constructor for Menu
    public MenuItemViewModel(Menu menu)
    {
        Type = MenuItemType.Menu;
        Id = menu.MenuID;
        Name = menu.Name;
        CategoryName = menu.Category?.Name ?? "Uncategorized";
        Description = menu.Description;
        Price = menu.Price;
        
        // For menus, we don't have a direct portion size, so we'll use 0
        PortionSize = 0;
        PortionUnit = "";
        
        // A menu is available if all its components (dishes) are available
        IsAvailable = menu.Components?.All(mc => mc.Dish.IsAvailable) ?? true;
        
        // Load images from the first component dish
        Images = new ObservableCollection<BitmapImage>();
        var firstDish = menu.Components?.FirstOrDefault()?.Dish;
        if (firstDish?.Images != null)
        {
            foreach (var image in firstDish.Images)
            {
                if (!string.IsNullOrEmpty(image.ImageUrl))
                {
                    try
                    {
                        // Use the same helper method for consistency
                        var imageUri = GetImageUri(image.ImageUrl);
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.UriSource = imageUri;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();
                        Images.Add(bitmapImage);
                    }
                    catch (Exception ex)
                    {
                        // Log error to help diagnose the issue
                        System.Diagnostics.Debug.WriteLine($"Failed to load menu image: {image.ImageUrl}. Error: {ex.Message}");
                    }
                }
            }
        }
        
        // Create allergens list from all component dishes
        var allergens = new HashSet<string>();
        if (menu.Components != null)
        {
            foreach (var component in menu.Components)
            {
                var dishAllergens = component.Dish.DishAllergens?
                    .Select(da => da.Allergen?.Name)
                    .Where(name => !string.IsNullOrEmpty(name));
                
                if (dishAllergens != null)
                {
                    foreach (var allergen in dishAllergens)
                    {
                        allergens.Add(allergen);
                    }
                }
            }
        }
        
        AllergensList = allergens.Any() ? 
            "Alergeni: " + string.Join(", ", allergens.OrderBy(a => a)) : 
            "Fără alergeni";
    }
    
    // Helper method to handle different image path formats
    private Uri GetImageUri(string imagePath)
    {
        // If the path is already a URI scheme (pack, http, etc.)
        if (imagePath.Contains("://"))
        {
            return new Uri(imagePath);
        }
        
        // If it's a packaged application path using pack syntax
        if (imagePath.StartsWith("pack:"))
        {
            return new Uri(imagePath);
        }
        
        // If it's a relative path like "Images\image.png"
        if (imagePath.Contains("\\") || imagePath.Contains("/"))
        {
            // Convert to application-relative path
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            string fullPath = System.IO.Path.Combine(appPath, imagePath.Replace("\\", System.IO.Path.DirectorySeparatorChar.ToString()));
            
            // Check if file exists, if not try to look in the project directory
            if (!System.IO.File.Exists(fullPath))
            {
                // Try to find the file in the parent directories (useful during development)
                string projectDir = appPath;
                while (!System.IO.Directory.Exists(System.IO.Path.Combine(projectDir, "Images")) && 
                       System.IO.Directory.GetParent(projectDir) != null)
                {
                    projectDir = System.IO.Directory.GetParent(projectDir).FullName;
                }
                
                if (System.IO.Directory.Exists(System.IO.Path.Combine(projectDir, "Images")))
                {
                    fullPath = System.IO.Path.Combine(projectDir, imagePath.Replace("\\", System.IO.Path.DirectorySeparatorChar.ToString()));
                }
            }
            
            return new Uri(fullPath);
        }
        
        // If it's just a file name, assume it's in the Images folder
        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        string combinedPath = System.IO.Path.Combine(basePath, "Images", imagePath);
        
        // If file doesn't exist in the output directory, try to find it in the project directory
        if (!System.IO.File.Exists(combinedPath))
        {
            string projectDir = basePath;
            while (!System.IO.Directory.Exists(System.IO.Path.Combine(projectDir, "Images")) && 
                   System.IO.Directory.GetParent(projectDir) != null)
            {
                projectDir = System.IO.Directory.GetParent(projectDir).FullName;
            }
            
            if (System.IO.Directory.Exists(System.IO.Path.Combine(projectDir, "Images")))
            {
                combinedPath = System.IO.Path.Combine(projectDir, "Images", imagePath);
            }
        }
        
        return new Uri(combinedPath);
    }
} 