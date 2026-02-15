using Microsoft.EntityFrameworkCore;

namespace RestaurantManagement.Models;

public class RestaurantDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDish> OrderDishes { get; set; }
    public DbSet<OrderMenu> OrderMenus { get; set; }
    public DbSet<Menu> Menus { get; set; }
    public DbSet<MenuComponent> MenuComponents { get; set; }
    public DbSet<Dish> Dishes { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Allergen> Allergens { get; set; }
    public DbSet<DishAllergen> DishAllergens { get; set; }
    public DbSet<DishImage> DishImages { get; set; }
    public DbSet<Setting> Settings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=localhost;Database=RestaurantManagement;User Id=admin;Password=1234;TrustServerCertificate=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Users
        modelBuilder.Entity<User>()
            .HasKey(u => u.UserID);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Categories
        modelBuilder.Entity<Category>()
            .HasKey(c => c.CategoryID);

        // Dishes
        modelBuilder.Entity<Dish>()
            .HasKey(d => d.DishID);

        modelBuilder.Entity<Dish>()
            .HasOne(d => d.Category)
            .WithMany(c => c.Dishes)
            .HasForeignKey(d => d.CategoryID);

        // DishImages
        modelBuilder.Entity<DishImage>()
            .HasKey(i => i.ImageID);

        modelBuilder.Entity<DishImage>()
            .HasOne(i => i.Dish)
            .WithMany(d => d.Images)
            .HasForeignKey(i => i.DishID);

        // Allergens
        modelBuilder.Entity<Allergen>()
            .HasKey(a => a.AllergenID);

        // DishAllergens (many-to-many)
        modelBuilder.Entity<DishAllergen>()
            .HasKey(da => new { da.DishID, da.AllergenID });

        modelBuilder.Entity<DishAllergen>()
            .HasOne(da => da.Dish)
            .WithMany(d => d.DishAllergens)
            .HasForeignKey(da => da.DishID);

        modelBuilder.Entity<DishAllergen>()
            .HasOne(da => da.Allergen)
            .WithMany(a => a.DishAllergens)
            .HasForeignKey(da => da.AllergenID);

        // Menus
        modelBuilder.Entity<Menu>()
            .HasKey(m => m.MenuID);

        modelBuilder.Entity<Menu>()
            .HasOne(m => m.Category)
            .WithMany(c => c.Menus)
            .HasForeignKey(m => m.CategoryID);

        // MenuComponents (many-to-many)
        modelBuilder.Entity<MenuComponent>()
            .HasKey(mc => new { mc.MenuID, mc.DishID });

        modelBuilder.Entity<MenuComponent>()
            .HasOne(mc => mc.Menu)
            .WithMany(m => m.Components)
            .HasForeignKey(mc => mc.MenuID)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MenuComponent>()
            .HasOne(mc => mc.Dish)
            .WithMany(d => d.MenuComponents)
            .HasForeignKey(mc => mc.DishID)
            .OnDelete(DeleteBehavior.NoAction);

        // Orders
        modelBuilder.Entity<Order>()
            .HasKey(o => o.OrderID);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserID);

        modelBuilder.Entity<Order>()
            .Property(o => o.OrderDate)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasDefaultValue("inregistrata");

        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasConversion<string>();

        // OrderDishes (many-to-many)
        modelBuilder.Entity<OrderDish>()
            .HasKey(od => new { od.OrderID, od.DishID });

        modelBuilder.Entity<OrderDish>()
            .HasOne(od => od.Order)
            .WithMany(o => o.OrderDishes)
            .HasForeignKey(od => od.OrderID);

        modelBuilder.Entity<OrderDish>()
            .HasOne(od => od.Dish)
            .WithMany(d => d.OrderDishes)
            .HasForeignKey(od => od.DishID);

        // OrderMenus (many-to-many)
        modelBuilder.Entity<OrderMenu>()
            .HasKey(om => new { om.OrderID, om.MenuID });

        modelBuilder.Entity<OrderMenu>()
            .HasOne(om => om.Order)
            .WithMany(o => o.OrderMenus)
            .HasForeignKey(om => om.OrderID);

        modelBuilder.Entity<OrderMenu>()
            .HasOne(om => om.Menu)
            .WithMany(m => m.OrderMenus)
            .HasForeignKey(om => om.MenuID);

        // Settings (key-value)
        modelBuilder.Entity<Setting>()
            .HasKey(s => s.SettingKey);

        modelBuilder.Entity<Setting>()
            .Property(s => s.SettingValue)
            .IsRequired();
    }
} 