using BookingApp.Application.Common.Utility;
using BookingApp.Application.Services.Interface;
using BookingApp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace BookingApp.Web.Controllers
{
    public class ReservationController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IVillaService _villaService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IVillaNumberService _villaNumberService;
        public ReservationController(IReservationService reservationService,
            IVillaService villaService, IVillaNumberService villaNumberService,
            IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _villaService = villaService;
            _villaNumberService = villaNumberService;
            _reservationService = reservationService;
            _webHostEnvironment = webHostEnvironment;
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

            ApplicationUser user = _userManager.FindByIdAsync(userId).GetAwaiter().GetResult();
            var reservation = new Reservation
            {
                VillaId = villaId,
                Villa = _villaService.GetVillaById(villaId),
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
            var villa = _villaService.GetVillaById(reservation.VillaId);
            reservation.TotalCost = villa.Price * reservation.Nights;
            reservation.Status = SD.StatusPending;
            reservation.BookingDate = DateTime.Now;

            if (_villaService.IsVillaAvailableByDate(reservation.Id, reservation.Nights, reservation.CheckInDate))
            {
                TempData["error"] = "Room is not available!";
                return RedirectToAction(nameof(FinalizeReservation), new
                {
                    villaId = reservation.VillaId,
                    checkInDate = reservation.CheckInDate,
                    nights = reservation.Nights
                });
            }
            _reservationService.CreateReservation(reservation);

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

            _reservationService.UpdateStripePaymentId(reservation.Id, session.Id, session.PaymentIntentId);
            ReservationConfirmation(reservation.Id);
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
        [Authorize]
        public IActionResult ReservationConfirmation(int reservationId)
        {
            Reservation reservationFromDb = _reservationService.GetReservationById(reservationId);

            if (reservationFromDb.Status == SD.StatusPending)
            {
                var service = new SessionService();
                Session session = service.Get(reservationFromDb.StripeSessionId);

                if (session.PaymentStatus == "paid")
                {
                    _reservationService.UpdateStatus(reservationFromDb.Id, SD.StatusApproved, 0);
                    _reservationService.UpdateStripePaymentId(reservationFromDb.Id, session.Id, session.PaymentIntentId);
                }
            }
            return View(reservationId);
        }

        public IActionResult ReservationDetails(int reservationId)
        {
            Reservation reservation = _reservationService.GetReservationById(reservationId);

            if (reservation.VillaNumber == 0 && reservation.Status == SD.StatusApproved)
            {
                var availableVillaNumber = AssignAvailableVillaNumberByVilla(reservation.VillaId);
                reservation.VillaNumbers = _villaNumberService.GetAllVillaNumbers().Where(u => u.VillaId == reservation.VillaId
                && availableVillaNumber.Any(x => x == u.Villa_Number)).ToList();
            }
            return View(reservation);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckIn(Reservation reservation)
        {
            _reservationService.UpdateStatus(reservation.Id, SD.StatusCheckedIn, reservation.VillaNumber);
            TempData["success"] = "Reservation Updated Successfully!";
            return RedirectToAction(nameof(ReservationDetails), new { reservationId = reservation.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckOut(Reservation reservation)
        {
            _reservationService.UpdateStatus(reservation.Id, SD.StatusCompleted, reservation.VillaNumber);
            TempData["success"] = "Reservation Completed Successfully!";
            return RedirectToAction(nameof(ReservationDetails), new { reservationId = reservation.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CancelReservation(Reservation reservation)
        {
            _reservationService.UpdateStatus(reservation.Id, SD.StatusCancelled, 0);
            TempData["success"] = "Reservation Cancelled Successfully!";
            return RedirectToAction(nameof(ReservationDetails), new { reservationId = reservation.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        private List<int> AssignAvailableVillaNumberByVilla(int villaId)
        {
            List<int> availableVillaNumbers = new();

            var villaNumbers = _villaNumberService.GetAllVillaNumbers().Where(x => x.VillaId == villaId);

            var checkedInVilla = _reservationService.GetCheckedInVillaNumbers(villaId);

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
            string userId = "";

            if (string.IsNullOrEmpty(status))
            {
                status = "";
            }

            if (User.IsInRole(SD.Role_Admin))
            {

            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            }
            objReservations = _reservationService.GetAllReservations(userId, status);
           
            return Json(new { data = objReservations });
        }
        #endregion
    }
}
