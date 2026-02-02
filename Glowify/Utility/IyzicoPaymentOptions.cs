namespace Glowify.Utility
{
    public class IyzicoPaymentOptions
    {
        public const string Iyzico = "IyzicoOptions";

        public string ApiKey { get; set; }
        public string SecretKey { get; set; }
        public string BaseUrl { get; set; }
    }
}
