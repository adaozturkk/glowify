using Glowify.Data;
using Glowify.Data.Repository.IRepository;
using Glowify.Models;
using Glowify.Models.ViewModels;
using Glowify.Utility;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
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
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<IyzicoPaymentOptions> _iyzicoOptions;
        private readonly IEmailSender _emailSender;

        public CartController(IUnitOfWork unitOfWork, IOptions<IyzicoPaymentOptions> iyzicoOptions, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _iyzicoOptions = iyzicoOptions;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM shoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
                OrderTotal = 0,
                ShippingCost = 0
            };

            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                shoppingCartVM.OrderTotal += (cart.Product.Price * cart.Count);
            }

            var couponCode = HttpContext.Session.GetString("CouponCode");

            if (!string.IsNullOrEmpty(couponCode))
            {
                var couponFromDb = _unitOfWork.Coupon.Get(u => u.Code == couponCode);

                if (couponFromDb != null && couponFromDb.IsActive)
                {
                    if (shoppingCartVM.OrderTotal >= couponFromDb.MinAmount)
                    {
                        shoppingCartVM.Coupon = couponFromDb;
                        shoppingCartVM.OrderTotal -= couponFromDb.DiscountAmount;
                    }
                    else
                    {
                        HttpContext.Session.Remove("CouponCode");
                    }
                }
            }

            if (shoppingCartVM.OrderTotal < 1000 && shoppingCartVM.ShoppingCartList.Count() > 0)
            {
                shoppingCartVM.ShippingCost = 39.99;
                shoppingCartVM.OrderTotal += shoppingCartVM.ShippingCost;
            }

            return View(shoppingCartVM);
        }

        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId, includeProperties: "Product");

            if (cartFromDb != null)
            {
                if (cartFromDb.Product.Stock > cartFromDb.Count)
                {
                    cartFromDb.Count += 1;
                    _unitOfWork.ShoppingCart.Update(cartFromDb);
                    _unitOfWork.Save();

                    var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Sum(u => u.Count);
                    HttpContext.Session.SetInt32(SD.SessionCart, count);
                }
                else
                {
                    TempData["Error"] = "You reached stock limit! You can't add more products.";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);

            if (cartFromDb != null)
            {
                if (cartFromDb.Count <= 1)
                {
                    _unitOfWork.ShoppingCart.Remove(cartFromDb);
                }
                else
                {
                    cartFromDb.Count -= 1;
                    _unitOfWork.ShoppingCart.Update(cartFromDb);
                }

                _unitOfWork.Save();

                var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Sum(u => u.Count);
                HttpContext.Session.SetInt32(SD.SessionCart, count);
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);

            if (cartFromDb != null)
            {
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
                _unitOfWork.Save();

                var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Sum(u => u.Count);
                HttpContext.Session.SetInt32(SD.SessionCart, count);
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM shoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
                OrderHeader = new()
            };

            if (shoppingCartVM.ShoppingCartList.Count() == 0)
            {
                TempData["error"] = "Your cart is empty! Please add products first.";
                return RedirectToAction(nameof(Index));
            }

            var applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

            shoppingCartVM.OrderHeader.ApplicationUser = applicationUser;
            shoppingCartVM.OrderHeader.Name = applicationUser.Name;
            shoppingCartVM.OrderHeader.PhoneNumber = applicationUser.PhoneNumber;
            shoppingCartVM.OrderHeader.StreetAddress = applicationUser.StreetAddress;
            shoppingCartVM.OrderHeader.City = applicationUser.City;
            shoppingCartVM.OrderHeader.State = applicationUser.State;
            shoppingCartVM.OrderHeader.PostalCode = applicationUser.PostalCode;

            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                shoppingCartVM.OrderTotal += cart.Product.Price * cart.Count;
            }

            var couponCode = HttpContext.Session.GetString("CouponCode");

            if (!string.IsNullOrEmpty(couponCode))
            {
                var couponFromDb = _unitOfWork.Coupon.Get(u => u.Code == couponCode);

                if (couponFromDb != null && couponFromDb.IsActive)
                {
                    if (shoppingCartVM.OrderTotal >= couponFromDb.MinAmount)
                    {
                        shoppingCartVM.Coupon = couponFromDb;
                        shoppingCartVM.OrderTotal -= couponFromDb.DiscountAmount;
                    }
                }
            }

            shoppingCartVM.ShippingCost = shoppingCartVM.OrderTotal < 1000 ? 39.99 : 0;
            shoppingCartVM.OrderTotal += shoppingCartVM.ShippingCost;

            return View(shoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SummaryPOST(ShoppingCartVM shoppingCartVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product");

            shoppingCartVM.OrderHeader.OrderTotal = 0;
            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Price = cart.Product.Price;
                shoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }

            var couponCode = HttpContext.Session.GetString("CouponCode");
            double rawProductTotal = shoppingCartVM.OrderHeader.OrderTotal;

            if (!string.IsNullOrEmpty(couponCode))
            {
                var couponFromDb = _unitOfWork.Coupon.Get(u => u.Code == couponCode);

                if (couponFromDb != null && couponFromDb.IsActive)
                {
                    if (shoppingCartVM.OrderHeader.OrderTotal >= couponFromDb.MinAmount)
                    {
                        shoppingCartVM.Coupon = couponFromDb;
                        shoppingCartVM.OrderHeader.OrderTotal -= couponFromDb.DiscountAmount;

                        shoppingCartVM.OrderHeader.CouponCode = couponFromDb.Code;
                        shoppingCartVM.OrderHeader.OrderDiscount = couponFromDb.DiscountAmount;
                    }
                }
            }

            shoppingCartVM.ShippingCost = shoppingCartVM.OrderHeader.OrderTotal < 1000 ? 39.99 : 0;
            shoppingCartVM.OrderHeader.OrderTotal += shoppingCartVM.ShippingCost;

            shoppingCartVM.OrderHeader.ShippingCost = shoppingCartVM.ShippingCost;

            if (!ModelState.IsValid)
            {
                return View(shoppingCartVM);
            }

            shoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            shoppingCartVM.OrderHeader.ApplicationUserId = userId;

            _unitOfWork.OrderHeader.UpdateStatus(shoppingCartVM.OrderHeader.Id, SD.StatusPending, SD.PaymentStatusPending);

            _unitOfWork.OrderHeader.Add(shoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = shoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
            }
            _unitOfWork.Save();


            var applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == shoppingCartVM.OrderHeader.ApplicationUserId);

            Iyzipay.Options options = new Iyzipay.Options();
            options.ApiKey = _iyzicoOptions.Value.ApiKey;
            options.SecretKey = _iyzicoOptions.Value.SecretKey;
            options.BaseUrl = _iyzicoOptions.Value.BaseUrl;

            CreateCheckoutFormInitializeRequest request = new CreateCheckoutFormInitializeRequest();
            request.Locale = Locale.EN.ToString();
            request.ConversationId = shoppingCartVM.OrderHeader.Id.ToString();
            request.Price = rawProductTotal.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            request.PaidPrice = shoppingCartVM.OrderHeader.OrderTotal.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            request.Currency = Currency.TRY.ToString();
            request.BasketId = shoppingCartVM.OrderHeader.Id.ToString();
            request.PaymentGroup = PaymentGroup.PRODUCT.ToString();
            request.CallbackUrl = "https://localhost:7213/Customer/Cart/CallBack?orderId=" + shoppingCartVM.OrderHeader.Id;
            request.EnabledInstallments = new List<int>() { 2, 3, 6, 9 };

            Buyer buyer = new Buyer();
            buyer.Id = shoppingCartVM.OrderHeader.ApplicationUserId;
            buyer.Name = shoppingCartVM.OrderHeader.Name;
            buyer.Surname = "Müşteri";
            buyer.GsmNumber = shoppingCartVM.OrderHeader.PhoneNumber;
            buyer.Email = applicationUser.Email;
            buyer.IdentityNumber = "11111111111";
            buyer.RegistrationAddress = shoppingCartVM.OrderHeader.StreetAddress;
            buyer.Ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            buyer.City = shoppingCartVM.OrderHeader.City;
            buyer.Country = "Turkey";
            request.Buyer = buyer;

            Address billingAddress = new Address();
            billingAddress.ContactName = shoppingCartVM.OrderHeader.Name;
            billingAddress.City = shoppingCartVM.OrderHeader.City;
            billingAddress.Country = "Turkey";
            billingAddress.Description = shoppingCartVM.OrderHeader.StreetAddress;
            request.BillingAddress = billingAddress;
            request.ShippingAddress = billingAddress;

            List<BasketItem> basketItems = new List<BasketItem>();
            foreach (var item in shoppingCartVM.ShoppingCartList)
            {
                BasketItem basketItem = new BasketItem();
                basketItem.Id = item.ProductId.ToString();
                basketItem.Name = item.Product.Name;
                basketItem.Category1 = "Skincare";
                basketItem.ItemType = BasketItemType.PHYSICAL.ToString();
                basketItem.Price = item.Price.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);

                for (int i = 0; i < item.Count; i++)
                {
                    basketItems.Add(basketItem);
                }
            }
            request.BasketItems = basketItems;

            CheckoutFormInitialize checkoutFormInitialize = await CheckoutFormInitialize.Create(request, options);

            if (checkoutFormInitialize.Status == "success")
            {
                return Redirect(checkoutFormInitialize.PaymentPageUrl);
            }
            else
            {
                ViewData["Error"] = checkoutFormInitialize.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult OrderConfirmation(int id)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == id);

            if (orderHeader != null)
            {
                HttpContext.Session.Clear();
            }

            return View(id);
        }

        [HttpPost]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CallBack(IFormCollection formCollection, int orderId)
        {
            string token = formCollection["token"];

            Iyzipay.Options options = new Iyzipay.Options();
            options.ApiKey = _iyzicoOptions.Value.ApiKey;
            options.SecretKey = _iyzicoOptions.Value.SecretKey;
            options.BaseUrl = _iyzicoOptions.Value.BaseUrl;

            RetrieveCheckoutFormRequest request = new RetrieveCheckoutFormRequest();
            request.Token = token;
            CheckoutForm checkoutForm = await CheckoutForm.Retrieve(request, options);

            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId);

            if (checkoutForm.Status == "success" && checkoutForm.PaymentStatus == "SUCCESS")
            {
                if (orderHeader != null)
                {
                    orderHeader.OrderStatus = SD.StatusApproved;
                    orderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    orderHeader.PaymentDate = DateTime.Now;

                    orderHeader.PaymentTransactionId = checkoutForm.PaymentId;

                    var orderDetails = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderHeader.Id);

                    foreach (var detail in orderDetails)
                    {
                        var product = _unitOfWork.Product.Get(u => u.Id == detail.ProductId);

                        if (product != null)
                        {
                            product.Stock -= detail.Count;
                        }
                    }
                    _unitOfWork.Save();

                    try
                    {
                        var userId = orderHeader.ApplicationUserId;
                        var user = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

                        if (user != null && !string.IsNullOrEmpty(user.Email))
                        {
                            string subject = $"Order Confirmed! - Order #{orderHeader.Id}";

                            string body = $@"
                                <h3>Thank you, {orderHeader.Name}!</h3>
                                <p>Your payment has been successfully received and your order is confirmed.</p>
                                <p><strong>Order Number:</strong> {orderHeader.Id}</p>
                                <p><strong>Total Amount:</strong> {orderHeader.OrderTotal:c}</p>
                                <br/>
                                <p>We are getting your order ready to be shipped. You will receive another email once it's on the way!</p>
                                <p>Best Regards,<br/>The Glowify Team</p>";

                            await _emailSender.SendEmailAsync(user.Email, subject, body);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ERROR SENDING EMAIL: " + ex.Message);
                    }
                }

                var shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId);

                _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
                _unitOfWork.Save();

                return RedirectToAction(nameof(OrderConfirmation), "Cart", new { id = orderId });
            }
            else
            {
                if (orderHeader != null)
                {
                    orderHeader.OrderStatus = SD.StatusCancelled;
                    orderHeader.PaymentStatus = SD.PaymentStatusRejected;

                    _unitOfWork.Save();
                }

                TempData["Error"] = "Payment Failed: " + checkoutForm.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public IActionResult ApplyCoupon(ShoppingCartVM shoppingCartVM)
        {
            if (HttpContext.Session.GetString("CouponCode") != null)
            {
                TempData["Error"] = "You can't add more than 1 coupon!";
                return RedirectToAction(nameof(Index));
            }

            var couponCode = shoppingCartVM.CouponCode;

            if (string.IsNullOrEmpty(couponCode))
            {
                TempData["Error"] = "Coupon code can't be empty!";
                return RedirectToAction(nameof(Index));
            }

            var couponFromDb = _unitOfWork.Coupon.Get(u => u.Code.ToLower() == couponCode.ToLower());

            if (couponFromDb == null)
            {
                TempData["Error"] = "Invalid Coupon Code!";
                return RedirectToAction(nameof(Index));
            }

            if (!couponFromDb.IsActive)
            {
                TempData["Error"] = "This coupon is no longer active.";
                return RedirectToAction(nameof(Index));
            }

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product");

            double cartTotal = 0;
            foreach (var cart in shoppingCarts)
            {
                cartTotal += (cart.Product.Price * cart.Count);
            }

            if (cartTotal < couponFromDb.MinAmount)
            {
                TempData["Error"] = $"This coupon requires a minimum spend of {couponFromDb.MinAmount:c}!";
                return RedirectToAction(nameof(Index));
            }

            HttpContext.Session.SetString("CouponCode", couponFromDb.Code);
            TempData["Success"] = "Coupon applied successfully!";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult RemoveCoupon()
        {
            HttpContext.Session.Remove("CouponCode");
            TempData["Success"] = "Coupon removed.";

            return RedirectToAction(nameof(Index));
        }
    }
}
