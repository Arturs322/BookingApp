using BookingApp.Application.Common.Interfaces;
using BookingApp.Domain.Entities;
using BookingApp.Infrastructure.Data;
using BookingApp.Infrastructure.Repository;
using BookingApp.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Booking.Web.Controllers
{
    public class VillaNumberController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public VillaNumberController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var villaNumbers = _unitOfWork.VillaNumber.GetAll(includeproperties: "Villa");
            return View(villaNumbers);
        }
        public IActionResult Create()
        {
            VillaNumberVM villaNumberVM = new()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(x => new SelectListItem
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
            var existingVillaNumber = _unitOfWork.VillaNumber.Get(x => x.Villa_Number == villaNumberVM.VillaNumber.Villa_Number);
            if(existingVillaNumber != null)
            {
                ModelState.AddModelError("VillaNumber.Villa_Number", "This Villa Number Already Exists!");
            }

            if (ModelState.IsValid)
            {
                _unitOfWork.VillaNumber.Add(villaNumberVM.VillaNumber);
                _unitOfWork.Save();
                TempData["success"] = "Villa Created Successfully!";
                return RedirectToAction(nameof(Index));
            }
            villaNumberVM.VillaList =
                 _unitOfWork.Villa.GetAll().Select(x => new SelectListItem
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
                VillaList = _unitOfWork.Villa.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
                VillaNumber = _unitOfWork.VillaNumber.Get(x => x.Villa_Number == villaId),
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
                _unitOfWork.VillaNumber.Update(villaNumberVM.VillaNumber);
                _unitOfWork.Save();
                TempData["success"] = "Villa Updated Successfully!";
                return RedirectToAction(nameof(Index));
            }
            villaNumberVM.VillaList =
                 _unitOfWork.Villa.GetAll().Select(x => new SelectListItem
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
                VillaList = _unitOfWork.Villa.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
                VillaNumber = _unitOfWork.VillaNumber.Get(x => x.Villa_Number == villaId),
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
            VillaNumber? villaNumber = _unitOfWork.VillaNumber.Get(x => x.Villa_Number == villaNumberVM.VillaNumber.Villa_Number);
            if (villaNumber != null)
            {
                _unitOfWork.VillaNumber.Remove(villaNumber);
                _unitOfWork.Save();
                TempData["success"] = "Villa Deleted Successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(villaNumberVM);
        }
    }
}
