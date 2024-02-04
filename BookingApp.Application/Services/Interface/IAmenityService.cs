using BookingApp.Domain.Entities;

namespace BookingApp.Application.Services.Interface
{
    public interface IAmenityService
    {
        IEnumerable<Amenity> GetAllAmenities();
        Amenity GetAmenityById(int id);
        void CreateAmenity(Amenity amenity);
        void UpdateAmenity(Amenity amenity);
        bool DeleteAmenity(int id);
    }
}
