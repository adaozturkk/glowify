using System.ComponentModel.DataAnnotations;

namespace Glowify.Models
{
    public class Coupon
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Coupon Code")]
        public string Code { get; set; }

        [Required]
        [Range(1, 10000, ErrorMessage = "Discount must be between 1 and 10,000!")]
        [Display(Name = "Discount Amount")]
        public double DiscountAmount { get; set; }

        [Required]
        [Range(0, 50000, ErrorMessage = "Minimum amount cannot be less than 0!")]
        [Display(Name = "Minimum Amount")]
        public double MinAmount { get; set; }

        [Required]
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }
    }
}
