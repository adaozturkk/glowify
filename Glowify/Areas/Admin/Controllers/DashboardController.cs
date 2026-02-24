using Glowify.Data.Repository.IRepository;
using Glowify.Models.ViewModels;
using Glowify.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Glowify.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var vm = new DashboardVM()
            {
                TotalUsers = _unitOfWork.ApplicationUser.GetAll().Count(),
                TotalOrders = _unitOfWork.OrderHeader.GetAll().Count(),
                PendingOrders = _unitOfWork.OrderHeader.GetAll(u => 
                    u.OrderStatus != SD.StatusPending || 
                    u.OrderStatus != SD.StatusCancelled).Count(),
                TotalRevenue = _unitOfWork.OrderHeader.GetAll(u => 
                    u.OrderStatus == SD.StatusShipped || 
                    u.OrderStatus == SD.StatusDelivered).Sum(u => u.OrderTotal)
            };

            return View(vm);
        }
    }
}
