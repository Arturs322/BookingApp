using BookingApp.Application.Common.Interfaces;
using BookingApp.Application.Common.Utility;
using BookingApp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe.Checkout;
using System.Security.Claims;

namespace BookingApp.Web.Controllers
{
    public class ReservationController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ReservationController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        public IActionResult FinalizeReservation(int villaId, DateOnly checkInDate, int nights)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var user = _unitOfWork.ApplicationUser.Get(x => x.Id == userId);
            var reservation = new Reservation
            {
                VillaId = villaId,
                Villa = _unitOfWork.Villa.Get(x => x.Id == villaId, includeproperties: "VillaAmenity"),
                CheckInDate = checkInDate,
                CheckOutDate = checkInDate.AddDays(nights),
                Nights = nights,
                Phone = user.PhoneNumber,
                Email = user.Email,
                Name = user.Name,
                UserId = userId,
            };
            reservation.TotalCost = reservation.Villa.Price * nights;
            return View(reservation);
        }
        [Authorize]
        [HttpPost]
        public IActionResult FinalizeReservation(Reservation reservation)
        {
            var villa = _unitOfWork.Villa.Get(x => x.Id == reservation.VillaId);
            reservation.TotalCost = villa.Price * reservation.Nights;
            reservation.Status = SD.StatusPending;
            reservation.BookingDate = DateTime.Now;


            var villaNumberList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Reservation.GetAll(x => x.Status == SD.StatusApproved || x.Status == SD.StatusCheckedIn).ToList();

            int roomAvailable = SD.VillaRoomsAvailable_Count
                (villa.Id, villaNumberList, reservation.CheckInDate, reservation.Nights, bookedVillas);

            if(roomAvailable == 0)
            {
                TempData["error"] = "Room is not available!";
                return RedirectToAction(nameof(FinalizeReservation), new
                {
                    villaId = reservation.VillaId,
                    checkInDate = reservation.CheckInDate,
                    nights = reservation.Nights
                });
            }
            _unitOfWork.Reservation.Add(reservation);
            _unitOfWork.Save();

            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions()
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"reservation/ReservationConfirmation?reservationId={reservation.Id}",
                CancelUrl = domain + $"reservation/FinalizeReservation?villaId={reservation.VillaId}&checkInDate={reservation.CheckInDate}&nights={reservation.Nights}",
            };

            options.LineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(reservation.TotalCost * 100),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = villa.Name,
                        Description = villa.Description,
                    }
                },
                Quantity = 1,
            });


            var service = new SessionService();
            Session session = service.Create(options);

            _unitOfWork.Reservation.UpdateStripePaymentId(reservation.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
        [Authorize]
        public IActionResult ReservationConfirmation(int reservationId)
        {
            Reservation reservationFromDb = _unitOfWork.Reservation.Get(u => u.Id == reservationId,
            includeproperties: "User,Villa");

            if(reservationFromDb.Status == SD.StatusPending)
            {
                var service = new SessionService();
                Session session = service.Get(reservationFromDb.StripeSessionId);

                if(session.PaymentStatus == "paid")
                {
                    _unitOfWork.Reservation.UpdateStatus(reservationFromDb.Id, SD.StatusApproved, 0);
                    _unitOfWork.Reservation.UpdateStripePaymentId(reservationFromDb.Id, session.Id, session.PaymentIntentId);
                    _unitOfWork.Save();
                }
            }
            return View(reservationId);
        }

        public IActionResult ReservationDetails(int reservationId)
        {
            Reservation reservation = _unitOfWork.Reservation.
                Get(u => u.Id == reservationId, includeproperties: "User,Villa");

            if(reservation.VillaNumber == 0 && reservation.Status == SD.StatusApproved)
            {
                var availableVillaNumber = AssignAvailableVillaNumberByVilla(reservation.VillaId);
                reservation.VillaNumbers = _unitOfWork.VillaNumber.GetAll(u => u.VillaId == reservation.VillaId
                && availableVillaNumber.Any(x => x == u.Villa_Number)).ToList();
            }
            return View(reservation);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckIn(Reservation reservation)
        {
            _unitOfWork.Reservation.UpdateStatus(reservation.Id, SD.StatusCheckedIn, reservation.VillaNumber);
            _unitOfWork.Save();
            TempData["success"] = "Reservation Updated Successfully!";
            return RedirectToAction(nameof(ReservationDetails), new { reservationId = reservation.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckOut(Reservation reservation)
        {
            _unitOfWork.Reservation.UpdateStatus(reservation.Id, SD.StatusCompleted, reservation.VillaNumber);
            _unitOfWork.Save();
            TempData["success"] = "Reservation Completed Successfully!";
            return RedirectToAction(nameof(ReservationDetails), new { reservationId = reservation.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CancelReservation(Reservation reservation)
        {
            _unitOfWork.Reservation.UpdateStatus(reservation.Id, SD.StatusCancelled, 0);
            _unitOfWork.Save();
            TempData["success"] = "Reservation Cancelled Successfully!";
            return RedirectToAction(nameof(ReservationDetails), new { reservationId = reservation.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        private List<int> AssignAvailableVillaNumberByVilla(int villaId)
        {
            List<int> availableVillaNumbers = new();

            var villaNumbers = _unitOfWork.VillaNumber.GetAll(x => x.VillaId == villaId);

            var checkedInVilla = _unitOfWork.Reservation.GetAll(x => x.VillaId == villaId && x.Status == SD.StatusCheckedIn)
                .Select(x => x.VillaNumber);

            foreach (var villaNumber in villaNumbers)
            {
                if (!checkedInVilla.Contains(villaNumber.Villa_Number))
                {
                    availableVillaNumbers.Add(villaNumber.Villa_Number);
                }
            }

            return availableVillaNumbers;
        }
        #region API CALLS
        [HttpGet]
        [Authorize]
        public IActionResult GetAll(string status)
        {
            IEnumerable<Reservation> objReservations;

            if(User.IsInRole(SD.Role_Admin))
            {
                objReservations = _unitOfWork.Reservation.GetAll(includeproperties: "User,Villa");
            } else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                objReservations = _unitOfWork.Reservation.
                    GetAll(u => u.UserId == userId, includeproperties: "User,Villa");
            }
            if(!string.IsNullOrEmpty(status))
            {
                objReservations = objReservations.Where(u => u.Status.ToLower().Equals(status.ToLower()));
            }
            return Json(new {data = objReservations});
        }
        #endregion
    }
}
