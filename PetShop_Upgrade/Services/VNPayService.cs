using PetShop_Upgrade.DTOS.VNPay;
using PetShop_Upgrade.Services.Interfaces;
using PetShop_Upgrade.Utils;
using PetShop_Upgrade.Utils.Interfaces;

namespace PetShop_Upgrade.Services
{
    public class VNPayService : IVNPayService
    {
        private readonly IConfiguration _configuration;
        private readonly IVNPayHelper _vnpayHelper;
        private readonly string vnpTmnCode;
        private readonly string vnpHashSecret;
        private readonly string vnpBaseUrl;
        private readonly string vnpReturnUrl;
        private readonly string vnpVersion;
        private readonly string vnpCommand;
        private readonly string vnpCurrCode;
        private readonly string vnpLocale;
        public VNPayService(IConfiguration configuration, IVNPayHelper vnpayHelper)
        {
            _configuration = configuration;
            _vnpayHelper = vnpayHelper;
            vnpTmnCode = _configuration["VNPay:TmnCode"];
            vnpHashSecret = _configuration["VNPay:HashSecret"];
            vnpBaseUrl = _configuration["VNPay:BaseUrl"];
            vnpReturnUrl = _configuration["VNPay:ReturnUrl"];
            vnpVersion = _configuration["VNPay:Version"];
            vnpCommand = _configuration["VNPay:Command"];
            vnpCurrCode = _configuration["VNPay:CurrCode"];
            vnpLocale = _configuration["VNPay:Locale"];
        }
        public string CreatePaymentUrl(int orderId, decimal amount, string orderInfo, string ipAddress)
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vnpayCurrentDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);
            // TxnRef PHẢI duy nhất cho mỗi lần request (VNPay không cho trùng),
            // nên ghép OrderId + timestamp để vẫn parse ngược lại được OrderId khi nhận callback
            var txnRef = $"{orderId}_{vnpayCurrentDate:yyyyMMddHHmmssfff}";
            var vnp_Params = new SortedDictionary<string, string>
            {
                { "vnp_Version", vnpVersion },
                { "vnp_Command", vnpCommand },
                { "vnp_TmnCode", vnpTmnCode },
                { "vnp_Amount", ((long)(amount * 100)).ToString() },
                { "vnp_CurrCode", vnpCurrCode },
                { "vnp_TxnRef", txnRef },
                { "vnp_OrderInfo", orderInfo},
                { "vnp_OrderType", "other"},
                { "vnp_Locale", vnpLocale},
                { "vnp_ReturnUrl", vnpReturnUrl },
                { "vnp_IpAddr", ipAddress },
                { "vnp_CreateDate", vnpayCurrentDate.ToString("yyyyMMddHHmmss") }
            };  
            var signData = _vnpayHelper.BuildQueryString(vnp_Params, true);
            var secureHash = _vnpayHelper.HmacSHA512(vnpHashSecret, signData);
            return $"{vnpBaseUrl}?{signData}&vnp_SecureHash={secureHash}";
        }
        public VNPayCallbackResultDTO ValidateCallback(IQueryCollection query)
        {
            var vnpParams = new SortedDictionary<string, string>();
            string receivedHash = string.Empty;

            foreach (var kv in query)
            {
                if (kv.Key == "vnp_SecureHash" || kv.Key == "vnp_SecureHashType")
                {
                    if (kv.Key == "vnp_SecureHash")
                        receivedHash = kv.Value.ToString();
                    continue;
                }
                vnpParams[kv.Key] = kv.Value.ToString();
            }

            var signData = _vnpayHelper.BuildQueryString(vnpParams, urlEncode: true);
            var calculatedHash = _vnpayHelper.HmacSHA512(vnpHashSecret, signData);

            var isSignatureValid = string.Equals(calculatedHash, receivedHash, StringComparison.OrdinalIgnoreCase);

            var txnRef = vnpParams.GetValueOrDefault("vnp_TxnRef", "");
            var orderId = 0;
            var underscoreIndex = txnRef.IndexOf('_');
            if (underscoreIndex > 0)
                int.TryParse(txnRef[..underscoreIndex], out orderId);

            var responseCode = vnpParams.GetValueOrDefault("vnp_ResponseCode", "");
            var transactionStatus = vnpParams.GetValueOrDefault("vnp_TransactionStatus", "");
            var amountRaw = vnpParams.GetValueOrDefault("vnp_Amount", "0");
            decimal.TryParse(amountRaw, out var amountVnpFormat);

            return new VNPayCallbackResultDTO
            {
                IsSignatureValid = isSignatureValid,
                IsSuccess = isSignatureValid && responseCode == "00" && transactionStatus == "00",
                TxnRef = txnRef,
                OrderId = orderId,
                TransactionNo = vnpParams.GetValueOrDefault("vnp_TransactionNo", ""),
                ResponseCode = responseCode,
                TransactionStatus = transactionStatus,
                TmnCode = vnpParams.GetValueOrDefault("vnp_TmnCode", ""),
                Amount = amountVnpFormat / 100, // đổi ngược lại về VND thật
                RawQuery = signData
            };
        }
    }
}
