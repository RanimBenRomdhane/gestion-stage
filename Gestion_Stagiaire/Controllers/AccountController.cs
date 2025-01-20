using Gestion_Stagiaire.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace Gestion_Stagiaire.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;


        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, RoleManager<IdentityRole> roleManager, ILogger<AccountController> logger)

        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Account");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                return RedirectToAction("Login");
            }

            // Retrieve user information
            var email = result.Principal.FindFirstValue(ClaimTypes.Email);
            var name = result.Principal.FindFirstValue(ClaimTypes.Name);
            var id = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);


            // Check if the user exists
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = email,
                    Email = email
                };

                // Create the user
                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return RedirectToAction("Error", "Home");
                }

                // Add default role if needed
                var roleExists = await _roleManager.RoleExistsAsync("Stagiaire");
                if (!roleExists)
                {
                    await _roleManager.CreateAsync(new IdentityRole("Stagiaire"));
                }

                await _userManager.AddToRoleAsync(user, "Stagiaire");
            }

            // Get the roles of the user
            var roles = await _userManager.GetRolesAsync(user);

            // Add claims, including roles
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Email, email),
        new Claim(ClaimTypes.Name, name),
        new Claim(ClaimTypes.NameIdentifier, id)

    };

            // Add roles as claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Create a new ClaimsIdentity
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Create the ClaimsPrincipal
            var principal = new ClaimsPrincipal(identity);

            // Sign in the user with the claims
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Redirect to ensure claims are refreshed
            return RedirectToAction("Index", "Home");
        }
       
    }
}