using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNatural.Services.DTO;
using WinkNatural.Services.Interfaces;

namespace WinkNaturals.Controllers
{
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
            
        }
        [HttpGet("ProcessPayment")]
        public IActionResult ProcessPayment(WinkPaymentRequest winkPaymentRequest)
        {
            return Ok(_paymentService.ProcessPayment(winkPaymentRequest));
        }
    }
}
