using BookingApp.Application.Common.Interfaces;
using BookingApp.Domain.Entities;
using BookingApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookingApp.Infrastructure.Repository
{
    public class VillaNumberRepository : Repository<VillaNumber>, IVillaNumberRepository
    {
        private readonly ApplicationDbContext _db;

        public VillaNumberRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(VillaNumber entity)
        {
            _db.VillaNumbers.Update(entity);
        }
    }
}
