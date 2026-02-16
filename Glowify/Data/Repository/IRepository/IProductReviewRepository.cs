using Glowify.Models;

namespace Glowify.Data.Repository.IRepository
{
    public interface IProductReviewRepository : IRepository<ProductReview>
    {
        void Update(ProductReview obj);
    }
}
