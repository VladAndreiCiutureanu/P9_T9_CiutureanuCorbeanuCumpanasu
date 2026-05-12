using AirlineFlightManagement.DataAccess.Data;
using AirlineFlightManagement.DataAccess.Repositories.Implementations;
using AirlineFlightManagement.DataAccess.Repositories.Interfaces;
using AirlineFlightManagement.Models.Models;
using AirlineFlightManagement.Services.Implementations;
using AirlineFlightManagement.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Fortam incarcarea user-secrets indiferent de mediu. Default-ul
// (incarca doar in Development) e respectat de obicei, dar Visual Studio
// uneori cache-uieste configurarea si nu vede secret-urile actualizate
// dupa "dotnet user-secrets set". Cu apelul explicit, ne asiguram ca
// SerpApi:ApiKey e disponibil oricum a fost pornit app-ul.
builder.Configuration.AddUserSecrets(typeof(Program).Assembly, optional: true);

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

// Selectie automata: daca exista API key configurat (in appsettings.json sau
// user-secrets), folosim RealSerpApiClient; altfel cadem inapoi pe Mock.
// Asta permite oricui sa ruleze proiectul fara API key (cu Mock), iar cei
// care vor date reale doar adauga key-ul in config.
// ─── DIAGNOSTIC: starea configurarii la pornire ─────────────────────────
Console.WriteLine("════════════════════════════════════════════════════════");
Console.WriteLine($"[Startup] Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"[Startup] ContentRoot:  {builder.Environment.ContentRootPath}");

var serpApiKey = builder.Configuration["SerpApi:ApiKey"];
if (string.IsNullOrWhiteSpace(serpApiKey))
{
    Console.WriteLine("[Startup] SerpApi:ApiKey = <EMPTY>");
}
else
{
    var preview = serpApiKey.Length >= 8
        ? serpApiKey.Substring(0, 8) + "..." + serpApiKey.Substring(serpApiKey.Length - 4)
        : "<too short>";
    Console.WriteLine($"[Startup] SerpApi:ApiKey loaded: {preview} (length {serpApiKey.Length})");
}

if (!string.IsNullOrWhiteSpace(serpApiKey))
{
    builder.Services.AddHttpClient<ISerpApiClient, RealSerpApiClient>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(5);  // REQ 5.1
    });
    Console.WriteLine("[SerpAPI] >>> RealSerpApiClient registered <<<");
}
else
{
    builder.Services.AddScoped<ISerpApiClient, MockSerpApiClient>();
    Console.WriteLine("[SerpAPI] >>> MockSerpApiClient registered (no API key) <<<");
}
Console.WriteLine("════════════════════════════════════════════════════════");

// Coleg C — Reservations + Payments + Reports
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// ─── Seeding roluri si primul administrator ─────────────────────────────
// Ruleaza la fiecare startup, dar e idempotent (verifica existenta inainte).
// REQ-4: cele 3 roluri din SRS (Administrator, Staff, Customer).
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    // 1. Cream rolurile lipsa
    foreach (var role in new[] { "Administrator", "Staff", "Customer" })
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // 2. Conventie first-run: daca nu exista niciun admin in sistem,
    //    primul user existent devine automat administrator.
    //    Util pentru dezvoltare si demo (rezolvi catch-22-ul "cine creeaza
    //    primul admin"). Ruleaza o singura data — dupa ce exista un admin,
    //    blocul devine no-op.
    var existingAdmins = await userManager.GetUsersInRoleAsync("Administrator");
    if (!existingAdmins.Any())
    {
        var firstUser = userManager.Users.FirstOrDefault();
        if (firstUser != null)
        {
            await userManager.AddToRoleAsync(firstUser, "Administrator");
        }
    }
}

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