using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.bKash.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;

namespace Nop.Plugin.Payments.bKash
{
    public class bKashPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly bKashPaymentSettings _bKashPaymentSettings;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public bKashPaymentProcessor(
            ISettingService settingService,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            bKashPaymentSettings bKashPaymentSettings, IStoreContext storeContext)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
            _bKashPaymentSettings = bKashPaymentSettings;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult
            {
                NewPaymentStatus = PaymentStatus.Pending
            };
            return Task.FromResult(result);
        }

        public Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            // Nothing happens here for bKash as it's handled externally
            return Task.CompletedTask;
        }

        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            // You can add logic to hide the payment method under certain conditions
            return Task.FromResult(false);
        }

        public Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(_bKashPaymentSettings.AdditionalFee);
        }

        public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return Task.FromResult(result);
        }

        public Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return Task.FromResult(result);
        }

        public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return Task.FromResult(result);
        }

        public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return Task.FromResult(result);
        }

        public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return Task.FromResult(result);
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentbKash/Configure";
        }

        public string GetPublicViewComponentName()
        {
            return "PaymentbKash";
        }

        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            var warnings = new List<string>();

            // Validate bKash transaction ID
            var transactionId = form["bkashTransactionId"].ToString();
            if (string.IsNullOrEmpty(transactionId))
            {
                warnings.Add("bKash Transaction ID is required");
            }
            //else if (transactionId.Length != 10 || !transactionId.StartsWith("BK"))
            //{
            //    warnings.Add("Invalid bKash Transaction ID format");
            //}

            return Task.FromResult<IList<string>>(warnings);
        }

        public async Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var bKashPaymentSettings = await _settingService.LoadSettingAsync<bKashPaymentSettings>(storeScope);
            var paymentRequest = new ProcessPaymentRequest
            {
                CustomValues = new Dictionary<string, object>
        {
            { "Bkash TransactionId", form["bkashTransactionId"].ToString() },
            { "Bkash Sender Number", form["bkashSenderNumber"].ToString() },
            { "Bkash Receiver Number", bKashPaymentSettings?.ReceiverNumber ?? "Not configured" },
            { "Bkash Receiver Name", bKashPaymentSettings?.ReceiverName ?? "Not configured" },
            { "TransactionId Submit Date", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
        }
            };

            return paymentRequest;
        }

        public Type GetPublicViewComponent()
        {
            return typeof(Components.PaymentbKashViewComponent);
        }

        public override async Task InstallAsync()
        {
            // Settings
            var settings = new bKashPaymentSettings
            {
                DescriptionText = "Pay by bKash. You will be redirected to bKash to complete the payment.",
                AdditionalFee = 0
            };
            await _settingService.SaveSettingAsync(settings);

            // Locales
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Payments.bKash.RedirectionTip"] = "You will be redirected to bKash site to complete the payment.",
                ["Plugins.Payments.bKash.TransactionId"] = "bKash Transaction ID",
                ["Plugins.Payments.bKash.SenderNumber"] = "bKash Sender Number",
                ["Plugins.Payments.bKash.ReceiverNumber"] = "Receiver Number",
                ["Plugins.Payments.bKash.ReceiverName"] = "Receiver Name",
                ["Plugins.Payments.bKash.PaymentInstructions"] = "Please send money to the bKash number below and provide the transaction ID.",
                ["Plugins.Payments.bKash.AdditionalFee"] = "Additional fee",
                ["Plugins.Payments.bKash.AdditionalFee.Hint"] = "Enter additional fee to charge your customers.",
                ["Plugins.Payments.bKash.DescriptionText"] = "Description",
                ["Plugins.Payments.bKash.DescriptionText.Hint"] = "Enter info that will be shown to customers during checkout"
            });

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            // Settings
            await _settingService.DeleteSettingAsync<bKashPaymentSettings>();

            // Locales
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Payments.bKash");

            await base.UninstallAsync();
        }

        #endregion

        #region Properties

        public bool SupportCapture => false;
        public bool SupportRefund => false;
        public bool SupportVoid => false;
        public bool SupportPartiallyRefund => false;
        public bool SupportCaptureThenVoid => false;
        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;
        public bool SkipPaymentInfo => false;
        public Task<string> GetPaymentMethodDescriptionAsync()
        {
            return Task.FromResult(_localizationService.GetResourceAsync("Plugins.Payments.bKash.DescriptionText").Result);
        }

        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            ArgumentNullException.ThrowIfNull(order);

            //it's not a redirection payment method. So we always return false
            return Task.FromResult(false);
        }

        #endregion
    }
}