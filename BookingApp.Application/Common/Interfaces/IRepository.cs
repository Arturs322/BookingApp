using BookingApp.Domain.Entities;
using System.Linq.Expressions;

namespace BookingApp.Application.Common.Interfaces
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeproperties = null);
        T Get(Expression<Func<T, bool>> filter, string? includeproperties = null, bool tracked = false);
        void Add(T villa);
        void Remove(T villa);
    }
}
