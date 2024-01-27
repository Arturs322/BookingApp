namespace BookingApp.Application.Common.Interfaces
{
    public interface IUnitOfWork
    {
        IVillaRepository Villa { get; }
        IVillaNumberRepository VillaNumber { get; }
        IAmenityRepository Amenity { get; }
        IReservationRepository Reservation { get; }
        IApplicationUserRepository ApplicationUser { get; }
        void Save();
    }
}
