using Glowify.Models;

namespace Glowify.Data.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICouponRepository Coupon { get; }
        IProductRepository Product { get; }
        IOrderHeaderRepository OrderHeader { get; }
        IOrderDetailRepository OrderDetail { get; }
        IShoppingCartRepository ShoppingCart { get; }
        IRepository<ApplicationUser> ApplicationUser { get; }
        IProductReviewRepository ProductReview { get; }
        void Save();
    }
}
