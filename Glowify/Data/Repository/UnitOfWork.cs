using Glowify.Data.Repository.IRepository;
using Glowify.Models;

namespace Glowify.Data.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        public ICouponRepository Coupon {  get; set; }

        public UnitOfWork(ApplicationDbContext db) 
        { 
            _db = db;
            Coupon = new CouponRepository(_db);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
