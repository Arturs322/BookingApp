using BookingApp.Domain.Entities;
using System.Linq.Expressions;

namespace BookingApp.Application.Common.Interfaces
{
    public interface IApplicationUserRepository : IRepository<ApplicationUser>
    {
        void Update(ApplicationUser entity);
    }
}
