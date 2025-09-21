// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.IdentityModel.Tokens;
// using Microsoft.OpenApi.Models;
// using System;
// using System.Security.Claims;
// using System.Text;
// using ToursService.Database;
// using ToursService.Domain.RepositoryInterfaces;
// using ToursService.Repositories;
// using ToursService.UseCases;



// var builder = WebApplication.CreateBuilder(args);

// // DbContext
// builder.Services.AddDbContext<ToursContext>(opt =>
//     opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// var cs = builder.Configuration.GetConnectionString("DefaultConnection");

// var dsb = new Npgsql.NpgsqlDataSourceBuilder(cs);
// dsb.EnableDynamicJson();                 // ili: dsb.UseJsonNet();
// var dataSource = dsb.Build();
// builder.Services.AddDbContext<ToursContext>(opt => opt.UseNpgsql(dataSource));

// // Controllers
// builder.Services.AddControllers();

// // Swagger + JWT schema
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen(options =>
// {
//     options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//     {
//         Name = "Authorization",
//         Type = SecuritySchemeType.Http,
//         Scheme = "Bearer",
//         BearerFormat = "JWT",
//         In = ParameterLocation.Header,
//         Description = "Enter 'Bearer' [space] and then your valid token."
//     });

//     options.AddSecurityRequirement(new OpenApiSecurityRequirement
//     {
//         {
//             new OpenApiSecurityScheme {
//                 Reference = new OpenApiReference {
//                     Type = ReferenceType.SecurityScheme,
//                     Id = "Bearer"
//                 }
//             },
//             Array.Empty<string>()
//         }
//     });
// });

// // AuthN (JWT) – koristi iste vrednosti kao stakeholders (Jwt:Issuer/Audience/Key)
// builder.Services.AddAuthentication("Bearer")
//     .AddJwtBearer("Bearer", options =>
//     {
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidateIssuer = true,
//             ValidateAudience = true,
//             ValidateLifetime = true,
//             ValidateIssuerSigningKey = true,
//             ValidIssuer = builder.Configuration["Jwt:Issuer"],   // "explorer"
//             ValidAudience = builder.Configuration["Jwt:Audience"], // "explorer-front.com"
//             IssuerSigningKey = new SymmetricSecurityKey(
//                 Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)), // ista tajna

//             // VAŽNO: role claim ti je upisan kao ClaimTypes.Role
//             RoleClaimType = ClaimTypes.Role,         // radi sa tvojim tokenom
//             NameClaimType = "id"                     // pošto ti upisuješ custom "id" claim
//             // (alternativa: NameClaimType = ClaimTypes.NameIdentifier ako bi koristila "sub")
//         };

//         // mala tolerancija ako satovi kasne
//         options.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(2);
//     });

// // AuthZ (policy-je prilagodi kasnije po potrebi)
// builder.Services.AddAuthorization(options =>
// {
//     options.AddPolicy("administratorPolicy", p => p.RequireRole("Administrator"));
//     options.AddPolicy("touristPolicy", p => p.RequireRole("Tourist"));
//     options.AddPolicy("authorPolicy", p => p.RequireRole("TourAuthor"));
//     options.AddPolicy("userPolicy", p => p.RequireRole("TourAuthor", "Tourist"));
// });

// // CORS
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowAngularDevClient", policy =>
//     {
//         policy.WithOrigins("http://localhost:4200")
//               .AllowAnyHeader()
//               .AllowAnyMethod()
//               .AllowCredentials();
//     });
// });

// // Repositories
// builder.Services.AddScoped<ITourRepository, TourRepository>();
// builder.Services.AddScoped<IKeyPointRepository, KeyPointRepository>();
// builder.Services.AddScoped<ITourReviewRepository, TourReviewRepository>();
// builder.Services.AddScoped<IPositionRepository, PositionRepository>();
// builder.Services.AddScoped<ITourExecutionRepository, TourExecutionRepository>();


// // Services
// builder.Services.AddScoped<IPositionService, PositionService>();
// builder.Services.AddScoped<ITourService, TourService>();
// builder.Services.AddScoped<ITourReviewService, TourReviewService>();
// builder.Services.AddScoped<ITourExecutionService, TourExecutionService>();


// // AutoMapper
// builder.Services.AddAutoMapper(typeof(Program).Assembly);




// var app = builder.Build();

// // Swagger
// app.UseSwagger();
// app.UseSwaggerUI();

// app.UseCors("AllowAngularDevClient");

// app.UseStaticFiles(); // wwwroot/*

// app.UseAuthentication();   // 👈 bitno: pre Authorization
// app.UseAuthorization();

// app.MapControllers();
// app.MapGet("/health/db", async (ToursContext db) =>
// {
//     var ok = await db.Database.CanConnectAsync();
//     return ok ? Results.Ok("DB OK") : Results.Problem("DB FAIL");
// });

// app.Run();



using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Grafana.Loki;
using System.Security.Claims;
using System.Text;
using ToursService.Database;
using ToursService.Domain.RepositoryInterfaces;
using ToursService.Repositories;
using ToursService.UseCases;

//
// 1) Minimalni bootstrap logger (prije builder-a)
//
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("service", "tours-service")
    .WriteTo.Console(new CompactJsonFormatter())
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    //
    // 2) Host -> Serilog
    //
    builder.Host.UseSerilog((ctx, cfg) =>
    {
        var env = ctx.HostingEnvironment.EnvironmentName;

        cfg.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
           .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
           .Enrich.FromLogContext()
           .Enrich.WithProperty("service", "tours-service")
           .Enrich.WithProperty("env", env)
           .WriteTo.Console(new CompactJsonFormatter());

        var lokiUrl = ctx.Configuration["Logging:Loki:Url"];
        if (!string.IsNullOrWhiteSpace(lokiUrl))
        {
            cfg.WriteTo.GrafanaLoki(
                lokiUrl,
                labels: new List<LokiLabel>
                {
                    new LokiLabel { Key = "service", Value = "tours-service" },
                    new LokiLabel { Key = "env", Value = env }
                }
            );
        }
    });

    //
    // 3) Services
    //
    builder.Services.AddControllers();

    // Swagger + JWT schema
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "Tours API", Version = "v1" });
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter 'Bearer {token}'"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme {
                    Reference = new OpenApiReference {
                        Type = ReferenceType.SecurityScheme, Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // DbContext (može i preko NpgsqlDataSource ako želiš)
    builder.Services.AddDbContext<ToursContext>(opt =>
        opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    // AuthN
    builder.Services.AddAuthentication("Bearer")
        .AddJwtBearer("Bearer", options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
                ),
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = "id"
            };
            options.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(2);
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
    builder.Services.AddScoped<ITourExecutionRepository, TourExecutionRepository>();

    // Services
    builder.Services.AddScoped<IPositionService, PositionService>();
    builder.Services.AddScoped<ITourService, TourService>();
    builder.Services.AddScoped<ITourReviewService, TourReviewService>();
    builder.Services.AddScoped<ITourExecutionService, TourExecutionService>();

    builder.Services.AddAutoMapper(typeof(Program).Assembly);

    //
    // 4) Build
    //
    var app = builder.Build();

    //
    // 5) Middleware
    //
    app.UseSerilogRequestLogging(); // request log u Serilog

    // Correlation-ID
    app.Use(async (ctx, next) =>
    {
        const string header = "X-Correlation-ID";
        if (!ctx.Request.Headers.TryGetValue(header, out var cid) || string.IsNullOrWhiteSpace(cid))
            cid = Guid.NewGuid().ToString("n");

        using (LogContext.PushProperty("correlationId", cid.ToString()))
        {
            ctx.Response.Headers[header] = cid!;
            await next();
        }
    });

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors("AllowAngularDevClient");
    app.UseStaticFiles();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Health
    app.MapGet("/health/db", async (ToursContext db) =>
    {
        var ok = await db.Database.CanConnectAsync();
        return ok ? Results.Ok("DB OK") : Results.Problem("DB FAIL");
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}
