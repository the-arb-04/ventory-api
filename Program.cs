using Serilog;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Inventory_Tracker.Models;
using Inventory_Tracker.Services;
using InventoryTracker.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using System.Security.Claims; // ✅ Needed for RoleClaimType
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;

// --- 1. Configure Serilog for initial startup logging ---
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up Inventory Tracker API");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // --- 2. Replace default logger with Serilog ---
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    // --- 3. Configure CORS ---
    var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: MyAllowSpecificOrigins, policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://localhost:5174")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    // --- 4. Register DbContext ---
    builder.Services.AddDbContext<InventoryDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    // --- 5. Add Identity ---
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<InventoryDbContext>()
        .AddDefaultTokenProviders();

    // --- 6. Add Authentication & Configure JWT ---
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),

            // ✅ This makes ASP.NET recognize your "role" claim
            NameClaimType = JwtRegisteredClaimNames.Name,
            RoleClaimType = ClaimTypes.Role

        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                // This will log the exact reason the token validation failed.
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError("Authentication failed.", context.Exception);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                // This will log when a token is successfully validated.
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Token validated for {user}.", context.Principal.Identity.Name);
                return Task.CompletedTask;
            }
        };

    });

    // --- 7. Controllers + JSON options ---
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });

    // --- 8. Swagger ---
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Inventory Tracker API", Version = "v1" });

        var securitySchema = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Enter only your JWT token here (without the 'Bearer ' prefix)",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        };

        c.AddSecurityDefinition("Bearer", securitySchema);
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            { securitySchema, new string[] { } }
        });
    });

    // --- 9. Application Services ---
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<IItemService, ItemService>();
    builder.Services.AddScoped<IItemHistoryService, ItemHistoryService>();
    builder.Services.AddScoped<IDashboardService, DashboardService>();
    builder.Services.AddScoped<ISalesService, SalesService>();
    builder.Services.AddScoped<IAiService, AiService>();
    builder.Services.AddScoped<ForecastingService>();
    builder.Services.AddHttpClient();
    builder.Services.AddScoped<ITokenService, TokenService>();

    var app = builder.Build();

    // --- 10. Seed Admin ---
    using (var scope = app.Services.CreateScope())
    {
        await AdminSeeder.SeedAsync(scope.ServiceProvider);
    }

    // --- 11. Middleware ---
    app.UseSerilogRequestLogging();
    
    app.UseSwagger();
        app.UseSwaggerUI();

    if (app.Environment.IsDevelopment())
    {

    }

    app.UseHttpsRedirection();
    app.UseCors(MyAllowSpecificOrigins);

    app.UseAuthentication(); // ✅ Must come before UseAuthorization
    app.UseAuthorization();

    app.MapControllers();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}
