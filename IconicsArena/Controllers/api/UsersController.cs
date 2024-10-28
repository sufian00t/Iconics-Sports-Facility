using IconicsArena.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace IconicsArena.Controllers.api
{
    public class UsersController : ApiController
    {
        private FootballArenaEntities db = new FootballArenaEntities();

        // GET: api/Users
        [HttpGet]
        [Route("api/Users/GetUsers")]
        public IHttpActionResult GetUsers()
        {
            var users = db.Users.ToList();

            return Ok(users);
        }

        
        [HttpPut]
        [Route("api/Users/BanUnbanUser/{id}")]
        public IHttpActionResult BanUnbanUser(int id)
        {
            var user = db.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            user.isBanned = !user.isBanned;
            db.SaveChanges();

            return Ok(new { user.UserId, user.isBanned });
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UserExists(int id)
        {
            return db.Users.Count(e => e.UserId == id) > 0;
        }


    }
}
