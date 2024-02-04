using BookingApp.Application.Common.Interfaces;
using BookingApp.Application.Services.Interface;
using BookingApp.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Booking.Web.Controllers
{
    public class VillaController : Controller
    {
        private readonly IVillaService _villaService;

        public VillaController(IVillaService villaService)
        {
            _villaService = villaService;
        }

        public IActionResult Index()
        {
            var villas = _villaService.GetAllVillas();
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
                _villaService.CreateVilla(villa);
                TempData["success"] = "Villa Created Successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(villa);
        }
        public IActionResult Update(int villaId)
        {
            var villa = _villaService.GetVillaById(villaId);
            if (villa == null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(villa);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(Villa villa)
        {
            if (ModelState.IsValid)
            {
                _villaService.UpdateVilla(villa);
                TempData["success"] = "Villa Updated Successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        public IActionResult Delete(int villaId)
        {
            var villa = _villaService.GetVillaById(villaId);
            if (villa == null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(villa);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Villa villa)
        {
            bool deleted = _villaService.DeleteVilla(villa.Id);

            if (deleted)
            {
                TempData["success"] = "Villa Deleted Successfully!";
                return RedirectToAction(nameof(Index));
            } else
            {
                TempData["error"] = "Failed to delete Villa!";
            }
            return View();
        }
    }
}
