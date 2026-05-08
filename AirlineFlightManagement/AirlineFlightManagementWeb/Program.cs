using AirlineFlightManagement.DataAccess.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

// Register Repositories
builder.Services.AddScoped<AirlineFlightManagement.DataAccess.Repositories.Interfaces.IUnitOfWork, AirlineFlightManagement.DataAccess.Repositories.Implementations.UnitOfWork>();

// Register Flight and API Services
builder.Services.AddHttpClient<AirlineFlightManagement.Services.Interfaces.ISerpApiClient, AirlineFlightManagement.Services.Implementations.RealSerpApiClient>();
builder.Services.AddScoped<AirlineFlightManagement.Services.Interfaces.ISystemConfigService, AirlineFlightManagement.Services.Implementations.SystemConfigService>();
builder.Services.AddScoped<AirlineFlightManagement.Services.Interfaces.ISerpApiClient, AirlineFlightManagement.Services.Implementations.RealSerpApiClient>(); // Comută de la MockSerpApiClient la RealSerpApiClient
builder.Services.AddScoped<AirlineFlightManagement.Services.Interfaces.IFlightService, AirlineFlightManagement.Services.Implementations.FlightService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
