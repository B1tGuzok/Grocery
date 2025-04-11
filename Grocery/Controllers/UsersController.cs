using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Grocery.Models;

namespace Grocery.Controllers
{
    [Authorize(Roles = "admin")]
    public class UsersController : Controller
    {
        private GroceryDBEntities db = new GroceryDBEntities();

        // GET: Users
        public ActionResult Index(string sortOrder, string searchString)
        {
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.SurnameSortParm = sortOrder == "Surname" ? "surname_desc" : "Surname";
            ViewBag.RoleSortParm = sortOrder == "Role" ? "role_desc" : "Role";
            ViewBag.CurrentUserFilter = searchString;

            var users = db.Users.Include(u => u.Role).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => u.Surname.Equals(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    users = users.OrderByDescending(u => u.Name);
                    break;
                case "Surname":
                    users = users.OrderBy(u => u.Surname);
                    break;
                case "surname_desc":
                    users = users.OrderByDescending(u => u.Surname);
                    break;
                case "Role":
                    users = users.OrderBy(u => u.Role.RoleName);
                    break;
                case "role_desc":
                    users = users.OrderByDescending(u => u.Role.RoleName);
                    break;
                default:
                    users = users.OrderBy(u => u.Name);
                    break;
            }

            ViewBag.RoleId = new SelectList(db.Roles, "RoleId", "RoleName");
            return View(users.ToList());
        }

        // GET: Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            ViewBag.RoleId = new SelectList(db.Roles, "RoleId", "RoleName");
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "UserId,RoleId,Login,Name,Surname,Password")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.RoleId = new SelectList(db.Roles, "RoleId", "RoleName", user.RoleId);
            return View(user);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            ViewBag.RoleId = new SelectList(db.Roles, "RoleId", "RoleName", user.RoleId);
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [Bind(Include = "UserId,RoleId,Login,Name,Surname,Password")] User user)
        {
            if (id != user.UserId)
            {
                return Json(new { success = false, message = "Некорректный ID пользователя." });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    var roleName = db.Roles.Find(user.RoleId)?.RoleName;
                    return Json(new { success = true, roleName = roleName });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Произошла ошибка при обновлении пользователя: " + ex.Message });
                }
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // GET: Users/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            User user = db.Users.Find(id);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            try
            {
                db.Users.Remove(user);
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Произошла ошибка при удалении пользователя: " + ex.Message });
            }
        }


        // POST: Users/Delete/5

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}