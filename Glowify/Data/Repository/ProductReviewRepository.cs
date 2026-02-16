using Glowify.Data.Repository.IRepository;
using Glowify.Models;

namespace Glowify.Data.Repository
{
    public class ProductReviewRepository : Repository<ProductReview>, IProductReviewRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductReviewRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ProductReview obj)
        {
            _db.ProductReviews.Update(obj);
        }
    }
}
