using Gestion_Stagiaire.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Configuration de la connexion � la base de donn�es
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configuration de l'identit� et des r�les
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>() // Ajout de la gestion des r�les
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

// Configuration de l'authentification avec Google
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.CallbackPath = "/signin-google"; // Doit correspondre � la route de callback
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("StagiaireOnly", policy => policy.RequireRole("Stagiaire"));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RHOnly", policy => policy.RequireRole("RH"));
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login"; // Redirige l'utilisateur vers la page de connexion s'il est non authentifi�
    options.AccessDeniedPath = "/Identity/Account/AccessDenied"; // Redirige s'il n'a pas acc�s � une page en fonction des r�les
});
var app = builder.Build();

// Ajout des r�les par d�faut si ils n'existent pas
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Cr�ation des r�les par d�faut
    var roles = new[] { "Stagiaire", "Admin", "RH" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Initialisation des donn�es, cr�ation de l'utilisateur "Stagiaire"
    await SeedData.Initialize(scope.ServiceProvider, userManager, roleManager);
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();


// Classe SeedData pour initialiser les r�les et utilisateurs par d�faut
public class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // V�rification de l'existence du r�le "Stagiaire"
        var roleExists = await roleManager.RoleExistsAsync("Stagiaire");
        if (!roleExists)
        {
            var role = new IdentityRole("Stagiaire");
            await roleManager.CreateAsync(role);
        }

        // Cr�er un utilisateur "test@stagiaire.com" sans mot de passe (l'authentification Google prendra en charge cela)
        var user = await userManager.FindByEmailAsync("test@stagiaire.com");

        if (user == null)
        {
            user = new IdentityUser { UserName = "test@stagiaire.com", Email = "test@stagiaire.com" };
            await userManager.CreateAsync(user); // Pas besoin de d�finir de mot de passe ici
        }

        // Ajouter l'utilisateur au r�le "Stagiaire" si ce n'est pas d�j� fait
        if (!await userManager.IsInRoleAsync(user, "Stagiaire"))
        {
            await userManager.AddToRoleAsync(user, "Stagiaire");
        }
    }
}

