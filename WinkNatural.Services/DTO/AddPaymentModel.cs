using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinkNatural.Services.DTO
{
    public class AddPaymentModel
    {
        public string CardNumber { get; set; }
        public int ExpMonth { get; set; }
        public int ExpYear { get; set; }
        public string CVV { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string city { get; set; }
        public string Zip { get; set; }

        public decimal Amount { get; set; }
        public string Payment { get; set; }
        public string BillingAddress { get; set; }
    }
}
