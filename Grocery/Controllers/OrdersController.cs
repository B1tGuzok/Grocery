using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Grocery.Models;

namespace Grocery.Controllers
{
    [Authorize(Roles = "admin, cashier, supplier")]
    public class OrdersController : Controller
    {
        private GroceryDBEntities db = new GroceryDBEntities();

        // GET: Orders
        public ActionResult Index(string sortOrder, DateTime? searchOrderDate)
        {
            ViewBag.OrderDateSortParm = String.IsNullOrEmpty(sortOrder) ? "OrderDate_desc" : "";
            ViewBag.DeliveryDateSortParm = sortOrder == "DeliveryDate" ? "DeliveryDate_desc" : "DeliveryDate";

            ViewBag.CurrentOrderDateFilter = searchOrderDate;

            var orders = db.Orders.AsQueryable();

            if (searchOrderDate.HasValue)
            {
                // Сравниваем только дату, отбрасывая время
                orders = orders.Where(o => DbFunctions.TruncateTime(o.OrderDate) == DbFunctions.TruncateTime(searchOrderDate.Value));
            }

            switch (sortOrder)
            {
                case "OrderDate_desc":
                    orders = orders.OrderByDescending(o => o.OrderDate);
                    break;
                case "DeliveryDate":
                    orders = orders.OrderBy(o => o.DeliveryDate);
                    break;
                case "DeliveryDate_desc":
                    orders = orders.OrderByDescending(o => o.DeliveryDate);
                    break;
                default: // OrderDate ascending
                    orders = orders.OrderBy(o => o.OrderDate);
                    break;
            }

            return View(orders.ToList());
        }

        [Authorize(Roles = "admin, cashier")]
        // GET: Orders/Create
        public ActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "admin, cashier")]
        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "OrderDate,DeliveryDate")] Order order)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Orders.Add(order);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Произошла ошибка при добавлении заказа: " + ex.Message });
                }
            }

            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        [Authorize(Roles = "admin, cashier")]
        // GET: Orders/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        [Authorize(Roles = "admin, cashier")]
        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [Bind(Include = "OrderId,OrderDate,DeliveryDate")] Order order)
        {
            if (id != order.OrderId)
            {
                return Json(new { success = false }); // Несоответствие ID
            }

            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(order).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Произошла ошибка при обновлении заказа: " + ex.Message });
                }
            }

            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        [Authorize(Roles = "admin, cashier")]
        // POST: Orders/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int orderId)
        {
            Order order = db.Orders.Find(orderId);
            if (order == null)
            {
                return Json(new { success = false, message = "Заказ не найден." });
            }

            try
            {
                db.Orders.Remove(order);
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Произошла ошибка при удалении заказа: " + ex.Message });
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