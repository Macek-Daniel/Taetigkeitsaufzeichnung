using Taetigkeitsaufzeichnung.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

// 1. Authentifizierung hinzufügen (NEU)
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddDbContext<TaetigkeitsaufzeichnungContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. UI Support für Login hinzufügen (GEÄNDERT)
builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

// Razor Pages werden für die Login-UI benötigt
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// 3. Reihenfolge ist wichtig: Erst Authentication, dann Authorization (NEU)
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Taetigkeit}/{action=Index}/{id?}")
.WithStaticAssets();

// Login-Seiten mappen (NEU)
app.MapRazorPages()
   .WithStaticAssets();

app.Run();