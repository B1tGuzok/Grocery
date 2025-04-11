using Grocery.Models;
using System.Data.Entity;
using System.Web.Mvc;
using System;
using System.Linq;

[Authorize(Roles = "admin, cashier")]
public class SuppliersController : Controller
{
    private GroceryDBEntities db = new GroceryDBEntities();

    // GET: Suppliers
    public ActionResult Index(string sortOrder, string searchString)
    {
        ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewBag.CurrentFilter = searchString;

        IQueryable<Supplier> suppliers = db.Suppliers;

        if (!String.IsNullOrEmpty(searchString))
        {
            suppliers = suppliers.Where(s => s.Name.Contains(searchString));
        }

        switch (sortOrder)
        {
            case "name_desc":
                suppliers = suppliers.OrderByDescending(s => s.Name);
                break;
            default:
                suppliers = suppliers.OrderBy(s => s.Name);
                break;
        }

        return View(suppliers.ToList());
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(Supplier supplier)
    {
        if (ModelState.IsValid)
        {
            try
            {
                db.Suppliers.Add(supplier);
                db.SaveChanges();

                // Упрощаем ответ, оставляем только необходимые данные
                return Json(new
                {
                    success = true,
                    id = supplier.SupplierId,
                    name = supplier.Name,
                    address = supplier.Address,
                    phone = supplier.PhoneNumber,
                    email = supplier.Email
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Ошибка при добавлении поставщика: " + ex.Message
                });
            }
        }

        return Json(new
        {
            success = false,
            message = "Неверные данные формы"
        });
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit([Bind(Include = "SupplierId,Name,Address,PhoneNumber,Email")] Supplier supplier)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var existingSupplier = db.Suppliers.Find(supplier.SupplierId);
                if (existingSupplier == null)
                {
                    return Json(new { success = false, message = "Поставщик не найден" });
                }

                existingSupplier.Name = supplier.Name;
                existingSupplier.Address = supplier.Address;
                existingSupplier.PhoneNumber = supplier.PhoneNumber;
                existingSupplier.Email = supplier.Email;

                db.Entry(existingSupplier).State = EntityState.Modified;
                db.SaveChanges();

                // Упрощаем ответ для редактирования
                return Json(new
                {
                    success = true,
                    id = supplier.SupplierId,
                    name = supplier.Name,
                    address = supplier.Address,
                    phone = supplier.PhoneNumber,
                    email = supplier.Email
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        return Json(new
        {
            success = false,
            message = "Неверные данные формы"
        });
    }

    [Authorize(Roles = "admin")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id)
    {
        Supplier supplier = db.Suppliers.Find(id);
        if (supplier == null)
        {
            return Json(new { success = false, message = "Поставщик не найден" });
        }

        try
        {
            db.Suppliers.Remove(supplier);
            db.SaveChanges();
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
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