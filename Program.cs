using API.Model;
using API.Model.DTO;
using API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===== Services =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Database
builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySQLconnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySQLconnection"))
    )
);

// Custom Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// JWT Service
builder.Services.AddScoped<JwtService>();

// ===== JWT Authentication =====
var jwtConfig = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtConfig["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtConfig["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Secret"]!)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ===== Swagger Config =====
builder.Services.AddSwaggerGen(c =>
{
    // Swagger Security JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Masukkan JWT token dengan format: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new List<string>()
        }
    });

    // CategoryDTO Schema
    c.MapType<CategoryDTO>(() => new OpenApiSchema
    {
        Type = "object",
        Properties = new Dictionary<string, OpenApiSchema>
        {
            ["id"] = new OpenApiSchema { Type = "integer", Format = "int32", Example = new OpenApiInteger(0) },
            ["name"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("Electronics") },
            ["description"] = new OpenApiSchema { Type = "string", Example = new OpenApiString("Category for electronic products") },
            ["createdAt"] = new OpenApiSchema { Type = "string", Format = "date-time", Example = new OpenApiString("2025-08-09T10:00:00.000Z") },
            ["updatedAt"] = new OpenApiSchema { Type = "string", Format = "date-time", Example = new OpenApiString("2025-08-09T10:00:00.000Z") },
            ["productCount"] = new OpenApiSchema { Type = "integer", Format = "int32", Example = new OpenApiInteger(0) },
            ["products"] = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "ProductDTO" } } }
        },
        Example = new OpenApiObject
        {
            ["id"] = new OpenApiInteger(0),
            ["name"] = new OpenApiString("Electronics"),
            ["description"] = new OpenApiString("Category for electronic products"),
            ["createdAt"] = new OpenApiString("2025-08-09T10:00:00.000Z"),
            ["updatedAt"] = new OpenApiString("2025-08-09T10:00:00.000Z"),
            ["productCount"] = new OpenApiInteger(0),
            ["products"] = new OpenApiArray()
        }
    });

    // CustomerDTO Schema
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

// ===== Middleware =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// JWT Middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
