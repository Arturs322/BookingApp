using BookingApp.Application.Common.Interfaces;
using BookingApp.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BookingApp.Web.Controllers
{
    public class ReservationController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ReservationController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult FinalizeReservation(int villaId, DateOnly checkInDate, int nights)
        {
            var reservation = new Reservation
            {
                VillaId = villaId,
                Villa = _unitOfWork.Villa.Get(x => x.Id == villaId, includeproperties: "VillaAmenity"),
                CheckInDate = checkInDate,
                CheckOutDate = checkInDate.AddDays(nights),
                Nights = nights
            };
            reservation.TotalCost = reservation.Villa.Price * nights;
            return View(reservation);
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
