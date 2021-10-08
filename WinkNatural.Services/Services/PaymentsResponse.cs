using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinkNatural.Services.Services
{
   public class PaymentsResponse
    {

    }

    public class AddCardResponse
    {
        public string Message { get; set; }
        public string ResultCode { get; set; }
    }

    public class AddCardRequest
    {
        public int CardType { get; set; }
        public string CardNumber { get; set; }
        public int ExpMonth { get; set; }
        public int ExpYear { get; set; }
        public int CustomerId { get; set; }
        public bool Primary { get; set; }
        public bool Active { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ZipCode { get; set; }
        public string AuthProfileId { get; set; }
        public string AuthCardId { get; set; }
    }
}
