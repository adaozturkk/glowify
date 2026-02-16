using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using System.Globalization;

namespace Glowify.Utility
{
    public class IyzicoPaymentService
    {
        public static async Task<bool> RefundOrder(string paymentId, string ip, IyzicoPaymentOptions options)
        {
            Options iyzicoOptions = new Options();
            iyzicoOptions.ApiKey = options.ApiKey;
            iyzicoOptions.SecretKey = options.SecretKey;
            iyzicoOptions.BaseUrl = options.BaseUrl;

            RetrievePaymentRequest retrieveRequest = new RetrievePaymentRequest();
            retrieveRequest.Locale = Locale.TR.ToString();
            retrieveRequest.PaymentId = paymentId;

            Payment payment = await Payment.Retrieve(retrieveRequest, iyzicoOptions);

            if (payment.Status != "success")
            {
                return false;
            }

            foreach (var item in payment.PaymentItems)
            {
                CreateRefundRequest refundRequest = new CreateRefundRequest();
                refundRequest.ConversationId = Guid.NewGuid().ToString();
                refundRequest.Locale = Locale.TR.ToString();

                refundRequest.PaymentTransactionId = item.PaymentTransactionId;

                refundRequest.Price = item.PaidPrice.ToString(CultureInfo.InvariantCulture);

                refundRequest.Ip = ip;
                refundRequest.Currency = Currency.TRY.ToString();

                Refund refund = await Refund.Create(refundRequest, iyzicoOptions);

                if (refund.Status != "success")
                {
                    Console.WriteLine($"IYZICO REFUND ERROR (Item ID: {item.PaymentTransactionId}): {refund.ErrorMessage}");
                    return false;
                }
            }

            return true;
        }
    }
}
