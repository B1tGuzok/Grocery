using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Grocery.Models;

namespace Grocery.Controllers
{
    [Authorize(Roles = "admin, cashier, supplier")]
    public class PositionsInOrdersController : Controller
    {
        private GroceryDBEntities db = new GroceryDBEntities();

        // GET: PositionsInOrders
        public ActionResult Index(string sortOrder, string searchString)
        {
            ViewBag.QuantitySortParm = sortOrder == "Quantity" ? "quantity_desc" : "Quantity";
            ViewBag.CurrentFilter = searchString;

            ViewBag.OrderId = new SelectList(db.Orders, "OrderId", "OrderId");
            ViewBag.ProductId = new SelectList(db.Products, "ProductId", "ProductName");

            var positions = db.PositionsInOrders.Include(p => p.Order).Include(p => p.Product);

            if (!String.IsNullOrEmpty(searchString))
            {
                positions = positions.Where(p => p.Order.OrderId.ToString().Contains(searchString));
            }

            switch (sortOrder)
            {
                case "Quantity":
                    positions = positions.OrderBy(p => p.Quantity ?? int.MaxValue);
                    break;
                case "quantity_desc":
                    positions = positions.OrderByDescending(p => p.Quantity ?? int.MinValue);
                    break;
                default:
                    positions = positions.OrderBy(p => p.PositionId);
                    break;
            }

            return View(positions.ToList());
        }

        [Authorize(Roles = "admin, cashier")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductId,OrderId,Quantity")] PositionsInOrder position)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.PositionsInOrders.Add(position);
                    db.SaveChanges();

                    // Получаем связанные данные для ответа
                    var product = db.Products.Find(position.ProductId);
                    var order = db.Orders.Find(position.OrderId);

                    return Json(new
                    {
                        success = true,
                        positionId = position.PositionId,
                        orderId = order.OrderId,
                        productName = product.ProductName,
                        productId = product.ProductId,
                        quantity = position.Quantity
                    });
                }
                catch (Exception ex)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Ошибка при добавлении: " + ex.Message
                    });
                }
            }

            return Json(new
            {
                success = false,
                message = "Неверные данные формы"
            });
        }

        [Authorize(Roles = "admin, cashier")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [Bind(Include = "PositionId,ProductId,OrderId,Quantity")] PositionsInOrder position)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingPosition = db.PositionsInOrders.Find(id);
                    if (existingPosition == null)
                    {
                        return Json(new { success = false, message = "Позиция не найдена" });
                    }

                    // Обновляем данные
                    existingPosition.ProductId = position.ProductId;
                    existingPosition.OrderId = position.OrderId;
                    existingPosition.Quantity = position.Quantity;

                    db.Entry(existingPosition).State = EntityState.Modified;
                    db.SaveChanges();

                    // Получаем обновленные данные для ответа
                    var product = db.Products.Find(position.ProductId);
                    var order = db.Orders.Find(position.OrderId);

                    return Json(new
                    {
                        success = true,
                        positionId = position.PositionId,
                        orderId = order.OrderId,
                        productName = product.ProductName,
                        productId = product.ProductId,
                        quantity = position.Quantity
                    });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Ошибка при редактировании: " + ex.Message });
                }
            }

            return Json(new
            {
                success = false,
                message = "Неверные данные формы"
            });
        }

        [Authorize(Roles = "admin, cashier")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PositionsInOrder position = db.PositionsInOrders.Find(id);
            if (position == null)
            {
                return Json(new { success = false, message = "Позиция не найдена" });
            }

            try
            {
                db.PositionsInOrders.Remove(position);
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
}