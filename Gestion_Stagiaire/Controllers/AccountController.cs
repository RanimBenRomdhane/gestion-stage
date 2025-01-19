using Gestion_Stagiaire.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Gestion_Stagiaire.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // Login avec Google
        [HttpGet]
        public IActionResult Login()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Account");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        // Réponse de l'authentification Google
        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                // Gestion des erreurs d'authentification
                return RedirectToAction("Login");
            }

            // Récupérer les informations de l'utilisateur (email, nom, prénom)
            var email = result.Principal.FindFirstValue(ClaimTypes.Email);
            var name = result.Principal.FindFirstValue(ClaimTypes.Name);
            var firstName = result.Principal.FindFirstValue(ClaimTypes.GivenName); // Prénom

            // Vérifier si l'utilisateur existe déjà
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = email,
                    Email = email
                };

                // Créer l'utilisateur
                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return RedirectToAction("Error", "Home");
                }

                // Vérifier si le rôle "Stagiaire" existe et le créer s'il n'existe pas
                var roleExist = await _roleManager.RoleExistsAsync("Stagiaire");
                if (!roleExist)
                {
                    var role = new IdentityRole("Stagiaire");
                    await _roleManager.CreateAsync(role);
                }

                // Assigner le rôle "Stagiaire"
                var roleResult = await _userManager.AddToRoleAsync(user, "Stagiaire");
                if (!roleResult.Succeeded)
                {
                    return RedirectToAction("Error", "Home");
                }
            }

            // Sign in the user
            await _signInManager.SignInAsync(user, isPersistent: true);

            // Rediriger vers la page d'accueil
            return RedirectToAction("Index", "Home");
        }


        // Déconnexion
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Logout()
        {
            // Déconnexion de l'utilisateur
            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Rediriger vers la page de login
            return RedirectToAction("Login");
        }
        

    }
}
