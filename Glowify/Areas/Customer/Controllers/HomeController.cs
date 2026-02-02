using System.Diagnostics;
using Glowify.Data;
using Glowify.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Glowify.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> products = _db.Products.ToList();
            return View(products);
        }

        public IActionResult Details(int productId)
        {
            Product product = _db.Products.FirstOrDefault(u => u.Id == productId);

            ShoppingCart cart = new()
            {
                Product = product,
                ProductId = productId,
                Count = 1
            };

            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;

            shoppingCart.ApplicationUserId = userId;

            Product productFromDb = _db.Products.FirstOrDefault(u => u.Id == shoppingCart.ProductId);
            ShoppingCart cartFromDb = _db.ShoppingCarts.FirstOrDefault(
                u => u.ApplicationUserId == userId && u.ProductId == shoppingCart.ProductId);

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
                _db.ShoppingCarts.Update(cartFromDb);
            }
            else
            {
                _db.ShoppingCarts.Add(shoppingCart);
            }

            _db.SaveChanges();

            TempData["success"] = "Added to Cart successfully!";
            return RedirectToAction(nameof(Index));
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
