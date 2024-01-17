using BookingApp.Application.Common.Interfaces;
using BookingApp.Application.Common.Utility;
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
        private readonly IUnitOfWork _unitOfWork;

        public AmenityController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var amenities = _unitOfWork.Amenity.GetAll(includeproperties: "Villa");
            return View(amenities);
        }
        public IActionResult Create()
        {
            var amenityVM = new AmenityVM
            {
                VillaList =
                 _unitOfWork.Villa.GetAll().Select(x => new SelectListItem
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
                _unitOfWork.Amenity.Add(amenity);
                _unitOfWork.Save();
                TempData["success"] = "Amenity Created Successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(amenity);
        }
        public IActionResult Update(int id)
        {
            var amenity = _unitOfWork.Amenity.Get(x => x.Id == id);
            if (amenity == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var amenityVM = new AmenityVM
            {
                Amenity = amenity,
                VillaList =
          _unitOfWork.Villa.GetAll().Select(x => new SelectListItem
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
                _unitOfWork.Amenity.Update(amenityVM.Amenity);
                _unitOfWork.Save();
                TempData["success"] = "Amenity Updated Successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        public IActionResult Delete(int id)
        {
            var amenity = _unitOfWork.Amenity.Get(x => x.Id == id);
            if (amenity == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var amenityVM = new AmenityVM
            {
                Amenity = amenity,
                VillaList =
          _unitOfWork.Villa.GetAll().Select(x => new SelectListItem
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
            var amenityToDelete = _unitOfWork.Amenity.Get(x => x.Id == amenityVM.Amenity.Id);

            if (amenityToDelete != null)
            {
                _unitOfWork.Amenity.Remove(amenityToDelete);
                _unitOfWork.Save();
                TempData["success"] = "Amenity Deleted Successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }
    }
}
