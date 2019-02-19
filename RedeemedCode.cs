using System;

namespace PromoCodeService.Models
{
    public class RedeemedCode
    {
        public int Id { get; set; }
        public int PromoCodeId { get; set; }
        public string DeviceId { get; set; }
        public DateTime DateRedeemed { get; set; }

        public virtual PromoCode PromoCode { get; set; }
    }
}
