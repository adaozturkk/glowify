using Glowify.Data;
using Glowify.Models;
using Glowify.Models.ViewModels;
using Glowify.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Glowify.Areas.Customer.Controllers
{
    [Authorize]
    [Area("Customer")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;

        public OrderController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            OrderVM orderVM = new()
            {
                OrderHeader = _db.OrderHeaders.Include(u => u.ApplicationUser).FirstOrDefault(u => u.Id == orderId),
                OrderDetail = _db.OrderDetails.Include(o => o.Product).Where(u => u.OrderHeaderId == orderId).ToList()
            };

            return View(orderVM);
        }

        [HttpPost]
        public IActionResult CancelOrder(OrderVM orderVM)
        {
            var orderHeader = _db.OrderHeaders.FirstOrDefault(u => u.Id == orderVM.OrderHeader.Id);

            if (orderHeader.OrderStatus == SD.StatusShipped)
            {
                TempData["Error"] = "Shipped orders cannot be cancelled!";
                return RedirectToAction(nameof(Details), "Order", new { orderId = orderVM.OrderHeader.Id });
            }

            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                orderHeader.OrderStatus = SD.StatusCancelled;
                orderHeader.PaymentStatus = SD.StatusRefunded;
            }
            else
            {
                orderHeader.OrderStatus = SD.StatusCancelled;
                orderHeader.PaymentStatus = SD.StatusCancelled;
            }

            _db.SaveChanges();

            TempData["Success"] = "Order Cancelled Successfully!";
            return RedirectToAction(nameof(Details), "Order", new { orderId = orderVM.OrderHeader.Id });
        }

        [HttpGet]
        public IActionResult GetAll(string status)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            IEnumerable<OrderHeader> orderHeaders = _db.OrderHeaders
                .Where(u => u.ApplicationUserId == claim.Value && u.PaymentStatus != SD.PaymentStatusPending)
                .Include(u => u.ApplicationUser)
                .ToList();

            switch (status)
            {
                case "pending":
                    orderHeaders = orderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusPending);
                    break;
                case "inprocess":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                case "cancelled":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusCancelled || u.OrderStatus == SD.StatusRefunded || u.PaymentStatus == SD.PaymentStatusRejected);
                    break;
                default:
                    break;
            }

            return Json(new { data = orderHeaders });
        }
    }
}
