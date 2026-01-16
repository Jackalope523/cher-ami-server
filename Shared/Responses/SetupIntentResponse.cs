namespace CherAmiAPI.Shared.Responses
{
    public record SetupIntentResponse
    {
        public string ClientSecret { get; set; }
        public string ReturnURL { get; set; }
        public string MerchantDisplayName { get; set; }
        public bool AllowsDelayedPaymentMethods { get; set; }
    }
}
