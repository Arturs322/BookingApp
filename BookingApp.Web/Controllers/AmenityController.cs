using BookingApp.Application.Common.Interfaces;
using BookingApp.Application.Common.Utility;
using BookingApp.Application.Services.Interface;
using BookingApp.Domain.Entities;
using BookingApp.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Booking.Web.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class AmenityController : Controller
    {
        private readonly IAmenityService _amenityService;
        private readonly IVillaService _villaService;

        public AmenityController(IAmenityService amenityService, IVillaService villaService)
        {
            _amenityService = amenityService;
            _villaService = villaService;
        }
        public IActionResult Index()
        {
            var amenities = _amenityService.GetAllAmenities();
            return View(amenities);
        }
        public IActionResult Create()
        {
            var amenityVM = new AmenityVM
            {
                VillaList =
                 _villaService.GetAllVillas().Select(x => new SelectListItem
                 {
                     Text = x.Name,
                     Value = x.Id.ToString()
                 })
            };
            
            return View(amenityVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Amenity amenity)
        {
            if (ModelState.IsValid)
            {
                _amenityService.CreateAmenity(amenity);
                TempData["success"] = "Amenity Created Successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(amenity);
        }
        public IActionResult Update(int id)
        {
            var amenity = _amenityService.GetAmenityById(id);
            if (amenity == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var amenityVM = new AmenityVM
            {
                Amenity = amenity,
                VillaList =
          _villaService.GetAllVillas().Select(x => new SelectListItem
          {
              Text = x.Name,
              Value = x.Id.ToString()
          })
            };

            return View(amenityVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(AmenityVM amenityVM)
        {
            if (ModelState.IsValid)
            {
                _amenityService.UpdateAmenity(amenityVM.Amenity);
                TempData["success"] = "Amenity Updated Successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        public IActionResult Delete(int id)
        {
            var amenity = _amenityService.GetAmenityById(id);
            if (amenity == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var amenityVM = new AmenityVM
            {
                Amenity = amenity,
                VillaList =
          _villaService.GetAllVillas().Select(x => new SelectListItem
          {
              Text = x.Name,
              Value = x.Id.ToString()
          })
            }; 
            return View(amenityVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(AmenityVM amenityVM)
        {
            bool deleted = _amenityService.DeleteAmenity(amenityVM.Amenity.Id);

            if (deleted)
            {
                TempData["success"] = "Amenity Deleted Successfully!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["error"] = "Failed to delete Amenity!";
            }
            return View();
        }
    }
}
