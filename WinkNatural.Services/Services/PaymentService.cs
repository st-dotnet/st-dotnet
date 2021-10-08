using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinkNatural.Services.DTO;
using WinkNatural.Services.Interfaces;

namespace WinkNatural.Services.Services
{
    public class PaymentService : IPaymentService
    {
       

        public BankCardTransaction ProcessPayment(WinkPaymentRequest winkPaymentRequest)
        {
            BankCardTransaction bcx = new BankCardTransaction();

            bcx.setType(winkPaymentRequest.cardType);
            bcx.setCardName(winkPaymentRequest.cardName);
            bcx.setCardNumber(winkPaymentRequest.CardNumber);
            bcx.setCardExpirationMonth(winkPaymentRequest.CardExpirationMonth);
            bcx.setCardExpirationYear(winkPaymentRequest.CardExpirationYear);
            bcx.setAmount(winkPaymentRequest.Amount);
            BaseCommerceClient bcClient = new BaseCommerceClient("user", "pass", "key");

            bcx = bcClient.processBankCardTransaction(bcx);
            return bcx;
        }
       
    }
}
