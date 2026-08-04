using System.Text;
using BookingSystem.Application.Interfaces;
using BookingSystem.Application.Services;
using BookingSystem.Infrastructure.Identity;
using BookingSystem.Infrastructure.Notifications;
using BookingSystem.Infrastructure.Persistence;
using BookingSystem.Infrastructure.Persistence.Seed;
using BookingSystem.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// --- EF Core / SQL Server ---
builder.Services.AddDbContext<BookingSystemDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BookingSystem")));

// --- ASP.NET Core Identity (Admin / Provider accounts only - clients never log in) ---
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        // For demo purposes, the requirements are kept simple
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<BookingSystemDbContext>();

// --- JWT bearer authentication ---
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services
    .AddAuthentication(options =>
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
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// --- Application services (business logic) ---
builder.Services.AddScoped<IAppointmentBookingService, AppointmentBookingService>();

// --- Infrastructure implementations ---
// Swap ConsoleNotificationService for a real Email/SMS provider here later -
// nothing above this line, and nothing in Application, needs to change.
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IProviderRepository, ProviderRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<INotificationService, ConsoleNotificationService>();

// --- CORS: the public booking site and the provider admin site both call this API ---
const string CorsPolicy = "BookingSystemClients";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy.WithOrigins(
                builder.Configuration["Clients:PublicSiteUrl"] ?? "http://localhost:5173",
                builder.Configuration["Clients:AdminSiteUrl"] ?? "http://localhost:5174")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Lets you paste a bearer token into Swagger UI's "Authorize" button
    // to try the [Authorize]-protected endpoints manually.
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Paste the token returned from POST /api/auth/login"
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
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Dev-only: creates roles, a demo admin login, a demo provider + login,
    // and enough sample data (one provider, two services, weekly hours) to
    // exercise the public site without manual setup. See DevelopmentSeeder
    // for the actual accounts/passwords it creates.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BookingSystemDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await DevelopmentSeeder.SeedAsync(db, userManager, roleManager, logger);
}

app.UseHttpsRedirection();
app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Exposed for WebApplicationFactory-based integration tests.
public partial class Program { }
