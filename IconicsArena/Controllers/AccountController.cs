using IconicsArena.Context;
using IconicsArena.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static IconicsArena.ViewModel.LoginVM;

namespace IconicsArena.Controllers
{
    public class AccountController : Controller
    {
        FootballArenaEntities db;

        public AccountController()
        {
            this.db = new FootballArenaEntities();
        }
        
        public ActionResult Login()
        {

            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginVM u)
        {
            if (ModelState.IsValid)
            {
                var data = db.Users.Where(x => x.Email.ToLower() == u.Email.ToLower() && x.PasswordHash == u.Password).FirstOrDefault();

                if(data != null && data.isBanned == true)
                {
                    ViewBag.DataBanned = 1;
                    TempData["BanMsg"] = "Your account is Banned. Contact to Support";
                    return RedirectToAction("Index", "Home");
                }
                else if (data != null)
                {
                    Session["Id"] = data.UserId;
                    Session["Email"] = data.Email;
                    Session["Role"] = data.Role;
                    Session["Name"] = data.Name;
                    return RedirectToAction("Index", "Home");
                }
                TempData["Invalid"] = "Invalid User";
                ModelState.Clear();
                return View();
            }
            ModelState.Clear();
            return View();
        }
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            TempData["Logout"] = "Logged out from the system successfully";

            return RedirectToAction("Index", "Home");
        }

        public ActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SignUp(SignUpVM u)
        {
            if (ModelState.IsValid)
            {
                if (db.Users.Any(x => x.Email == u.Email))
                {
                    ModelState.Clear();
                    TempData["exist"] = "User Already Exists. Try a Different E-mail.";
                    return View();
                }
                var user = new User
                {
                    Name = u.Name,
                    Email = u.Email,
                    PasswordHash = u.Password,
                    Address = u.Address,
                    Gender = u.Gender,
                    Role = "Customer"
                };
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        public ViewResult ClearLogin()
        {
            ModelState.Clear();
            return View("Login", "Account");
        }

        public ViewResult ClearSignUp()
        {
            ModelState.Clear();
            return View("SignUp", "Account");
        }

        public ActionResult ShowAccount()
        {

            return View();
        }

        public ActionResult EditAccount()
        {
            if (Session["Role"] == null || Session["Id"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            int userId = (int)Session["Id"];

            var user = db.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        [HttpPost]
        public ActionResult EditAccount(User updatedUser)
        {
            if (Session["Role"] == null || Session["Id"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            if (ModelState.IsValid)
            {
                int userId = (int)Session["Id"];
                var user = db.Users.FirstOrDefault(u => u.UserId == userId);

                if (user != null)
                {

                    user.Name = updatedUser.Name;
                    Session["Name"] = updatedUser.Name;
                    user.Email = updatedUser.Email;
                    user.PasswordHash = updatedUser.PasswordHash;
                    user.Address = updatedUser.Address;
                    user.Gender = updatedUser.Gender;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Profile updated successfully!";
                    return RedirectToAction("ShowAccount", "Account");
                }
            }

            return RedirectToAction("ShowAccount", "Account");
        }


    }
}