// Grocery.Controllers.RolesController.cs
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Grocery.Models;

namespace Grocery.Controllers
{
    [Authorize(Roles = "admin")]
    public class RolesController : Controller
    {
        private GroceryDBEntities db = new GroceryDBEntities();

        public ActionResult Index(string searchString)
        {
            var roles = db.Roles.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                roles = roles.Where(r => r.RoleName.Contains(searchString));
            }

            ViewBag.CurrentFilter = searchString;
            return View(roles.ToList());
        }

        // GET: Roles/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "RoleName,ImagePath")] Role role)
        {
            if (ModelState.IsValid)
            {
                db.Roles.Add(role);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(role);
        }

        // GET: Roles/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Role role = db.Roles.Find(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            return View(role);
        }

        // POST: Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [Bind(Include = "RoleId,RoleName,ImagePath")] Role role)
        {
            if (id != role.RoleId)
            {
                return Json(new { success = false, message = "Некорректный ID роли." });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(role).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Произошла ошибка при обновлении роли: " + ex.Message });
                }
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // GET: Roles/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Role role = db.Roles.Find(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            return View(role);
        }

        // POST: Roles/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            Role role = db.Roles.Find(id);
            if (role == null)
            {
                return Json(new { success = false, message = "Роль не найдена." });
            }

            try
            {
                db.Roles.Remove(role);
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Произошла ошибка при удалении роли: " + ex.Message });
            }
        }

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