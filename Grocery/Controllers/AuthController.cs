using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Grocery.Models;
using Microsoft.Owin.Security;

namespace Grocery.Controllers
{
    public class AuthController : Controller
    {
        private GroceryDBEntities db = new GroceryDBEntities();

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string login, string password, string returnUrl)
        {
            User user = db.Users.FirstOrDefault(u => u.Login == login && u.Password == password);
            if (user != null)
            {
                ClaimsIdentity claims = new ClaimsIdentity("ApplicationCookie");
                claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()));
                claims.AddClaim(new Claim(ClaimTypes.Name, user.Login));

                // Получаем роль пользователя из базы данных
                var userRole = db.Roles.Find(user.RoleId);
                if (userRole != null)
                {
                    claims.AddClaim(new Claim(ClaimTypes.Role, userRole.RoleName));
                }

                AuthenticationManager.SignOut("ApplicationCookie");
                AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = false }, claims);

                if (string.IsNullOrEmpty(returnUrl))
                    return RedirectToAction("Index", "Home");
                else
                    return Redirect(returnUrl);
            }
            else
            {
                ModelState.AddModelError("", "Неверный логин или пароль.");
                return View();
            }
        }

        public ActionResult Logout()
        {
            AuthenticationManager.SignOut("ApplicationCookie");
            return RedirectToAction("Index", "Home");
        }
    }
}