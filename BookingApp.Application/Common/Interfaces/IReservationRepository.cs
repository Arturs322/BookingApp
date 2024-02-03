using BookingApp.Domain.Entities;

namespace BookingApp.Application.Common.Interfaces
{
    public interface IReservationRepository : IRepository<Reservation>
    {
        void Update(Reservation entity);
        void UpdateStatus(int reservationId, string orderStatus, int villaNumber);
        void UpdateStripePaymentId(int reservationId, string sessionId, string paymentIntentId);
    }
}
