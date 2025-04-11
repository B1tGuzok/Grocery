using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Grocery.Models;
using System.Linq.Expressions;
using Microsoft.AspNet.Identity;

namespace Grocery.Controllers
{
        [Authorize(Roles = "admin, cashier")]
    public class ProductsController : Controller
    {
        private GroceryDBEntities db = new GroceryDBEntities();
        public ProductsController(GroceryDBEntities context)
        {
            db = context;
        }
        // GET: Products
        public ActionResult Index(string sortOrder, string searchString)
        {
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.QuantitySortParm = sortOrder == "Quantity" ? "quantity_desc" : "Quantity";
            ViewBag.ShelfSortParm = sortOrder == "ShelfNumber" ? "shelf_desc" : "ShelfNumber";
            ViewBag.SupplierSortParm = sortOrder == "Supplier" ? "supplier_desc" : "Supplier";
            ViewBag.CurrentFilter = searchString;
            ViewBag.SupplierId = new SelectList(db.Suppliers, "SupplierId", "Name");

            var products = db.Products.Include(p => p.Supplier); // Убрали .AsQueryable()

            if (!String.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.ProductName.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    products = products.OrderByDescending(p => p.ProductName);
                    break;
                case "Quantity":
                    products = products.OrderBy(p => p.Quantity);
                    break;
                case "quantity_desc":
                    products = products.OrderByDescending(p => p.Quantity);
                    break;
                case "ShelfNumber":
                    products = products.OrderBy(p => p.ShelfNumber);
                    break;
                case "shelf_desc":
                    products = products.OrderByDescending(p => p.ShelfNumber);
                    break;
                case "Supplier":
                    products = products.OrderBy(p => p.Supplier.Name);
                    break;
                case "supplier_desc":
                    products = products.OrderByDescending(p => p.Supplier.Name);
                    break;
                default:
                    products = products.OrderBy(p => p.ProductName);
                    break;
            }

            return View(products.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Include(p => p.Supplier).FirstOrDefault(p => p.ProductId == id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "SupplierId,ProductName,Quantity,ShelfNumber")] Product product)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Products.Add(product);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Ошибка при добавлении продукта: " + ex.Message
                    });
                }
            }

            return Json(new
            {
                success = false,
                errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [Bind(Include = "ProductId,SupplierId,ProductName,Quantity,ShelfNumber")] Product product)
        {
            if (id != product.ProductId)
            {
                return Json(new { success = false, message = "Несоответствие ID" });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = db.Products.Find(id);
                    if (existingProduct == null)
                    {
                        return Json(new { success = false, message = "Товар не найден" });
                    }

                    // Обновляем поля
                    existingProduct.ProductName = product.ProductName;
                    existingProduct.Quantity = product.Quantity;
                    existingProduct.ShelfNumber = product.ShelfNumber;
                    existingProduct.SupplierId = product.SupplierId;

                    db.Entry(existingProduct).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    // Получаем обновленные данные поставщика
                    var supplier = db.Suppliers.Find(product.SupplierId);

                    return Json(new
                    {
                        success = true,
                        productName = product.ProductName,
                        quantity = product.Quantity,
                        shelfNumber = product.ShelfNumber,
                        supplierName = supplier?.Name,
                        supplierId = product.SupplierId
                    });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
            }

            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new { success = false, errors = errors });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return Json(new { success = false, message = "Продукт не найден." });
            }

            try
            {
                db.Products.Remove(product);
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Ошибка при удалении продукта: " + ex.Message });
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