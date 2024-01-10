using BookingApp.Application.Common.Interfaces;
using BookingApp.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Booking.Web.Controllers
{
    public class VillaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public VillaController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
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
                if(villa.Image != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\VillaImages");

                    using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
                    villa.Image.CopyTo(fileStream);
                    villa.ImageUrl = @"\images\VillaImages\" + fileName;
                } 
                else
                {
                    villa.ImageUrl = "https://placehold.co/600x400";
                }

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
                if (villa.Image != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\VillaImages");

                    if(!string.IsNullOrEmpty(villa.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, villa.ImageUrl.TrimStart('\\'));

                        if(System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
                    villa.Image.CopyTo(fileStream);
                    villa.ImageUrl = @"\images\VillaImages\" + fileName;
                }

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
                if (!string.IsNullOrEmpty(villaToDelete.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, villaToDelete.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                _unitOfWork.Villa.Remove(villaToDelete);
                _unitOfWork.Save();
                TempData["success"] = "Villa Deleted Successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }
    }
}
