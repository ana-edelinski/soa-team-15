using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PaymentsService.Database;
using PaymentsService.Domain.RepositoryInterfaces;
using PaymentsService.Mappers;
using PaymentsService.Repositories;
using PaymentsService.UseCases;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ------------------------
// DbContext
// ------------------------
builder.Services.AddDbContext<PaymentsContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ------------------------
// Controllers
// ------------------------
builder.Services.AddControllers();

// ------------------------
// Swagger + JWT schema
// ------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ------------------------
// AuthN (JWT)
// ------------------------
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],      // "explorer"
            ValidAudience = builder.Configuration["Jwt:Audience"],  // "explorer-front.com"
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),

            RoleClaimType = ClaimTypes.Role,
            NameClaimType = "id"
        };

        options.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(2);
    });

// ------------------------
// AuthZ (policies)
// ------------------------
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("administratorPolicy", p => p.RequireRole("Administrator"));
    options.AddPolicy("touristPolicy", p => p.RequireRole("Tourist"));
    options.AddPolicy("authorPolicy", p => p.RequireRole("TourAuthor"));
    options.AddPolicy("userPolicy", p => p.RequireRole("TourAuthor", "Tourist"));
});

// ------------------------
// CORS
// ------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDevClient", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ------------------------
// Repositories
// ------------------------
builder.Services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
// kasnije dodaš i ostale: CouponRepository, PaymentRecordRepository itd.

// ------------------------
// Services
// ------------------------
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();
builder.Services.AddScoped<IOrderItemService, OrderItemService>();
// kasnije dodaš i ostale: CouponService, PaymentService itd.

// ------------------------
// AutoMapper
// ------------------------
builder.Services.AddAutoMapper(typeof(PaymentsProfile).Assembly);

var app = builder.Build();

// ------------------------
// Middleware pipeline
// ------------------------
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAngularDevClient");

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health endpoint
app.MapGet("/health/db", async (PaymentsContext db) =>
{
    var ok = await db.Database.CanConnectAsync();
    return ok ? Results.Ok("DB OK") : Results.Problem("DB FAIL");
});

app.Run();
