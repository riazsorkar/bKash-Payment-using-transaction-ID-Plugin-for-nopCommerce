using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Payments.bKash.Models;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.bKash.Components
{
    [ViewComponent(Name = "PaymentbKash")]
    public class PaymentbKashViewComponent : NopViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new PaymentInfoModel();
            return View("~/Plugins/Payments.bKash/Views/PaymentInfo.cshtml", model);
        }
    }
}