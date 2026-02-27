using Glowify.Data.Repository.IRepository;
using Glowify.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Glowify.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    [Area("Admin")]
    public class ProductReviewController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductReviewController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }


        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var reviews = _unitOfWork.ProductReview.GetAll(includeProperties: "ApplicationUser,Product")
                .OrderBy(u => u.IsApproved).ThenByDescending(u => u.ReviewDate);

            return Json(new { data = reviews });
        }

        [HttpPost]
        public IActionResult Approve(int id)
        {
            var review = _unitOfWork.ProductReview.Get(u => u.Id == id);
            if (review == null)
            {
                return Json(new { success = false, message = "Error while approving" });
            }

            review.IsApproved = true;
            _unitOfWork.Save();
            return Json(new { success = true, message = "Review Approved!" });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var review = _unitOfWork.ProductReview.Get(u => u.Id == id);
            if (review == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _unitOfWork.ProductReview.Remove(review);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Review Deleted!" });
        }

        #endregion
    }
}
