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

                TotalOrders = _unitOfWork.OrderHeader.GetAll(u => u.OrderStatus != SD.StatusPending).Count(),

                PendingOrders = _unitOfWork.OrderHeader.GetAll(u => u.OrderStatus == SD.StatusApproved || u.OrderStatus == SD.StatusInProcess).Count(),

                TotalRevenue = _unitOfWork.OrderHeader.GetAll(u => 
                    u.OrderStatus == SD.StatusShipped || 
                    u.OrderStatus == SD.StatusDelivered).Sum(u => u.OrderTotal),

                TopProducts = _unitOfWork.OrderDetail.GetAll(includeProperties: "Product")
                    .GroupBy(u => u.Product.Name)
                    .Select(g => new TopProductVM
                    {
                        ProductName = g.Key,
                        TotalSold = g.Sum(u => u.Count)
                    })
                    .OrderByDescending(u => u.TotalSold)
                    .Take(5)
                    .ToList()
            };

            return View(vm);
        }


        #region API CALLS

        [HttpGet]
        public IActionResult GetOrderStatusData()
        {
            var orderList = _unitOfWork.OrderHeader.GetAll();

            var statusCounts = orderList.GroupBy(o => o.OrderStatus)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                }).ToList();

            var labels = statusCounts.Select(s => s.Status ?? "Unknown").ToArray();
            var data = statusCounts.Select(s => s.Count).ToArray();

            return Json(new { labels = labels, data = data });
        }

        [HttpGet]
        public IActionResult GetSalesTrendData()
        {
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);

            var trendData = _unitOfWork.OrderHeader.GetAll(u =>
                (u.OrderStatus == SD.StatusShipped || u.OrderStatus == SD.StatusDelivered) && (u.OrderDate >= thirtyDaysAgo))
                .GroupBy(u => u.OrderDate.Date)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    DateStr = g.Key.ToString("dd MMM"),
                    Total = g.Sum(u => u.OrderTotal)
                })
                .ToList();

            var labels = trendData.Select(x => x.DateStr).ToArray();
            var data = trendData.Select(x => x.Total).ToArray();

            return Json(new { labels = labels, data = data });
        }

        [HttpGet]
        public IActionResult GetTopCustomerData()
        {
            var topCustomers = _unitOfWork.OrderHeader.GetAll(u => u.OrderStatus == SD.StatusShipped || u.OrderStatus == SD.StatusDelivered)
                .GroupBy(u => u.Name)
                .Select(g => new
                {
                    Name = g.Key,
                    Total = g.Sum(u => u.OrderTotal)
                })
                .OrderByDescending(u => u.Total)
                .Take(5)
                .ToList();

            var labels = topCustomers.Select(x => x.Name).ToArray();
            var data = topCustomers.Select(x => x.Total).ToArray();

            return Json(new { labels = labels, data = data });
        }

        #endregion
    }
}
