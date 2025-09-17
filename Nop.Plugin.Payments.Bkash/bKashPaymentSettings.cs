using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.bKash
{
    public class bKashPaymentSettings : ISettings
    {
        public string DescriptionText { get; set; }
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFeePercentage { get; set; }

        public string ReceiverNumber { get; set; }
        public string ReceiverName { get; set; }

    }
}