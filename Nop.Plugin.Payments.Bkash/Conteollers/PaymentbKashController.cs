using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.bKash.Models;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.bKash.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [AutoValidateAntiforgeryToken]
    public class PaymentbKashController : BasePaymentController
    {
        #region Fields

        protected readonly ILocalizationService _localizationService;
        protected readonly INotificationService _notificationService;
        protected readonly IPermissionService _permissionService;
        protected readonly ISettingService _settingService;
        protected readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public PaymentbKashController(
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        [CheckPermission(StandardPermission.Configuration.MANAGE_PAYMENT_METHODS)]
        public async Task<IActionResult> Configure()
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var bKashPaymentSettings = await _settingService.LoadSettingAsync<bKashPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                DescriptionText = bKashPaymentSettings.DescriptionText,
                AdditionalFee = bKashPaymentSettings.AdditionalFee,
                ReceiverNumber = bKashPaymentSettings.ReceiverNumber,
                ReceiverName = bKashPaymentSettings.ReceiverName,
                AdditionalFeePercentage = bKashPaymentSettings.AdditionalFeePercentage,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.DescriptionText_OverrideForStore = await _settingService.SettingExistsAsync(bKashPaymentSettings, x => x.DescriptionText, storeScope);
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(bKashPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(bKashPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.ReceiverNumber_OverrideForStore = await _settingService.SettingExistsAsync(bKashPaymentSettings, x => x.ReceiverNumber, storeScope);
                model.ReceiverName_OverrideForStore = await _settingService.SettingExistsAsync(bKashPaymentSettings, x => x.ReceiverName, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(bKashPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
            }

            return View("~/Plugins/Payments.bKash/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Configuration.MANAGE_PAYMENT_METHODS)]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return await Configure();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var bKashPaymentSettings = await _settingService.LoadSettingAsync<bKashPaymentSettings>(storeScope);

            //save settings
            bKashPaymentSettings.DescriptionText = model.DescriptionText;
            bKashPaymentSettings.AdditionalFee = model.AdditionalFee;
            bKashPaymentSettings.ReceiverNumber = model.ReceiverNumber;
            bKashPaymentSettings.ReceiverName = model.ReceiverName;
            bKashPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */

            await _settingService.SaveSettingOverridablePerStoreAsync(bKashPaymentSettings, x => x.DescriptionText, model.DescriptionText_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(bKashPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(bKashPaymentSettings, x => x.ReceiverNumber, model.ReceiverNumber_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(bKashPaymentSettings, x => x.ReceiverName, model.ReceiverName_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(bKashPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        #endregion
    }
}