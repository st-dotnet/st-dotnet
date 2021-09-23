using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinkNatural.Services.DTO.Shopping
{
   public class PromoCode
    {
        public int PromoID { get; set; }
        public string PromoName { get; set; }
        public float DiscountPer { get; set; }
        public string ErrorMessage { get; set; }
    }
}
