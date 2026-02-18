using System.Diagnostics;
using Glowify.Data;
using Glowify.Data.Repository.IRepository;
using Glowify.Models;
using Glowify.Models.ViewModels;
using Glowify.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Glowify.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index(string? search, string? category)
        {
            var productList = _unitOfWork.Product.GetAll();

            if (!string.IsNullOrEmpty(category) && category != "All")
            {
                productList = productList.Where(u => u.Category.ToString() == category);
            }

            if (!string.IsNullOrEmpty(search))
            {
                productList = productList.Where(u => u.Name.ToLower().Contains(search.ToLower())
                    || u.Description.ToLower().Contains(search.ToLower()));
            }

            return View(productList);
        }

        public IActionResult Details(int productId)
        {
            Product product = _unitOfWork.Product.Get(u => u.Id == productId);

            ShoppingCart cart = new()
            {
                Product = product,
                ProductId = productId,
                Count = 1
            };

            var reviews = _unitOfWork.ProductReview.GetAll(u => u.ProductId == productId, includeProperties: "ApplicationUser");

            ProductDetailsVM productDetailsVM = new()
            {
                ShoppingCart = cart,
                Reviews = reviews,
                ProductReview = new ProductReview()
            };

            if (User.Identity.IsAuthenticated)
            {
                var claimsIdentity = (System.Security.Claims.ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;

                var orderDetailsFromDb = _unitOfWork.OrderDetail.GetAll(u => u.ProductId == productId
                    && u.OrderHeader.ApplicationUserId == userId
                    && u.OrderHeader.OrderStatus == SD.StatusShipped,
                    includeProperties: "OrderHeader");

                productDetailsVM.CanReview = orderDetailsFromDb.Any();

                productDetailsVM.HasReviewed = _unitOfWork.ProductReview.Get(u => u.ProductId == productId 
                    && u.ApplicationUserId == userId) != null;
            }
            else
            {
                productDetailsVM.CanReview = false;
            }

            return View(productDetailsVM);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;

            shoppingCart.ApplicationUserId = userId;

            Product productFromDb = _unitOfWork.Product.Get(u => u.Id == shoppingCart.ProductId);
            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.ApplicationUserId == userId && u.ProductId == shoppingCart.ProductId);

            int currentCountInCart = cartFromDb != null ? cartFromDb.Count : 0;
            int totalRequested = currentCountInCart + shoppingCart.Count;

            if (totalRequested > productFromDb.Stock)
            {
                TempData["Error"] = $"Insufficient stock! You already have {currentCountInCart} items in your cart.";
                return RedirectToAction(nameof(Details), new { productId = shoppingCart.ProductId });
            }

            if (cartFromDb != null)
            {
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }
            else
            {
                _unitOfWork.ShoppingCart.Add(shoppingCart);
            }

            _unitOfWork.Save();

            var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Sum(u => u.Count);
            HttpContext.Session.SetInt32(SD.SessionCart, count);

            TempData["success"] = "Added to Cart successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult AddReview(ProductDetailsVM vm)
        {
            var claimsIdentity = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;

            if (_unitOfWork.ProductReview.Get(u => u.ProductId == vm.ProductReview.ProductId && u.ApplicationUserId == userId) != null)
            {
                TempData["Error"] = "You have already reviewed this product.";
            }
            else
            {
                ProductReview productReview = new()
                {
                    ProductId = vm.ProductReview.ProductId,
                    ApplicationUserId = userId,
                    Rating = vm.ProductReview.Rating,
                    Comment = vm.ProductReview.Comment,
                    ReviewDate = DateTime.Now
                };

                _unitOfWork.ProductReview.Add(productReview);
                _unitOfWork.Save();

                TempData["Success"] = "Review added successfully!";
            }

            return RedirectToAction(nameof(Details), new { productId = vm.ProductReview.ProductId });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
