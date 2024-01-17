using BookingApp.Domain.Entities;
using System.Linq.Expressions;

namespace BookingApp.Application.Common.Interfaces
{
    public interface IAmenityRepository : IRepository<Amenity>
    {
        void Update(Amenity entity);
    }
}
