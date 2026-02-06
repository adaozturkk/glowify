namespace Glowify.Data.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICouponRepository Coupon { get; }
        IProductRepository Product { get; }
        void Save();
    }
}
