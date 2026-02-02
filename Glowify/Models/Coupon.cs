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
        [Display(Name = "Discount Amount")]
        public double DiscountAmount { get; set; }

        [Required]
        [Display(Name = "Minimum Amount")]
        public double MinAmount { get; set; }

        [Required]
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }
    }
}
