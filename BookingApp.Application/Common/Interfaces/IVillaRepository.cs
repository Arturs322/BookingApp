using BookingApp.Domain.Entities;
using System.Linq.Expressions;

namespace BookingApp.Application.Common.Interfaces
{
    public interface IVillaRepository : IRepository<Villa>
    {
        void Update(Villa villa);
    }
}
