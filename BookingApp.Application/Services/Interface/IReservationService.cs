using BookingApp.Domain.Entities;

namespace BookingApp.Application.Services.Interface
{
    public interface IReservationService
    {
        void CreateReservation(Reservation reservation);
        Reservation GetReservationById(int reservationId);
        IEnumerable<Reservation> GetAllReservations(string userId = "", string statusFilterList = "");
        void UpdateStatus(int reservationId, string orderStatus, int villaNumber);
        void UpdateStripePaymentId(int reservationId, string sessionId, string paymentIntentId);
        public IEnumerable<int> GetCheckedInVillaNumbers(int villaId);
    }
}
