using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StakeholdersService.Authentication;
using StakeholdersService.Controllers;
using StakeholdersService.Database;
using StakeholdersService.Domain.RepositoryInterfaces;
using StakeholdersService.Repositories;
using StakeholdersService.UseCases;
using System.Diagnostics;




var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// gRPC
builder.Services.AddGrpc();

// JWT Auth
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("administratorPolicy", policy => policy.RequireRole("Administrator"));
    options.AddPolicy("touristPolicy", policy => policy.RequireRole("Tourist"));
    options.AddPolicy("guidePolicy", policy => policy.RequireRole("Guide"));
    options.AddPolicy("userPolicy", policy => policy.RequireRole("TourAuthor", "Tourist"));
});

// AutoMapper + repos + services
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<ITokenGenerator, JwtGenerator>();
builder.Services.AddScoped<IAccountService, AccountService>();

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

// DB
builder.Services.AddDbContext<StakeholdersContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("stakeholders-service", serviceVersion: "1.0.0"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation(o => o.RecordException = true)
        .AddHttpClientInstrumentation()
        .AddSource("stakeholders.custom")           // << BITNO: da se manual span iz /trace-test izvozi
        .SetSampler(new AlwaysOnSampler())

        // 1) Zipkin ? Jaeger (9411)
        .AddZipkinExporter(o =>
        {
            o.Endpoint = new Uri("http://jaeger:9411/api/v2/spans");
        })

        // 2) OTLP gRPC ? Jaeger (4317) (drxi oba paralelno, nema stete)
        .AddOtlpExporter(o =>
        {
            o.Protocol = OtlpExportProtocol.Grpc;
            o.Endpoint = new Uri("http://jaeger:4317");
        })
    );




var app = builder.Build();

var actSource = new ActivitySource("stakeholders.custom");

app.MapGet("/trace-test", async () =>
{
    using var span = actSource.StartActivity("manual-span");
    await Task.Delay(120);
    return Results.Text("traced", "text/plain");
});



app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();
app.UseCors("AllowAngularDevClient");
app.UseAuthentication();
app.UseAuthorization();

// REST endpoint (8080)
app.MapControllers();

// gRPC endpoint (5001, bez Swagger-a)
app.MapGrpcService<AuthenticationProtoController>();


app.Run();
