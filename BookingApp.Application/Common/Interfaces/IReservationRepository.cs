using BookingApp.Domain.Entities;

namespace BookingApp.Application.Common.Interfaces
{
    public interface IReservationRepository : IRepository<Reservation>
    {
        void Update(Reservation entity);
    }
}
