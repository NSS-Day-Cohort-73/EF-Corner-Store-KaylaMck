using CornerStore.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using CornerStore.Models.DTO;
using CornerStore.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// allows passing datetimes without time zone data 
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// allows our api endpoints to access the database through Entity Framework Core and provides dummy value for testing
builder.Services.AddNpgsql<CornerStoreDbContext>(builder.Configuration["CornerStoreDbConnectionString"] ?? "testing");

// Set the JSON serializer options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/cashiers", (CornerStoreDbContext db, CreateCashierDTO cashierDTO) =>
{
    Cashier cashier = new Cashier
    {
        FirstName = cashierDTO.FirstName,
        LastName = cashierDTO.LastName
    };

    db.Cashiers.Add(cashier);
    db.SaveChanges();

    return Results.Created($"/cashiers/{cashier.Id}", cashier);
});

app.MapGet("/cashiers/{id}", (CornerStoreDbContext db, int id) =>
{
    Cashier? cashier = db.Cashiers
        .Include(c => c.Orders)
            .ThenInclude(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                    .ThenInclude(p => p.Category)
        .FirstOrDefault(c => c.Id == id);

    if (cashier == null)
    {
        return Results.NotFound();
    }

    CashierDTO cashierDTO = new CashierDTO
    {
        Id = cashier.Id,
        FirstName = cashier.FirstName,
        LastName = cashier.LastName,
        FullName = cashier.FullName,
        Orders = cashier.Orders.Select(o => new OrderDTO
        {
            Id = o.Id,
            Total = o.Total,
            PaidOnDate = o.PaidOnDate,
            OrderProducts = o.OrderProducts.Select(op => new OrderProductDTO
            {
                Product = new ProductDTO
                {
                    Id = op.Product.Id,
                    ProductName = op.Product.ProductName,
                    Price = op.Product.Price,
                    Brand = op.Product.Brand,
                    Category = new CategoryDTO
                    {
                        Id = op.Product.Category.Id,
                        CategoryName = op.Product.Category.CategoryName
                    }
                },
                Quantity = op.Quantity
            }).ToList()
        }).ToList()
    };

    return Results.Ok(cashierDTO);
});

app.MapGet("/products", (CornerStoreDbContext db, string? search) =>
{
    IQueryable<Product> query = db.Products
        .Include(p => p.Category);

    if (!string.IsNullOrEmpty(search))
    {
        search = search.ToLower();
        query = query.Where(p =>
            p.ProductName.ToLower().Contains(search) ||
            p.Category.CategoryName.ToLower().Contains(search));
    }

    var products = query.Select(p => new ProductDTO
    {
        Id = p.Id,
        ProductName = p.ProductName,
        Price = p.Price,
        Brand = p.Brand,
        Category = new CategoryDTO
        {
            Id = p.Category.Id,
            CategoryName = p.Category.CategoryName
        }
    }).ToList();

    return Results.Ok(products);
});

app.MapPost("/products", (CornerStoreDbContext db, CreateProductDTO productDTO) =>
{
    Product product = new Product
    {
        ProductName = productDTO.ProductName,
        Price = productDTO.Price,
        Brand = productDTO.Brand,
        CategoryId = productDTO.CategoryId
    };

    db.Products.Add(product);
    db.SaveChanges();

    return Results.Created($"/products/{product.Id}", product);
});

app.MapPut("/products/{id}", (CornerStoreDbContext db, int id, UpdateProductDTO updateDTO) =>
{
    Product? productToUpdate = db.Products.FirstOrDefault(p => p.Id == id);

    if (productToUpdate == null)
    {
        return Results.NotFound();
    }

    productToUpdate.ProductName = updateDTO.ProductName;
    productToUpdate.Price = updateDTO.Price;
    productToUpdate.Brand = updateDTO.Brand;
    productToUpdate.CategoryId = updateDTO.CategoryId;

    db.SaveChanges();

    return Results.NoContent();
});

app.MapGet("/orders/{id}", (CornerStoreDbContext db, int id) =>
{
    Order? order = db.Orders
        .Include(o => o.Cashier)
        .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
                .ThenInclude(p => p.Category)
        .FirstOrDefault(o => o.Id == id);

    if (order == null)
    {
        return Results.NotFound();
    }

    OrderDTO orderDTO = new OrderDTO
    {
        Id = order.Id,
        PaidOnDate = order.PaidOnDate,
        Total = order.Total,
        Cashier = new CashierDTO
        {
            Id = order.Cashier.Id,
            FirstName = order.Cashier.FirstName,
            LastName = order.Cashier.LastName,
            FullName = order.Cashier.FirstName
        },
        OrderProducts = order.OrderProducts.Select(op => new OrderProductDTO
        {
            Product = new ProductDTO
            {
                Id = op.Product.Id,
                ProductName = op.Product.ProductName,
                Price = op.Product.Price,
                Brand = op.Product.Brand,
                Category = new CategoryDTO
                {
                    Id = op.Product.Category.Id,
                    CategoryName = op.Product.Category.CategoryName
                }
            },
            Quantity = op.Quantity
        }).ToList()
    };

    return Results.Ok(orderDTO);
});

app.MapGet("/orders", (CornerStoreDbContext db, DateTime? orderDate) =>
{
    IQueryable<Order> query = db.Orders
        .Include(o => o.Cashier)
        .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
                .ThenInclude(p => p.Category);

    if (orderDate.HasValue)
    {
        query = query.Where(o =>
            o.PaidOnDate.HasValue &&
            o.PaidOnDate.Value.Date == orderDate.Value.Date);
    }

    var orders = query.Select(o => new OrderDTO
    {
        Id = o.Id,
        Total = o.Total,
        PaidOnDate = o.PaidOnDate,
        Cashier = new CashierDTO
        {
            Id = o.Cashier.Id,
            FirstName = o.Cashier.FirstName,
            LastName = o.Cashier.LastName,
            FullName = o.Cashier.FirstName
        },
        OrderProducts = o.OrderProducts.Select(op => new OrderProductDTO
        {
            Product = new ProductDTO
            {
                Id = op.Product.Id,
                ProductName = op.Product.ProductName,
                Price = op.Product.Price,
                Brand = op.Product.Brand,
                Category = new CategoryDTO
                {
                    Id = op.Product.Category.Id,
                    CategoryName = op.Product.Category.CategoryName
                }
            },
            Quantity = op.Quantity
        }).ToList()
    }).ToList();

    return Results.Ok(orders);
});

app.MapDelete("/orders/{id}", (CornerStoreDbContext db, int id) =>
{
    Order? order = db.Orders.FirstOrDefault(o => o.Id == id);

    if (order == null)
    {
        return Results.NotFound();
    }

    db.Orders.Remove(order);
    db.SaveChanges();

    return Results.NoContent();
});

app.MapPost("/orders", (CornerStoreDbContext db, CreateOrderDTO orderDTO) =>
{
   Order order = new Order
   {
       CashierId = orderDTO.CashierId,
       OrderProducts = orderDTO.OrderProducts.Select(op =>
       {
           var product = db.Products
               .Include(p => p.Category)
               .FirstOrDefault(p => p.Id == op.ProductId);
               
           if (product == null) return null;

           return new OrderProduct
           {
               ProductId = op.ProductId,
               Product = product,
               Quantity = op.Quantity
           };
       }).Where(op => op != null).ToList()
   };

   order.Cashier = db.Cashiers.FirstOrDefault(c => c.Id == orderDTO.CashierId);

   db.Orders.Add(order);
   db.SaveChanges();

   return Results.Created($"/orders/{order.Id}", order);
});

app.Run();

//don't move or change this!
public partial class Program { }