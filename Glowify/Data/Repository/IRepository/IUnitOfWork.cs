namespace Glowify.Data.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICouponRepository Coupon { get; }
        void Save();
    }
}
