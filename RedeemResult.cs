using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PromoCodeService.Models.Responses
{
    public class RedeemResult
    {
        public RedeemStatus Status { get; set; }
        public string Message { get; set; }
        public double Value { get; set; }
    }

    public enum RedeemStatus : int
    {
        OK = 0,
        AlreadyUsed,
        Expired,
        FailUnknown,
        NotFound
    }
}
