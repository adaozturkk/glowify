using Glowify.Data.Repository.IRepository;
using Glowify.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Glowify.Areas.Customer.Controllers
{
    public class WishlistController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public WishlistController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult ToggleWishlist(int productId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var wishlist = _unitOfWork.Wishlist.Get(u => u.ApplicationUserId == userId && u.ProductId == productId);

            if (wishlist != null)
            {
                _unitOfWork.Wishlist.Remove(wishlist);
                _unitOfWork.Save();

                return Json(new { success = true, added = false });
            }
            else
            {
                _unitOfWork.Wishlist.Add(new Wishlist
                {
                    ProductId = productId,
                    ApplicationUserId = userId
                });
                _unitOfWork.Save();

                return Json(new { success = true, added = true });
            }
        }
    }
}
