using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PromoCodeService.Models;
using PromoCodeService.Models.Responses;

namespace PromoCodeService.Controllers
{
    [Route("api/[controller]")]
    public class PromoCodeController : Controller
    {
        private readonly PromoCodeContext _ctx;

        public PromoCodeController(PromoCodeContext ctx)
        {
            _ctx = ctx;
        }
       
        [HttpGet("{code}/{deviceId}")]
        public async Task<ActionResult> RedeemCodeAsync(string code, string deviceId)
        {
            var pcode = await _ctx.PromoCodes.Include("RedeemedCodes").FirstOrDefaultAsync(x => x.Code == code);
            if (pcode == null)
            {
                return new JsonResult(new RedeemResult() { Status = RedeemStatus.NotFound, Message = "This promo code doesn't exist." });
            }

            if (pcode.ExpirationDate < DateTime.Now)
            {
                return new JsonResult(new RedeemResult() { Status = RedeemStatus.Expired, Message = "This promo code has expired." });
            }

            if (pcode.IsMultipleRedeemable)
            {
                if (pcode.RedeemedCodes.Count == 0)
                {
                    await RedeemAsync(pcode, deviceId);
                }
                else
                {
                    if (pcode.RedeemedCodes.Count >= pcode.MaxRedeems)
                    {
                        return new JsonResult(new RedeemResult() { Status = RedeemStatus.AlreadyUsed, Message = "This promo code can't be used anymore." });
                    }

                    if (pcode.RedeemedCodes.FirstOrDefault(x => x.DeviceId == deviceId) != null)
                    {
                        return new JsonResult(new RedeemResult() { Status = RedeemStatus.AlreadyUsed, Message = "This promo code has already been used on this device." });
                    }
                    else
                    {
                        await RedeemAsync(pcode, deviceId);
                    }
                }
            }
            else
            {
                if (pcode.RedeemedCodes.Count == 0)
                {
                    await RedeemAsync(pcode, deviceId);
                }
                else
                {
                    return new JsonResult(new RedeemResult() { Status = RedeemStatus.AlreadyUsed, Message = "This promo code can't be used anymore." });
                }
            }

            return new JsonResult(new RedeemResult() { Status = RedeemStatus.OK, Value = pcode.Value });
        }

        private async Task RedeemAsync(PromoCode pc, string deviceId)
        {
            RedeemedCode c = new RedeemedCode()
            {
                DateRedeemed = DateTime.Now,
                DeviceId = deviceId,
                PromoCode = pc, 
                PromoCodeId = pc.Id
            };

            pc.RedeemedCodes.Add(c);

            await _ctx.SaveChangesAsync();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _ctx.Dispose();
        }
    }
}
