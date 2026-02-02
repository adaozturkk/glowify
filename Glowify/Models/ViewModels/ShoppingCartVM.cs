using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Glowify.Models.ViewModels
{
    public class ShoppingCartVM
    {
        [ValidateNever]
        public IEnumerable<ShoppingCart> ShoppingCartList { get; set; }

        public double OrderTotal { get; set; }
        public OrderHeader OrderHeader { get; set; }
        public double ShippingCost { get; set; }

        [ValidateNever]
        public Coupon Coupon { get; set; }

        public string? CouponCode { get; set; }
    }
}