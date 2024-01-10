using BookingApp.Domain.Entities;
using BookingApp.Infrastructure.Data;
using BookingApp.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Booking.Web.Controllers
{
    public class VillaNumberController : Controller
    {
        private readonly ApplicationDbContext _db;

        public VillaNumberController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            var villaNumbers = _db.VillaNumbers.Include(x => x.Villa).ToList();
            return View(villaNumbers);
        }
        public IActionResult Create()
        {
            VillaNumberVM villaNumberVM = new()
            {
                VillaList = _db.Villas.ToList().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
            };

            return View(villaNumberVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(VillaNumberVM villaNumberVM)
        {
            var existingVillaNumber = _db.VillaNumbers.FirstOrDefault(x => x.Villa_Number == villaNumberVM.VillaNumber.Villa_Number);
            if(existingVillaNumber != null)
            {
                ModelState.AddModelError("VillaNumber.Villa_Number", "This Villa Number Already Exists!");
            }

            if (ModelState.IsValid)
            {
                _db.VillaNumbers.Add(villaNumberVM.VillaNumber);
                _db.SaveChanges();
                TempData["success"] = "Villa Created Successfully!";
                return RedirectToAction(nameof(Index));
            }
            villaNumberVM.VillaList =
                 _db.Villas.ToList().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                });
            
            return View(villaNumberVM);
        }
        public IActionResult Update(int villaId)
        {
            VillaNumberVM villaNumberVM = new()
            {
                VillaList = _db.Villas.ToList().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
                VillaNumber = _db.VillaNumbers.FirstOrDefault(x => x.Villa_Number == villaId),
            };

            if(villaNumberVM.VillaNumber == null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(villaNumberVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(VillaNumberVM villaNumberVM)
        {
            if (ModelState.IsValid)
            {
                _db.VillaNumbers.Update(villaNumberVM.VillaNumber);
                _db.SaveChanges();
                TempData["success"] = "Villa Updated Successfully!";
                return RedirectToAction(nameof(Index));
            }
            villaNumberVM.VillaList =
                 _db.Villas.ToList().Select(x => new SelectListItem
                 {
                     Text = x.Name,
                     Value = x.Id.ToString()
                 });

            return View(villaNumberVM);
        }

        public IActionResult Delete(int villaId)
        {
            VillaNumberVM villaNumberVM = new()
            {
                VillaList = _db.Villas.ToList().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
                VillaNumber = _db.VillaNumbers.FirstOrDefault(x => x.Villa_Number == villaId),
            };

            if (villaNumberVM.VillaNumber == null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(villaNumberVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(VillaNumberVM villaNumberVM)
        {
            VillaNumber? villaNumber = _db.VillaNumbers.FirstOrDefault(x => x.Villa_Number == villaNumberVM.VillaNumber.Villa_Number);
            if (villaNumber != null)
            {
                _db.VillaNumbers.Remove(villaNumber);
                _db.SaveChanges();
                TempData["success"] = "Villa Deleted Successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(villaNumberVM);
        }
    }
}
