namespace Glowify.Data.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICouponRepository Coupon { get; }
        IProductRepository Product { get; }
        IOrderHeaderRepository OrderHeader { get; }
        IOrderDetailRepository OrderDetail { get; }
        void Save();
    }
}
