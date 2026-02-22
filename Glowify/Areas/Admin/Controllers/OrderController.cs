using Glowify.Data;
using Glowify.Data.Repository.IRepository;
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
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly IOptions<IyzicoPaymentOptions> _iyzicoOptions;

        public OrderController(IUnitOfWork unitOfWork, IEmailSender emailSender, IOptions<IyzicoPaymentOptions> iyzicoOptions)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _iyzicoOptions = iyzicoOptions;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");

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
        public IActionResult StartProcessing(OrderVM orderVM)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);
            _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();
            TempData["Success"] = "Order Status Updated Successfully!";
            return RedirectToAction(nameof(Details), "Order", new { orderId = orderVM.OrderHeader.Id });
        }

        [HttpPost]
        public async Task<IActionResult> ShipOrder(OrderVM orderVM)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id, includeProperties: "ApplicationUser");

            orderHeader.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = orderVM.OrderHeader.Carrier;

            _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusShipped);
            orderHeader.ShippingDate = DateTime.Now;

            _unitOfWork.Save();

            try
            {
                if (orderHeader.ApplicationUser != null && !string.IsNullOrEmpty(orderHeader.ApplicationUser.Email))
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

                    await _emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, subject, body);
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
        public async Task<IActionResult> CancelOrder(OrderVM orderVM)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);

            if (orderHeader.OrderStatus == SD.StatusRefunded ||
                orderHeader.OrderStatus == SD.StatusCancelled ||
                orderHeader.OrderStatus == SD.StatusShipped ||
                orderHeader.OrderStatus == SD.StatusDelivered)
            {
                TempData["Error"] = "This order cannot be cancelled!";
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

        [HttpPost]
        public async Task<IActionResult> DeliverOrder(OrderVM orderVM)
        {
            var order = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id, includeProperties: "ApplicationUser");

            if (order != null)
            {
                if (order.OrderStatus == SD.StatusShipped)
                {
                    _unitOfWork.OrderHeader.UpdateStatus(order.Id, SD.StatusDelivered);
                    _unitOfWork.Save();

                    try
                    {
                        if (order.ApplicationUser != null && !string.IsNullOrEmpty(order.ApplicationUser.Email))
                        {
                            string subject = $"Your Order Has Been Delivered! - Order #{order.Id}";

                            string htmlMessage = $@"
                                <h3>Hi {order.Name},</h3>
                                <p>Great news! Your order <strong>#{order.Id}</strong> has been successfully delivered.</p>
                                <hr/>
                                <p>We hope you love your Glowify products as much as we loved packing them for you.</p>
                                <br/>
                                <p>Thank you for choosing us.</p>
                                <p>Best Regards,<br/>The Glowify Team</p>";

                            await _emailSender.SendEmailAsync(order.ApplicationUser.Email, subject, htmlMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("DELIVERY EMAIL ERROR: " + ex.Message);
                    }

                    TempData["Success"] = "Order Delivered Successfully!";
                }
                else
                {
                    TempData["Error"] = "Order must be shipped first.";
                }
            }
            else
            {
                TempData["Error"] = "Order does not exist.";
            }

            return RedirectToAction(nameof(Details), "Order", new { orderId = orderVM.OrderHeader.Id });
        }
    }
}
