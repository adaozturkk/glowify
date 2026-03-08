using Glowify.Data.Repository.IRepository;
using Glowify.Models;

namespace Glowify.Data.Repository
{
    public class WishlistRepository : Repository<Wishlist>, IWishlistRepository
    {
        private readonly ApplicationDbContext _db;

        public WishlistRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Wishlist obj)
        {
            _db.Wishlists.Update(obj);
        }
    }
}
