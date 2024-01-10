using BookingApp.Domain.Entities;

namespace BookingApp.Application.Common.Interfaces
{
    public interface IVillaNumberRepository : IRepository<VillaNumber>
    {
        void Update(VillaNumber villa);
    }
}
