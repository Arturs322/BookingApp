using BookingApp.Application.Common.Interfaces;
using BookingApp.Application.Common.Utility;
using BookingApp.Domain.Entities;
using BookingApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookingApp.Infrastructure.Repository
{
    public class ReservationRepository : Repository<Reservation>, IReservationRepository
    {
        private readonly ApplicationDbContext _db;

        public ReservationRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Reservation entity)
        {
            _db.Reservations.Update(entity);
        }

        public void UpdateStatus(int reservationId, string reservationStatus, int villaNumber = 0)
        {
            var reservation = _db.Reservations.FirstOrDefault(x => x.Id == reservationId);
            if(reservation != null)
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
            
        }

        public void UpdateStripePaymentId(int reservationId, string sessionId, string paymentIntentId)
        {
            var reservation = _db.Reservations.FirstOrDefault(x => x.Id == reservationId);
            if (reservation != null)
            {
                if(!string.IsNullOrEmpty(sessionId))
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
        }
    }
}
