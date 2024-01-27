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
                    _unitOfWork.Reservation.UpdateStatus(reservationFromDb.Id, SD.StatusApproved);
                    _unitOfWork.Reservation.UpdateStripePaymentId(reservationFromDb.Id, session.Id, session.PaymentIntentId);
                    _unitOfWork.Save();
                }
            }
            return View(reservationId);
        }
        public IActionResult Index()
        {
            return View();
        }

    }
}
