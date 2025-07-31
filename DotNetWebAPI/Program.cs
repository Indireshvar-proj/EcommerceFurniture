using AngularAPI.Dtos.Helpers;
using AngularAPI.Repository;
using AngularAPI.Services;
using AngularProject.Data;
using AngularProject.Models;
using DotNetWebAPI.Middlewares;
using DotNetWebAPI.Services;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "AllowAll";

// Add services to the container.
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB Context
builder.Services.AddDbContext<ShoppingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ShopDbConn")));

// Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ShoppingDbContext>();

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"])),
    };
});

// Dependency Injection for repositories and services
builder.Services.AddScoped<IGenericRepository<User>, GenericRepositoryT<User>>();
builder.Services.AddScoped<IProductRepository, ProductService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IWishListRepository, WishListRepository>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfiles));

// JSON Reference Loop Handling
builder.Services.AddControllers().AddNewtonsoftJson(x =>
    x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

builder.Services.AddScoped<AuthMiddleware>();

// Enable CORS - open for all origins (adjust for production)
builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins, builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Seed DB
using (var scope = app.Services.CreateScope())
{
    var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ShoppingDbContext>();
        // await db.Database.MigrateAsync(); // Uncomment to enable migrations automatically
        await ShoppingContextSeed.SeedAsync(db, loggerFactory);
    }
    catch (Exception ex)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "An error occurred during migration");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Serve static files from wwwroot root (default)
// This enables URLs like: https://localhost:7231/images/myimage.jpg if image in wwwroot/images
app.UseStaticFiles();

// Explicitly serve static files from wwwroot/images mapped to /images request path
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "img")),
    RequestPath = "/images"
});

// Enable CORS
app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();

// Custom middleware to extract user from token
app.UseMiddleware<AuthMiddleware>();

app.MapControllers();

app.Run();


// Helper extension method you can add somewhere to build full image URLs
public static class ImageUrlHelper
{
    public static string GetFullImageUrl(string relativeImagePath, HttpRequest request)
    {
        if (string.IsNullOrEmpty(relativeImagePath))
            return "";

        // If image path already absolute, return as-is
        if (Uri.TryCreate(relativeImagePath, UriKind.Absolute, out _))
            return relativeImagePath;

        // Otherwise build absolute URL based on current request host + /images/...
        var baseUrl = $"{request.Scheme}://{request.Host}";

        // Ensure relativeImagePath doesn't start with slash to avoid double slash
        string path = relativeImagePath.StartsWith("/") ? relativeImagePath.Substring(1) : relativeImagePath;

        return $"{baseUrl}/images/{path}";
    }
}