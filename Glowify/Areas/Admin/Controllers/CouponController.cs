using Glowify.Data;
using Glowify.Models;
using Glowify.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Glowify.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    [Area("Admin")]
    public class CouponController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CouponController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var couponsFromDb = _db.Coupons.ToList();
            return View(couponsFromDb);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Coupon obj)
        {
            if (_db.Coupons.Any(u => u.Code.ToLower() == obj.Code.ToLower()))
            {
                ModelState.AddModelError("Code", "Coupon code already exists!");
            }

            if (ModelState.IsValid)
            {
                _db.Coupons.Add(obj);
                _db.SaveChanges();

                TempData["Success"] = "Coupon created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(obj);
        }
    }
}
