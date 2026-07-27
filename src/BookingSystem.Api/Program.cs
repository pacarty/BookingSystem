using BookingSystem.Application.Interfaces;
using BookingSystem.Application.Services;
using BookingSystem.Infrastructure.Notifications;
using BookingSystem.Infrastructure.Persistence;
using BookingSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- EF Core / SQL Server ---
builder.Services.AddDbContext<BookingSystemDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BookingSystem")));

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
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(CorsPolicy);
app.UseAuthorization();
app.MapControllers();

app.Run();

// Exposed for WebApplicationFactory-based integration tests.
public partial class Program { }
