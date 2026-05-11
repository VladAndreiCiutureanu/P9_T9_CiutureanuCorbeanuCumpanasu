using AirlineFlightManagement.DataAccess.Data;
using AirlineFlightManagement.DataAccess.Repositories.Implementations;
using AirlineFlightManagement.DataAccess.Repositories.Interfaces;
using AirlineFlightManagement.Models.Models;
using AirlineFlightManagement.Services.Implementations;
using AirlineFlightManagement.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// CONFIGURARE COMPLETĂ IDENTITY (REQ-12, REQ-19 + Suport Roluri)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Constrângeri parole (REQ-12)
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;

    // Unicitate email (REQ-19)
    options.User.RequireUniqueEmail = true;

    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ÎNREGISTRARE DEPENDENȚE (DI)

// Coleg A — Auth + Profile
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPassengerService, PassengerService>();

// Coleg B — Flights + SerpAPI
builder.Services.AddScoped<IFlightService, FlightService>();
builder.Services.AddScoped<IMarkupService, MarkupService>();
// Mock pentru dev/test — schimba in RealSerpApiClient pentru productie
builder.Services.AddScoped<ISerpApiClient, MockSerpApiClient>();
// HttpClient pentru RealSerpApiClient (cand vom comuta) — vezi REQ 5.1 timeout 5s
builder.Services.AddHttpClient<RealSerpApiClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
});

// Coleg C — Reservations + Payments + Reports
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Razor Pages NU e folosit — Identity UI custom prin AccountController (MVC).
// Daca cineva are nevoie de Razor Pages in viitor, adaugati builder.Services.AddRazorPages()
// si decomentati app.MapRazorPages() de mai jos.

app.Run();