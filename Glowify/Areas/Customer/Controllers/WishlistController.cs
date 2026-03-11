using Glowify.Data.Repository.IRepository;
using Glowify.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Glowify.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class WishlistController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public WishlistController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize]
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var wishlists = _unitOfWork.Wishlist.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product");
            return View(wishlists);
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

        [Authorize]
        public IActionResult Remove(int id)
        {
            var wishlistItem = _unitOfWork.Wishlist.Get(u => u.Id == id);
            if (wishlistItem != null)
            {
                _unitOfWork.Wishlist.Remove(wishlistItem);
                _unitOfWork.Save();
                TempData["success"] = "Item removed from wishlist successfully.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
