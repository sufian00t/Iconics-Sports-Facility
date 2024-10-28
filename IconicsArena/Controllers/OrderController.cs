using IconicsArena.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
namespace IconicsArena.Controllers
{
    public class OrderController : Controller
    {
        FootballArenaEntities db;

        public OrderController()
        {
            this.db = new FootballArenaEntities();
        }
        // GET: Order

        public ActionResult Checkout()
        {
            if (Session["Id"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            
            int userId = (int)Session["Id"];
            var cart = db.Carts.FirstOrDefault(c => c.UserId == userId);

            
            if (cart == null || !cart.CartItems.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty. Please add items to the cart before checking out.";
                return RedirectToAction("GetCart", "Cart");
            }

            return View(cart);
        }

        [HttpPost]
        public ActionResult ConfirmOrder()
        {
            if (Session["Id"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["Id"];
            var cart = db.Carts.Include(c => c.CartItems.Select(ci => ci.Product)).FirstOrDefault(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty. Please add items to the cart before placing an order.";
                return RedirectToAction("GetCart", "Cart");
            }

            var newOrder = new Order
            {
                UserId = userId,
                CartId = cart.CartId,
                OrderDate = DateTime.Now,
                IsDelivered = false
            };

            db.Orders.Add(newOrder);
            db.SaveChanges();

            foreach (var cartItem in cart.CartItems)
            {
                var sale = new Sale
                {
                    OrderId = newOrder.OrderId,
                    CartId = cart.CartId,
                    ProductId = (int)cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice,
                    TotalAmount = (decimal)cart.TotalAmount
                };

                db.Sales.Add(sale);
            }

            db.SaveChanges();

            var cartItemsToRemove = cart.CartItems.ToList();
            db.CartItems.RemoveRange(cartItemsToRemove);
            db.SaveChanges();

            TempData["OrderId"] = newOrder.OrderId;
            TempData["SuccessMessage"] = "Your order has been placed successfully!";

            return RedirectToAction("OrderConfirmation");
        }

        public ActionResult OrderConfirmation()
        {
            if (Session["Id"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }
        public ActionResult BookingHistory()
        {
            if (Session["Role"] == null || Session["Id"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            int userId = (int)Session["Id"];

            var bookings = db.Bookings
                .Where(b => b.UserId == userId && b.IsBooked == true)
                .Include(b => b.Slot)
                .ToList();

            return View(bookings);
        }

        public ActionResult PurchaseHistory()
        {
            if (Session["Id"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["Id"];

            var orders = db.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Sales.Select(s => s.Product))
                .ToList();

            return View(orders);
        }

        public ActionResult Statistics()
        {
            if (Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

             var dailySales = db.Orders
                .Where(o => o.Cart != null && o.OrderDate.HasValue)
                .GroupBy(o => DbFunctions.TruncateTime(o.OrderDate.Value))
                .Select(g => new
                {
                    Date = g.Key,
                    TotalSales = g.Sum(o => o.Cart.TotalAmount)
                })
                .OrderBy(g => g.Date)
                .ToList();

             ViewBag.DailySales = dailySales;

            return View(dailySales);
        }
    }
}