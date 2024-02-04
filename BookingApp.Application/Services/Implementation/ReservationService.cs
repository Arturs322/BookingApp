using BookingApp.Application.Common.Interfaces;
using BookingApp.Application.Common.Utility;
using BookingApp.Application.Services.Interface;
using BookingApp.Domain.Entities;
using Microsoft.AspNetCore.Hosting;

namespace BookingApp.Application.Services.Implementation
{
    public class ReservationService : IReservationService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ReservationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public void CreateReservation(Reservation reservation)
        {
            _unitOfWork.Reservation.Add(reservation);
            _unitOfWork.Save();
        }

        public IEnumerable<Reservation> GetAllReservations(string userId = "", string statusFilterList = "")
        {
            IEnumerable<string> statusList = statusFilterList.ToLower().Split(",");
            if (!string.IsNullOrEmpty(statusFilterList) && !string.IsNullOrEmpty(userId))
            {
                return _unitOfWork.Reservation.GetAll(u => statusList.Contains(u.Status.ToLower()) &&
                u.UserId == userId, includeproperties: "User,Villa");
            }
            else
            {
                if (!string.IsNullOrEmpty(statusFilterList))
                {
                    return _unitOfWork.Reservation.GetAll(u => statusList.Contains(u.Status.ToLower()), includeproperties: "User,Villa");
                }
                if (!string.IsNullOrEmpty(userId))
                {
                    return _unitOfWork.Reservation.GetAll(u => u.UserId == userId, includeproperties: "User,Villa");
                }
            }
            return _unitOfWork.Reservation.GetAll(includeproperties: "User, Villa");
        }

        public IEnumerable<int> GetCheckedInVillaNumbers(int villaId)
        {
            return _unitOfWork.Reservation.GetAll(x => x.VillaId == villaId && x.Status == SD.StatusCheckedIn)
                           .Select(x => x.VillaNumber);
        }

        public Reservation GetReservationById(int reservationId)
        {
            return _unitOfWork.Reservation.Get(u => u.Id == reservationId, includeproperties: "User, Villa");
        }
        public void UpdateStatus(int reservationId, string reservationStatus, int villaNumber = 0)
        {
            var reservation = _unitOfWork.Reservation.Get(x => x.Id == reservationId, tracked: true);
            if (reservation != null)
            {
                reservation.Status = reservationStatus;
                if (reservationStatus == SD.StatusPending)
                {
                    reservation.VillaNumber = villaNumber;
                    reservation.ActualCheckInDate = DateTime.Now;
                }
                if (reservationStatus == SD.StatusCompleted)
                {
                    reservation.ActualCheckOutDate = DateTime.Now;
                }
            }
            _unitOfWork.Save();
        }

        public void UpdateStripePaymentId(int reservationId, string sessionId, string paymentIntentId)
        {
            var reservation = _unitOfWork.Reservation.Get(x => x.Id == reservationId, tracked: true);
            if (reservation != null)
            {
                if (!string.IsNullOrEmpty(sessionId))
                {
                    reservation.StripeSessionId = sessionId;
                }
                if (!string.IsNullOrEmpty(paymentIntentId))
                {
                    reservation.StripePaymentIntentId = paymentIntentId;
                    reservation.PaymentDate = DateTime.Now;
                    reservation.IsPaymentSuccessful = true;
                }
            }
            _unitOfWork.Save();
        }
    }
}
