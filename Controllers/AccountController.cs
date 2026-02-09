using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient; 
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using trial.DAL;
using trial.Models;

namespace trial.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserDAL _userDAL;
        private readonly string _connectionString;

        public AccountController(IConfiguration configuration)
        {
            _userDAL = new UserDAL(configuration);
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                                ?? throw new InvalidOperationException("DefaultConnection not found.");
        }

        // --- 1. LOGIN METHODS ---
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Product");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = _userDAL.ValidateUser(model.Username, model.Password);

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("UserId", user.UserId.ToString()),
                    new Claim(ClaimTypes.Role, user.Role) // Stores "Admin" or "User" in the cookie
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = false, AllowRefresh = true };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Product");
            }

            ViewBag.Error = "Invalid Username or Password";
            return View(model);
        }

        // --- 2. LOGOUT ---
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // --- 3. SEEDER (RUN THIS TO FIX USER/1234) ---
        [HttpGet]
        [AllowAnonymous]
        public IActionResult SeedDatabase()
        {
            try 
            {
                // A. DELETE OLD DATA
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("DELETE FROM AppUser", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                // B. CREATE ADMIN (admin / 1234)
                _userDAL.RegisterUser("admin", "1234", "Admin");
                
                // C. CREATE USER (user / 1234)
                _userDAL.RegisterUser("user", "1234", "User");
                
                return Content("Database Reset! \n\nCreated: \nadmin / 1234 (Admin Role) \nuser / 1234 (User Role)");
            }
            catch (Exception ex)
            {
                return Content("Error Seeding: " + ex.Message);
            }
        }
    }
}