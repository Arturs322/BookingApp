using BookingApp.Application.Common.Interfaces;
using BookingApp.Infrastructure.Data;

namespace BookingApp.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        public IVillaRepository Villa { get; set; }
        public IVillaNumberRepository VillaNumber { get; set; }
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Villa = new VillaRepository(_db);
            VillaNumber = new VillaNumberRepository(_db);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
