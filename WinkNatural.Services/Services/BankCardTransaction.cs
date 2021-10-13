﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinkNatural.Services.DTO;
using RestSharp;
namespace WinkNatural.Services.Services
{
   public class BankCardTransaction
    {
        public void setType(BankCardTransactionEnum type)
        {
            Console.WriteLine(type);
        }
        public void setCardName(string nameOnCard)
        {
            Console.WriteLine(nameOnCard);
        }
        public void setCardNumber(string cardNumber)
        {
            Console.WriteLine(cardNumber);
        }
        public void setCardExpirationMonth(string month)
        {
            Console.WriteLine(month);
        }
        public void setCardExpirationYear(string year)
        {
            Console.WriteLine(year);
        }
        public void setAmount(double amount)
        {
            Console.WriteLine(amount);

        }
        public string status { get; set; }
        public int paymentId { get; set; }
    }

   
}
