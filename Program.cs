using API.Model;
using API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using API.Model.DTO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySQLconnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySQLconnection"))
    )
);

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // CategoryDTO Schema (existing)
    c.MapType<CategoryDTO>(() => new OpenApiSchema
    {
        Type = "object",
        Properties = new Dictionary<string, OpenApiSchema>
        {
            ["id"] = new OpenApiSchema { Type = "integer", Format = "int32", Example = new OpenApiInteger(0) },
            ["name"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("Electronics") },
            ["description"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("Category for electronic products and gadgets") },
            ["createdAt"] = new OpenApiSchema { Type = "string", Format = "date-time", Example = new OpenApiString("2025-08-09T10:00:00.000Z") },
            ["updatedAt"] = new OpenApiSchema { Type = "string", Format = "date-time", Example = new OpenApiString("2025-08-09T10:00:00.000Z") },
            ["productCount"] = new OpenApiSchema { Type = "integer", Format = "int32", Example = new OpenApiInteger(0) },
            ["products"] = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "ProductDTO" } } }
        },
        Example = new OpenApiObject
        {
            ["id"] = new OpenApiInteger(0),
            ["name"] = new OpenApiString("Electronics"),
            ["description"] = new OpenApiString("Category for electronic products and gadgets"),
            ["createdAt"] = new OpenApiString("2025-08-09T10:00:00.000Z"),
            ["updatedAt"] = new OpenApiString("2025-08-09T10:00:00.000Z"),
            ["productCount"] = new OpenApiInteger(0),
            ["products"] = new OpenApiArray()
        }
    });

    // CustomerDTO Schema - TAMBAHKAN INI
    c.MapType<CustomerDTO>(() => new OpenApiSchema
    {
        Type = "object",
        Properties = new Dictionary<string, OpenApiSchema>
        {
            ["id"] = new OpenApiSchema { Type = "integer", Format = "int32", Example = new OpenApiInteger(0) },
            ["firstName"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("John") },
            ["lastName"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("Doe") },
            ["email"] = new OpenApiSchema { Type = "string", Format = "email", Example = new OpenApiString("john.doe@example.com") },
            ["phone"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("081234567890") },
            ["address"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("Jl. Merdeka No. 123") },
            ["city"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("Bandung") },
            ["postalCode"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("40112") },
            ["isActive"] = new OpenApiSchema { Type = "boolean", Example = new OpenApiBoolean(true) },
            ["createdAt"] = new OpenApiSchema { Type = "string", Format = "date-time", Example = new OpenApiString("2025-08-10T10:00:00.000Z") },
            ["updatedAt"] = new OpenApiSchema { Type = "string", Format = "date-time", Example = new OpenApiString("2025-08-10T10:00:00.000Z") },
            ["totalOrders"] = new OpenApiSchema { Type = "integer", Format = "int32", Example = new OpenApiInteger(0) },
            ["totalSpent"] = new OpenApiSchema { Type = "number", Format = "decimal", Example = new OpenApiDouble(0.0) }
        },
        Example = new OpenApiObject
        {
            ["id"] = new OpenApiInteger(0),
            ["firstName"] = new OpenApiString("John"),
            ["lastName"] = new OpenApiString("Doe"),
            ["email"] = new OpenApiString("john.doe@example.com"),
            ["phone"] = new OpenApiString("081234567890"),
            ["address"] = new OpenApiString("Jl. Merdeka No. 123"),
            ["city"] = new OpenApiString("Bandung"),
            ["postalCode"] = new OpenApiString("40112"),
            ["isActive"] = new OpenApiBoolean(true),
            ["createdAt"] = new OpenApiString("2025-08-10T10:00:00.000Z"),
            ["updatedAt"] = new OpenApiString("2025-08-10T10:00:00.000Z"),
            ["totalOrders"] = new OpenApiInteger(0),
            ["totalSpent"] = new OpenApiDouble(0.0)
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();