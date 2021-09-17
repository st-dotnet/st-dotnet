using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinkNatural.Services.DTO;
using WinkNatural.Services.Services;

namespace WinkNatural.Services.Interfaces
{
    public interface IPaymentService
    {
        BankCardTransaction ProcessPayment(WinkPaymentRequest winkPaymentRequest);
    }
}
