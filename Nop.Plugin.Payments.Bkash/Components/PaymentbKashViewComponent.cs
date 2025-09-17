using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.bKash.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.bKash.Components
{
    [ViewComponent(Name = "PaymentbKash")]
    public class PaymentbKashViewComponent : NopViewComponent
    {
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;

        public PaymentbKashViewComponent(
            ISettingService settingService,
            IStoreContext storeContext,
            ILocalizationService localizationService)
        {
            _settingService = settingService;
            _storeContext = storeContext;
            _localizationService = localizationService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var bKashPaymentSettings = await _settingService.LoadSettingAsync<bKashPaymentSettings>(storeScope);

            var model = new PaymentInfoModel
            {
                ReceiverNumber = bKashPaymentSettings.ReceiverNumber,
                ReceiverName = bKashPaymentSettings.ReceiverName,
                PaymentInstructions = await _localizationService.GetResourceAsync("Plugins.Payments.bKash.PaymentInstructions")
            };

            return View("~/Plugins/Payments.bKash/Views/PaymentInfo.cshtml", model);
        }
    }
}