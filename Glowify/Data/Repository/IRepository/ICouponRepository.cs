using Glowify.Models;

namespace Glowify.Data.Repository.IRepository
{
    public interface ICouponRepository : IRepository<Coupon>
    {
        void Update(Coupon obj);
    }
}
