using IconicsArena.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;
using System.IO;
using System.Web.Mvc;
using System.Data.Entity;

namespace IconicsArena.Controllers
{
    public class ProductController : Controller
    {
        FootballArenaEntities db;

        public ProductController()
        {
            this.db = new FootballArenaEntities();
        }
        // GET: Product
        public ActionResult GetProducts(string sortOrder, int? categoryId, int? page)
        {
            int pageSize = 6;
            int pageNumber = (page ?? 1);

            
            var products = db.Products.AsQueryable();

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId);
            }

            
            switch (sortOrder)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                case "name_asc":
                    products = products.OrderBy(p => p.ProductName);
                    break;
                case "name_desc":
                    products = products.OrderByDescending(p => p.ProductName);
                    break;
                default:
                    products = products.OrderBy(p => p.ProductName);
                    break;
            }

            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentCategory = categoryId;

            ViewBag.Categories = db.Categories.ToList();

            return View(products.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult ShowProducts()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }
            var products = db.Products.Include(c => c.Category).ToList();
            return View(products);
        }

        public ActionResult AddProducts()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.Categories = new SelectList(db.Categories, "CategoryId", "CategoryName");
            return View();
        }

        [HttpPost]
        public ActionResult AddProducts(Product p, HttpPostedFileBase image)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                if (image != null && image.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(image.FileName);

                    var rootFolder = "~/Images/Picture/";

                    string categoryFolder = string.Empty;
                    if (p.CategoryId == 1)
                    {
                        categoryFolder = "Boot/";
                    }
                    else if (p.CategoryId == 2)
                    {
                        categoryFolder = "Cloth/";
                    }
                    else
                    {
                        categoryFolder = "Football/";
                    }
                    var savePath = Path.Combine(Server.MapPath(rootFolder + categoryFolder));
                    if (!Directory.Exists(savePath))
                    {
                        Directory.CreateDirectory(savePath);
                    }
                    var fullPath = Path.Combine(savePath, fileName);
                    image.SaveAs(fullPath);
                    p.ImagePath = Path.Combine(rootFolder, categoryFolder, fileName).Replace("\\", "/");
                }

                db.Products.Add(p);
                db.SaveChanges();
                return RedirectToAction("ShowProducts");
            }

            ViewBag.Categories = new SelectList(db.Categories, "CategoryId", "CategoryName");
            return View(p);
        }

        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            ViewBag.Categories = new SelectList(db.Categories, "CategoryId", "CategoryName", product.CategoryId);

            return View(product);
        }

        [HttpPost]
        public ActionResult EditProduct(Product p, HttpPostedFileBase image)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                var existingProduct = db.Products.Find(p.ProductId);

                if (existingProduct != null)
                {
                    existingProduct.ProductName = p.ProductName;
                    existingProduct.Price = p.Price;
                    existingProduct.CategoryId = p.CategoryId;
                    existingProduct.Stock = p.Stock;

                    if (image != null && image.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(image.FileName);

                        var rootFolder = "~/Images/Picture/";

                        string categoryFolder = string.Empty;
                        if (p.CategoryId == 1)
                        {
                            categoryFolder = "Boot/";
                        }
                        else if (p.CategoryId == 2)
                        {
                            categoryFolder = "Cloth/";
                        }
                        else
                        {
                            categoryFolder = "Football/";
                        }

                        var savePath = Path.Combine(Server.MapPath(rootFolder + categoryFolder));
                        if (!Directory.Exists(savePath))
                        {
                            Directory.CreateDirectory(savePath);
                        }
                        var fullPath = Path.Combine(savePath, fileName);
                        image.SaveAs(fullPath);

                        existingProduct.ImagePath = Path.Combine(rootFolder, categoryFolder, fileName).Replace("\\", "/");
                    }

                    db.Entry(existingProduct).State = EntityState.Modified;
                    db.SaveChanges();

                    return RedirectToAction("ShowProducts");
                }
            }

            ViewBag.Categories = new SelectList(db.Categories, "CategoryId", "CategoryName", p.CategoryId);
            return View(p);
        }

        [HttpPost]
        public ActionResult DeleteProduct(int id)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            db.Products.Remove(product);
            db.SaveChanges();

            return RedirectToAction("ShowProducts");
        }

    }
}