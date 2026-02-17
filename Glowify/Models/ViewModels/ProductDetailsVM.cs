namespace Glowify.Models.ViewModels
{
    public class ProductDetailsVM
    {
        public ShoppingCart ShoppingCart { get; set; }
        public IEnumerable<ProductReview> Reviews { get; set; }
        public ProductReview ProductReview { get; set; }
        public bool CanReview { get; set; }
    }
}
