using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Security.Claims;
using System.Text;
using ToursService.Controllers;
using ToursService.Database;
using ToursService.Domain.RepositoryInterfaces;
using ToursService.Repositories;
using ToursService.UseCases;


var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<ToursContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Controllers
builder.Services.AddControllers();

// Swagger + JWT schema
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

// gRPC
builder.Services.AddGrpc();

// AuthN (JWT) – koristi iste vrednosti kao stakeholders (Jwt:Issuer/Audience/Key)
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],   // "explorer"
            ValidAudience = builder.Configuration["Jwt:Audience"], // "explorer-front.com"
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)), // ista tajna

            // VAŽNO: role claim ti je upisan kao ClaimTypes.Role
            RoleClaimType = ClaimTypes.Role,         // radi sa tvojim tokenom
            NameClaimType = "id"                     // pošto ti upisuješ custom "id" claim
            // (alternativa: NameClaimType = ClaimTypes.NameIdentifier ako bi koristila "sub")
        };

        // mala tolerancija ako satovi kasne
        options.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(2);
    });

// AuthZ (policy-je prilagodi kasnije po potrebi)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("administratorPolicy", p => p.RequireRole("Administrator"));
    options.AddPolicy("touristPolicy", p => p.RequireRole("Tourist"));
    options.AddPolicy("authorPolicy", p => p.RequireRole("TourAuthor"));
    options.AddPolicy("userPolicy", p => p.RequireRole("TourAuthor", "Tourist"));
});

// CORS
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

// Repositories
builder.Services.AddScoped<ITourRepository, TourRepository>();
builder.Services.AddScoped<IKeyPointRepository, KeyPointRepository>();
builder.Services.AddScoped<ITourReviewRepository, TourReviewRepository>();
builder.Services.AddScoped<IPositionRepository, PositionRepository>();



// Services
builder.Services.AddScoped<IPositionService, PositionService>();
builder.Services.AddScoped<ITourService, TourService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);




var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();
app.UseCors("AllowAngularDevClient");

app.UseAuthentication();   // 👈 bitno: pre Authorization
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health/db", async (ToursContext db) =>
{
    var ok = await db.Database.CanConnectAsync();
    return ok ? Results.Ok("DB OK") : Results.Problem("DB FAIL");
});

// REST endpoint (5226)
app.MapControllers()
    .RequireHost("localhost:5226");

// gRPC endpoint (5001, bez Swagger-a)
app.MapGrpcService<ToursProtoController>()
    .RequireHost("localhost:5002");

//app.MapGrpcService();
//app.MapGrpcService<ToursProtoController>();

app.Run();
