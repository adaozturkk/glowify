using Glowify.Models;

namespace Glowify.Data.Repository.IRepository
{
    public interface IWishlistRepository : IRepository<Wishlist>
    {
        void Update(Wishlist obj);
    }
}
