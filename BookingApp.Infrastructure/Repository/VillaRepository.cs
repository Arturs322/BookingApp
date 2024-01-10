using BookingApp.Application.Common.Interfaces;
using BookingApp.Domain.Entities;
using BookingApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookingApp.Infrastructure.Repository
{
    public class VillaRepository : Repository<Villa>, IVillaRepository
    {
        private readonly ApplicationDbContext _db;

        public VillaRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Villa entity)
        {
            _db.Update(entity);
        }
    }
}
