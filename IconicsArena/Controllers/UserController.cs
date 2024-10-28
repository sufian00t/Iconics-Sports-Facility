using IconicsArena.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace IconicsArena.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        public ActionResult GetUsers()
        {
            if (Session["Role"].ToString() != "Admin")
            {
              return RedirectToAction("Login", "Account");
            }
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:64921/api/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.GetAsync("Users/GetUsers").Result;

                if (response.IsSuccessStatusCode)
                {
                    var users = response.Content.ReadAsAsync<IEnumerable<User>>().Result;
                    return View(users);
                }
                else
                {
                    return View("Error");
                }
            }
        }

        public ActionResult BanUnbanUser(int id)
        {
            if (Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:64921/api/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                
                HttpResponseMessage response = client.PutAsync($"Users/BanUnbanUser/{id}", null).Result;

                if (response.IsSuccessStatusCode)
                {
                    
                    return RedirectToAction("GetUsers");
                }
                else
                {
                    return View("Error");
                }
            }
        }
    }
}