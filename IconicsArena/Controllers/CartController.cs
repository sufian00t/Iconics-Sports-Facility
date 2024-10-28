using IconicsArena.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IconicsArena.Controllers
{
    public class CartController : Controller
    {
        FootballArenaEntities db;

        public CartController()
        {
            this.db = new FootballArenaEntities();
        }

        public ActionResult GetCart()
        {
            if (Session["Id"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["Id"];
            var cart = db.Carts.FirstOrDefault(c => c.UserId == userId);

            return View(cart);
        }
        [HttpPost]
        public ActionResult AddToCart(int productId)
        {
            if (Session["Id"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            int userId = (int)Session["Id"];
            var cart = db.Carts.FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId, TotalAmount = 0 };
                db.Carts.Add(cart);
                db.SaveChanges();
            }

            var product = db.Products.Find(productId);
            if (product != null)
            {
                var cartItem = db.CartItems.FirstOrDefault(ci => ci.CartId == cart.CartId && ci.ProductId == productId);

                if (cartItem == null)
                {
                    cartItem = new CartItem
                    {
                        CartId = cart.CartId,
                        ProductId = productId,
                        Quantity = 1,
                        UnitPrice = product.Price
                    };
                    db.CartItems.Add(cartItem);
                }
                else
                {
                    cartItem.Quantity++;
                }

                db.SaveChanges();

                cart.TotalAmount = db.CartItems
                                    .Where(ci => ci.CartId == cart.CartId)
                                    .Sum(ci => (decimal?)(ci.Quantity * ci.UnitPrice) ?? 0);
                db.SaveChanges();

            }

            return RedirectToAction("GetProducts", "Product");
        }

        [HttpPost]
        public ActionResult UpdateQuantity(int cartItemId, int quantity)
        {
            var cartItem = db.CartItems.Find(cartItemId);

            if (cartItem != null)
            {
                var product = db.Products.Find(cartItem.ProductId);

                if (product != null)
                {
                    if (quantity > product.Stock)
                    {
                        return Json(new { success = false, message = "Cannot exceed available stock. Only " + product.Stock + " items in stock." });
                    }

                    if (quantity >= 1)
                    {
                        cartItem.Quantity = quantity;
                        db.SaveChanges();

                        var cart = db.Carts.Find(cartItem.CartId);
                        cart.TotalAmount = db.CartItems
                                           .Where(ci => ci.CartId == cart.CartId)
                                           .Sum(ci => ci.Quantity * ci.UnitPrice);
                        db.SaveChanges();
                        return Json(new { success = true, totalAmount = cart.TotalAmount });
                    }
                }

                return Json(new { success = false, message = "Invalid quantity." });
            }
            return Json(new { success = false, message = "Cart item not found." });

        }

        [HttpPost]
        public ActionResult RemoveFromCart(int itemId)
        {
            var cartItem = db.CartItems.Find(itemId);
            if (cartItem != null)
            {
                var cart = db.Carts.Find(cartItem.CartId);

                db.CartItems.Remove(cartItem);
                db.SaveChanges();

                if (cart != null)
                {
                    var remainingItems = db.CartItems.Where(ci => ci.CartId == cart.CartId).ToList();

                    if (remainingItems.Any())
                    {
                        cart.TotalAmount = remainingItems.Sum(ci => (decimal?)(ci.Quantity * ci.UnitPrice) ?? 0m);
                    }
                    else
                    {
                        cart.TotalAmount = 0;
                    }

                    db.SaveChanges();
                }

                return RedirectToAction("GetCart", "Cart");
            }

            return RedirectToAction("GetCart");
        }

    }
}