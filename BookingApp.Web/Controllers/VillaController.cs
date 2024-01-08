using BookingApp.Domain.Entities;
using BookingApp.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace Booking.Web.Controllers
{
    public class VillaController : Controller
    {
        private readonly ApplicationDbContext _db;

        public VillaController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            var villas = _db.Villas.ToList();
            return View(villas);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Villa villa)
        {
            if (villa.Name == villa.Description)
            {
                ModelState.AddModelError("Name", "Name cannot exactly match Descirption!");
            }
            if (ModelState.IsValid)
            {
                _db.Villas.Add(villa);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(villa);
        }
    }
}
