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
            return View();
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

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                TempData["Error"] = "There is no coupon with this id.";
                return RedirectToAction(nameof(Index));
            }

            var couponFromDb = _db.Coupons.FirstOrDefault(u => u.Id == id);

            if (couponFromDb == null)
            {
                TempData["Error"] = "There is no coupon with this id.";
                return RedirectToAction(nameof(Index));
            }

            return View(couponFromDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Coupon obj)
        {
            if (_db.Coupons.Any(u => u.Code.ToLower() == obj.Code.ToLower() && u.Id != obj.Id))
            {
                ModelState.AddModelError("Code", "Coupon code already exists!");
            }

            if (ModelState.IsValid)
            {
                _db.Coupons.Update(obj);
                _db.SaveChanges();

                TempData["Success"] = "Coupon updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(obj);
        }


        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var objList = _db.Coupons.ToList();
            return Json(new { data = objList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _db.Coupons.FirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _db.Coupons.Remove(obj);
            _db.SaveChanges();

            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion
    }
}
