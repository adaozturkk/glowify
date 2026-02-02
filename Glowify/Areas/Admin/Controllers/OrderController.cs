using Glowify.Data;
using Glowify.Models;
using Glowify.Models.ViewModels;
using Glowify.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Collections;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Glowify.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;

        public OrderController(ApplicationDbContext db, IEmailSender emailSender)
        {
            _db = db;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;

            if (User.IsInRole(SD.Role_Admin))
            {
                orderHeaders = _db.OrderHeaders.Include(u => u.ApplicationUser).ToList();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

                orderHeaders = _db.OrderHeaders
                    .Where(u => u.ApplicationUserId == claim.Value && u.PaymentStatus != SD.PaymentStatusPending)
                    .Include(u => u.ApplicationUser)
                    .ToList();
            }

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
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult StartProcessing(OrderVM orderVM)
        {
            var orderHeader = _db.OrderHeaders.FirstOrDefault(u => u.Id == orderVM.OrderHeader.Id);
            orderHeader.OrderStatus = SD.StatusInProcess;
            _db.SaveChanges();
            TempData["Success"] = "Order Status Updated Successfully!";
            return RedirectToAction(nameof(Details), "Order", new { orderId = orderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> ShipOrder(OrderVM orderVM)
        {
            var orderHeader = _db.OrderHeaders.FirstOrDefault(u => u.Id == orderVM.OrderHeader.Id);

            orderHeader.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = orderVM.OrderHeader.Carrier;

            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;

            _db.SaveChanges();

            try
            {
                var userId = orderHeader.ApplicationUserId;
                var user = _db.ApplicationUsers.FirstOrDefault(u => u.Id == userId);

                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    string subject = $"Your Order is on its way! - Order #{orderHeader.Id}";

                    string body = $@"
                        <h3>Great News, {orderHeader.Name}!</h3>
                        <p>Your order has been shipped and is on its way to you.</p>
                        <hr/>
                        <p><strong>Order Number:</strong> {orderHeader.Id}</p>
                        <p><strong>Carrier:</strong> {orderHeader.Carrier}</p>
                        <p><strong>Tracking Number:</strong> {orderHeader.TrackingNumber}</p>
                        <br/>
                        <p>You can track your shipment using the tracking number above.</p>
                        <p>We hope you enjoy your Glowify products!</p>
                        <br/>
                        <p>Best Regards,<br/>The Glowify Team</p>";

                    await _emailSender.SendEmailAsync(user.Email, subject, body);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR SENDING EMAIL: " + ex.Message);
            }

            TempData["Success"] = "Order Shipped Successfuly!";
            return RedirectToAction(nameof(Details), "Order", new { orderId = orderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize]
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
    }
}
