using System;
using System.Collections.Generic;

namespace PromoCodeService.Models
{
    public class PromoCode
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string ObjectType { get; set; }
        public double Value { get; set; }
        public bool IsMultipleRedeemable { get; set; }
        public int MaxRedeems { get; set; }
        public DateTime DateAdded { get; set; }

        public DateTime ExpirationDate { get; set; }


        public List<RedeemedCode> RedeemedCodes { get; set; }
    }
}
