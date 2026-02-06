using Glowify.Data;
using Glowify.Data.Repository.IRepository;
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
        private readonly IUnitOfWork _unitOfWork;

        public CouponController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                return View(new Coupon());
            }

            var couponFromDb = _unitOfWork.Coupon.Get(u => u.Id == id);

            if (couponFromDb == null)
            {
                TempData["Error"] = "There is no coupon with this id.";
                return RedirectToAction(nameof(Index));
            }

            return View(couponFromDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Coupon obj)
        {
            var existingCoupon = _unitOfWork.Coupon.Get(u => u.Code.ToLower() == obj.Code.ToLower() && u.Id != obj.Id);

            if (existingCoupon != null)
            {
                ModelState.AddModelError("Code", "Coupon code already exists!");
            }

            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    _unitOfWork.Coupon.Add(obj);
                    TempData["Success"] = "Coupon created successfully!";
                }
                else
                {
                    _unitOfWork.Coupon.Update(obj);
                    TempData["Success"] = "Coupon updated successfully!";
                }

                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }

            return View(obj);
        }


        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var objList = _unitOfWork.Coupon.GetAll();
            return Json(new { data = objList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _unitOfWork.Coupon.Get(u => u.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _unitOfWork.Coupon.Remove(obj);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion
    }
}
