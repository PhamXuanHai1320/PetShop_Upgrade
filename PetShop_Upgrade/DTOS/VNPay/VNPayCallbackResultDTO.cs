namespace PetShop_Upgrade.DTOS.VNPay
{
    public class VNPayCallbackResultDTO
    {
        public bool IsSignatureValid { get; set; }
        public bool IsSuccess { get; set; }
        public string TxnRef { get; set; } = default!;
        public int OrderId { get; set; }
        public string TransactionNo { get; set; } = default!;
        public string ResponseCode { get; set; } = default!;
        public string TransactionStatus { get; set; } = default!;
        public string TmnCode { get; set; } = default!;
        public decimal Amount { get; set; }
        public string RawQuery { get; set; } = default!;
    }
}
