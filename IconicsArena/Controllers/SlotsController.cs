using IconicsArena.Context;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IconicsArena.Controllers
{
    public class SlotsController : Controller
    {
        FootballArenaEntities db;

        public SlotsController()
        {
            this.db = new FootballArenaEntities();
        }
        // GET: Slots
        public ActionResult ShowSlots()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }
            else if (Session["Role"].ToString() == "Customer")
            {
                return RedirectToAction("Index", "Home");
            }
            var slots = db.Slots.ToList();

            return View(slots);
        }

        public ActionResult AddSlot()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }
            var timeSlots = new List<SelectListItem>
            {
             new SelectListItem { Value = TimeSlots.Slot1, Text = TimeSlots.Slot1 },
             new SelectListItem { Value = TimeSlots.Slot2, Text = TimeSlots.Slot2 },
             new SelectListItem { Value = TimeSlots.Slot3, Text = TimeSlots.Slot3 },
             new SelectListItem { Value = TimeSlots.Slot4, Text = TimeSlots.Slot4 },
             new SelectListItem { Value = TimeSlots.Slot5, Text = TimeSlots.Slot5 },
             new SelectListItem { Value = TimeSlots.Slot6, Text = TimeSlots.Slot6 },
            };
            var price = new List<SelectListItem>
            {
                new SelectListItem { Value = Price.Price1, Text = Price.Price1 },
                new SelectListItem { Value = Price.Price2, Text = Price.Price2 },
                new SelectListItem { Value = Price.Price3, Text = Price.Price3 },
                new SelectListItem { Value = Price.Price4, Text = Price.Price4 },
            };

            ViewBag.TimeSlots = timeSlots;
            ViewBag.Price = price;
            var s = new Slot
            {
                IsAvailable = true
            };

            return View();
        }

        [HttpPost]
        public ActionResult AddSlot(Slot slot)
        {
            if (ModelState.IsValid)
            {
                var existingSlot = db.Slots.FirstOrDefault(s => s.Day == slot.Day && s.Time == slot.Time);
                if (existingSlot != null)
                {
                    ModelState.AddModelError("", "A slot with the same day and time already exists.");
                    return RedirectToAction("AddSlot", "Slots");
                }
                decimal parsedPrice;
                if (decimal.TryParse(slot.Price.ToString(), out parsedPrice))
                {
                    slot.Price = parsedPrice;
                    db.Slots.Add(slot);
                    db.SaveChanges();
                    return RedirectToAction("ShowSlots");
                }
            }
            return View(slot);
        }

        public ActionResult EditSlot(int id)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var slot = db.Slots.FirstOrDefault(s => s.SlotId == id);
            if (slot == null)
            {
                return HttpNotFound();
            }
            var timeSlots = new List<SelectListItem>
            {
             new SelectListItem { Value = TimeSlots.Slot1, Text = TimeSlots.Slot1 },
             new SelectListItem { Value = TimeSlots.Slot2, Text = TimeSlots.Slot2 },
             new SelectListItem { Value = TimeSlots.Slot3, Text = TimeSlots.Slot3 },
             new SelectListItem { Value = TimeSlots.Slot4, Text = TimeSlots.Slot4 },
             new SelectListItem { Value = TimeSlots.Slot5, Text = TimeSlots.Slot5 },
             new SelectListItem { Value = TimeSlots.Slot6, Text = TimeSlots.Slot6 },
            };
            var price = new List<SelectListItem>
            {
             new SelectListItem { Value = Price.Price1, Text = Price.Price1 },
             new SelectListItem { Value = Price.Price2, Text = Price.Price2 },
             new SelectListItem { Value = Price.Price3, Text = Price.Price3 },
             new SelectListItem { Value = Price.Price4, Text = Price.Price4 },
            };

            ViewBag.TimeSlots = timeSlots;
            ViewBag.Price = price;

            return View(slot);
        }

        [HttpPost]
        public ActionResult EditSlot(Slot slot)
        {
            if (ModelState.IsValid)
            {
                db.Entry(slot).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ShowSlots");
            }

            return View(slot);
        }

        [HttpPost]
        public ActionResult DeleteSlot(int id)
        {
            var slot = db.Slots.FirstOrDefault(s => s.SlotId == id);
            if (slot != null)
            {
                db.Slots.Remove(slot);
                db.SaveChanges();
            }

            return RedirectToAction("ShowSlots");
        }

        public ActionResult RemoveSlots()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }
            else if (Session["Role"].ToString() == "Customer")
            {
                return RedirectToAction("Index", "Home");
            }
            var bookings = db.Bookings
                             .Include(b => b.User)
                             .Include(b => b.Slot)
                             .Where(b => b.IsBooked == true)
                             .ToList();

            return View(bookings);
        }

        [HttpPost]
        public ActionResult RemoveSlots(int bookingId)
        {
            var booking = db.Bookings.FirstOrDefault(b => b.BookingId == bookingId && b.IsBooked == true);

            if (booking != null)
            {
                var slot = db.Slots.FirstOrDefault(s => s.SlotId == booking.SlotId);

                if (slot != null)
                {
                    slot.IsAvailable = true;
                }

                db.Bookings.Remove(booking);
                db.SaveChanges();
            }
            return RedirectToAction("RemoveSlots");
        }

        [HttpGet]
        public ActionResult BookSlots(string day)
        {
            if (Session["Email"] != null && Session["Role"]?.ToString() == "Customer")
            {
                var availableSlots = db.Slots.Where(s => s.IsAvailable == true && !db.Bookings.Any(b => b.SlotId == s.SlotId && b.IsBooked == true)).ToList();

                return View(availableSlots);
            }
            TempData["AuthBooking"] = "You Must Login First";
            return RedirectToAction("Login", "Account");


        }

        [HttpPost]
        public ActionResult BookSlot(int slotid)
        {
            var slot = db.Slots.FirstOrDefault(x => x.SlotId == slotid);
            var data = new Booking
            {

                SlotId = slotid,
                UserId = (int?)Session["Id"],
                IsBooked = true
            };
            db.Bookings.Add(data);
            slot.IsAvailable = false;
            db.Entry(slot).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("BookingHistory", "Order");
        }


    }
}