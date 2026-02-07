using Glowify.Data.Repository.IRepository;
using Glowify.Models;

namespace Glowify.Data.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        public ICouponRepository Coupon {  get; private set; }
        public IProductRepository Product { get; private set; }
        public IOrderHeaderRepository OrderHeader { get; private set; }
        public IOrderDetailRepository OrderDetail { get; private set; }
        public IShoppingCartRepository ShoppingCart { get; private set; }
        public IRepository<ApplicationUser> ApplicationUser { get; private set; }

        public UnitOfWork(ApplicationDbContext db) 
        { 
            _db = db;
            Coupon = new CouponRepository(_db);
            Product = new ProductRepository(_db);
            OrderHeader = new OrderHeaderRepository(_db);
            OrderDetail = new OrderDetailRepository(_db);
            ShoppingCart = new ShoppingCartRepository(_db);
            ApplicationUser = new Repository<ApplicationUser>(_db);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
