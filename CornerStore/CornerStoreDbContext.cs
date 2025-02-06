using Microsoft.EntityFrameworkCore;
using CornerStore.Models;

namespace CornerStore.Data;

public class CornerStoreDbContext : DbContext
{
    public DbSet<Cashier> Cashiers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderProduct> OrderProducts { get; set; }

    public CornerStoreDbContext(DbContextOptions<CornerStoreDbContext> context) : base(context) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderProduct>()
            .HasKey(op => new { op.OrderId, op.ProductId });

        modelBuilder.Entity<Category>().HasData(new Category[]
        {
            new Category { Id = 1, CategoryName = "Beverages" },
            new Category { Id = 2, CategoryName = "Snacks" },
            new Category { Id = 3, CategoryName = "Personal Care" },
            new Category { Id = 4, CategoryName = "Household" }
        });

        modelBuilder.Entity<Product>().HasData(new Product[]
        {
            new Product { Id = 1, ProductName = "Cola", Price = 1.99M, Brand = "Coca-Cola", CategoryId = 1 },
            new Product { Id = 2, ProductName = "Potato Chips", Price = 2.99M, Brand = "Lays", CategoryId = 2 },
            new Product { Id = 3, ProductName = "Toothpaste", Price = 3.99M, Brand = "Colgate", CategoryId = 3 },
            new Product { Id = 4, ProductName = "Paper Towels", Price = 4.99M, Brand = "Bounty", CategoryId = 4 },
            new Product { Id = 5, ProductName = "Energy Drink", Price = 2.99M, Brand = "Red Bull", CategoryId = 1 },
            new Product { Id = 6, ProductName = "Chocolate Bar", Price = 1.50M, Brand = "Hershey's", CategoryId = 2 }
        });

        modelBuilder.Entity<Cashier>().HasData(new Cashier[]
        {
            new Cashier { Id = 1, FirstName = "John", LastName = "Doe" },
            new Cashier { Id = 2, FirstName = "Jane", LastName = "Smith" }
        });

        modelBuilder.Entity<Order>().HasData(new Order[]
        {
            new Order 
            { 
                Id = 1, 
                CashierId = 1, 
                PaidOnDate = DateTime.Parse("2024-02-05 10:30:00")
            },
            new Order 
            { 
                Id = 2, 
                CashierId = 2, 
                PaidOnDate = DateTime.Parse("2024-02-05 14:45:00")
            }
        });

        modelBuilder.Entity<OrderProduct>().HasData(new OrderProduct[]
        {
            new OrderProduct { OrderId = 1, ProductId = 1, Quantity = 2 },
            new OrderProduct { OrderId = 1, ProductId = 2, Quantity = 1 },
            new OrderProduct { OrderId = 2, ProductId = 3, Quantity = 1 },
            new OrderProduct { OrderId = 2, ProductId = 4, Quantity = 2 }
        });
    }
}