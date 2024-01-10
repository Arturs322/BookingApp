using BookingApp.Application.Common.Interfaces;
using BookingApp.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Booking.Web.Controllers
{
    public class VillaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public VillaController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var villas = _unitOfWork.Villa.GetAll();
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
                _unitOfWork.Villa.Add(villa);
                _unitOfWork.Save();
                TempData["success"] = "Villa Created Successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(villa);
        }
        public IActionResult Update(int villaId)
        {
            var villa = _unitOfWork.Villa.Get(x => x.Id == villaId);
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
                _unitOfWork.Villa.Update(villa);
                _unitOfWork.Save();
                TempData["success"] = "Villa Updated Successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        public IActionResult Delete(int villaId)
        {
            var villa = _unitOfWork.Villa.Get(x => x.Id == villaId);
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
            var villaToDelete = _unitOfWork.Villa.Get(x => x.Id == villa.Id);

            if (villaToDelete != null)
            {
                _unitOfWork.Villa.Remove(villaToDelete);
                _unitOfWork.Save();
                TempData["success"] = "Villa Deleted Successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }
    }
}
