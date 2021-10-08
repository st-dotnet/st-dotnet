using Microsoft.AspNetCore.Mvc;
using System; 
using System.Linq;
using System.Threading.Tasks;
using WinkNatural.Services.DTO;
using WinkNatural.Services.Interfaces; 
using System.Net.Http; 
using WinkNaturals.Models; 
using Newtonsoft.Json;
using static WinkNaturals.Models.GetPaymentModel;
using WinkNatural.Services.Services; 
using Microsoft.Extensions.Configuration;

namespace WinkNaturals.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IPaymentService _paymentService;
        private readonly ICustomerService _customerService;

        public PaymentController(IConfiguration config,
            IPaymentService paymentService,
            ICustomerService customerService)
        {
            _config = config;
            _paymentService = paymentService;
            _customerService = customerService;
        }



        [HttpGet("ProcessPayment")]
        public IActionResult ProcessPayment(WinkPaymentRequest winkPaymentRequest)
        {
            return Ok(_paymentService.ProcessPayment(winkPaymentRequest));
        }

        //This below is for make payment using Authorize.net payment gateway
        [HttpPost("create-customer-profile")]
        public async Task<IActionResult> CreateCustomerProfile(AddCardModel model)
        {
            var finalResponse = new AddCardResponse();
            var expMonth = model.ExpMonth < 10 ? $"0{model.ExpMonth}" : model.ExpMonth.ToString();
            string jsonData;
           
                var customer =  _customerService.GetCustomer(model.CustomerId).Result.Customers[0];
                jsonData = JsonConvert.SerializeObject(new AuthorizeModel
                {
                    createCustomerProfileRequest = new CreateCustomerProfileRequest
                    {
                        merchantAuthentication = new Models.MerchantAuthentication
                        {
                            name = _config.GetSection("AppSettings:APIKey").Value,
                            transactionKey = _config.GetSection("AppSettings:TransactionKey").Value,
                        },
                        profile = new Models.Profile
                        {
                            description = $"This is a {customer.FirstName}'s Profile",
                            email = customer.Email,
                            paymentProfiles = new PaymentProfiles
                            {
                                customerType = "individual",
                                payment = new Payment
                                {
                                    creditCard = new CreditCard
                                    {
                                        cardNumber = model.CardNumber,
                                        expirationDate = $"20{model.ExpYear}-{model.ExpMonth}"
                                    }
                                }
                            }
                        },
                        validationMode = "testMode"
                    }
                });
                var stringContent = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");
                try
                {
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri($"{_config.GetSection("AppSettings:AuthorizeNetTestBaseUrl").Value}createCustomerProfile");
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    var response = await client.PostAsync("", stringContent).Result.Content.ReadAsStringAsync();
                    var finalResult = JsonConvert.DeserializeObject<CreateCustomerProfileResponse>(response);
                    finalResponse.Message = finalResult.messages.message.FirstOrDefault().text.ToString();
                    finalResponse.ResultCode = finalResult.messages.resultCode;
                }
                catch (Exception ex)
                {

                }
         
            return Ok(finalResponse);
        }
    }
}
