using Glowify.Data;
using Glowify.Data.Repository.IRepository;
using Glowify.Models;
using Glowify.Models.ViewModels;
using Glowify.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Glowify.Areas.Customer.Controllers
{
    [Authorize]
    [Area("Customer")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<IyzicoPaymentOptions> _iyzicoOptions;
        private readonly IEmailSender _emailSender;

        public OrderController(IUnitOfWork unitOfWork, IOptions<IyzicoPaymentOptions> iyzicoOptions, IEmailSender emailSender   )
        {
            _unitOfWork = unitOfWork;
            _iyzicoOptions = iyzicoOptions;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            OrderVM orderVM = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(includeProperties: "Product").Where(u => u.OrderHeaderId == orderId).ToList()
            };

            return View(orderVM);
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(OrderVM orderVM)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);

            if (orderHeader.OrderStatus == SD.StatusShipped)
            {
                TempData["Error"] = "Shipped orders cannot be cancelled!";
                return RedirectToAction(nameof(Details), "Order", new { orderId = orderVM.OrderHeader.Id });
            }

            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                bool isRefunded = await IyzicoPaymentService.RefundOrder(
                     orderHeader.PaymentTransactionId,
                     HttpContext.Connection.RemoteIpAddress?.ToString(),
                     _iyzicoOptions.Value
                );

                if (isRefunded)
                {
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);

                    try
                    {
                        var userEmail = _unitOfWork.ApplicationUser.Get(u => u.Id == orderHeader.ApplicationUserId).Email;

                        string subject = $"Order Cancelled - Order #{orderHeader.Id}";
                        string htmlMessage = $@"
                            <h3>Your order has been cancelled.</h3>
                            <p>Dear {orderHeader.Name},</p>
                            <p>Your order <strong>#{orderHeader.Id}</strong> has been successfully cancelled.</p>
                            <p>A refund of <strong>{orderHeader.OrderTotal.ToString("c")}</strong> has been initiated to your payment method.</p>
                            <p>Please note that it may take 1-3 business days for the refund to appear on your bank statement.</p>
                            <br/>
                            <p>Best Regards,<br/>Glowify Team</p>";

                        await _emailSender.SendEmailAsync(userEmail, subject, htmlMessage);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("EMAIL ERROR: " + ex.Message);
                    }

                    TempData["Success"] = "Order Cancelled & Payment Refunded Successfully.";
                }
                else
                {
                    TempData["Error"] = "Cancel Failed! The bank rejected the refund. Please contact support.";
                }
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
                TempData["Success"] = "Order Cancelled Successfully.";
            }

            _unitOfWork.Save();
            return RedirectToAction(nameof(Details), "Order", new { orderId = orderVM.OrderHeader.Id });
        }

        [HttpGet]
        public IActionResult GetAll(string status)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            IEnumerable<OrderHeader> orderHeaders = _unitOfWork.OrderHeader.GetAll(
                u => u.ApplicationUserId == claim.Value && u.PaymentStatus != SD.PaymentStatusPending,
                includeProperties: "ApplicationUser"
            );

            switch (status)
            {
                case "pending":
                    orderHeaders = orderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusPending);
                    break;
                case "inprocess":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "shipped":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                case "cancelled":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusCancelled || u.OrderStatus == SD.StatusRefunded || u.PaymentStatus == SD.PaymentStatusRejected);
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusDelivered);
                    break;
                default:
                    break;
            }

            return Json(new { data = orderHeaders });
        }
    }
}
