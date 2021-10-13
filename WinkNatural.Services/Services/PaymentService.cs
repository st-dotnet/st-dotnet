using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinkNatural.Services.DTO;
using WinkNatural.Services.Interfaces;
using RestSharp;
using WinkNatural.Services.DTO.Shopping;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http;
using static WinkNatural.Services.DTO.AuthPaymentModel;
using AutoMapper;

using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers.Bases;

namespace WinkNatural.Services.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _config;
        private readonly IPaymentService _paymentService;
        private readonly ICustomerService _customerService;
        private readonly IMapper _mapper;
        public PaymentService(IConfiguration config, ICustomerService customerService)
        {
            _config = config;
            _customerService = customerService;
        }

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

        ProcessPaymentMethodTransactionResponse IPaymentService.ProcessPaymentMethod(GetPaymentRequest getPaymentProPayModel)
        {
            var baseUrl = "https://xmltestapi.propay.com/ProtectPay";
            var request = BuildRequest(getPaymentProPayModel);
            var pathAndQuery = string.Format("/Payers/{0}/PaymentMethods/ProcessedTransactions/", request.CardNumber);
            var restRequest = CreateRestRequest(pathAndQuery, RestSharp.Method.PUT);
            restRequest.AddBody(request);
            return Execute<ProcessPaymentMethodTransactionResponse>(restRequest, baseUrl);
        }


        public ProcessPaymentMethodTransactionRequest BuildRequest(GetPaymentRequest getPaymentProPayModel)
        {
            return new ProcessPaymentMethodTransactionRequest
            {
                Amount = getPaymentProPayModel.Price,
                CurrencyCode = "USD",
                CustomerId = getPaymentProPayModel.CustomerId,
                CardNumber = getPaymentProPayModel.CardNumber,
                PaymentMethodId = "411879be-1011-4ee3-8382-3598007769df,",
                ExpMonth = getPaymentProPayModel.ExpMonth,
                ExpYear = getPaymentProPayModel.ExpYear,
                CVV = getPaymentProPayModel.CVV,
                FullName = getPaymentProPayModel.FirstName,
                ZipCode = getPaymentProPayModel.ZipCode,
                Address1 = getPaymentProPayModel.Address1,
                City = getPaymentProPayModel.City,
                State = getPaymentProPayModel.State,
                Country = getPaymentProPayModel.Country,
            };
        }

        /// <summary>
        /// Request factory to ensure API key is always first parameter added.
        /// </summary>
        /// <param name="resource">The resource name.</param>
        /// <param name="method">The HTTP method.</param>
        /// <returns>Returns a new <see cref="RestRequest"/>.</returns>
        public RestRequest CreateRestRequest(string resource, Method method)
        {
            var restRequest = new RestRequest { Resource = resource, Method = method, RequestFormat = DataFormat.Json, };
            var credentials = GetCredentials();
            restRequest.AddHeader("accept", "application/json");
            restRequest.AddHeader("Authorization", credentials);
            return restRequest;
        }

        /// <summary>
        /// Gets the credentials for the call.
        /// </summary>
        /// <returns>The credentials.</returns>
        public string GetCredentials()
        {
            var billerAccountId = _config.GetSection("AppSettings:billerAccountId").Value;  // biller account id
            var authToken = _config.GetSection("AppSettings:authToken").Value;  // authentication token of biller
            var encodedCredentials = Convert.ToBase64String(Encoding.Default.GetBytes(billerAccountId + ":" + authToken));

            var credentials = string.Format("Basic {0}", encodedCredentials);
            return credentials;
        }

        private T Execute<T>(IRestRequest request, string baseUrl) where T : class, new()
        {
            var client = new RestClient(baseUrl);
            var response = client.Execute<T>(request);

            if (response.ErrorException != null)
            {
                Console.WriteLine(
                "Error: Exception: {0}, Headers: {1}, Content: {2}, Status Code: {3}",
                response.ErrorException,
                response.Headers,
                response.Content,
                response.StatusCode);
            }

            return response.Data;
        }
        public class ProcessPaymentMethodTransactionRequest
        {
            public int ExpMonth { get; set; }
            public int ExpYear { get; set; }
            public int CVV { get; set; }
            public string ZipCode { get; set; }
            public string FullName { get; set; }
            public string Address1 { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string Country { get; set; }
            /// <summary>
            /// Gets or sets the payment method id.
            /// </summary>
            public string PaymentMethodId { get; set; }

            /// <summary>
            /// Gets or sets the ID of the merchant profile to use for this transaction.
            /// </summary>
            /// <remarks>
            /// <para>Optional </para>
            /// Any 64-bit signed integer.
            /// If zero (default) is specified, the "default" merchant profile for the identity will be used.
            /// </remarks>
            public int CustomerId { get; set; }

            /// <summary>
            /// REQUIRED: Gets or sets the payer's card number.
            /// </summary>
            public string CardNumber { get; set; }

            /// <summary>
            /// REQUIRED: Gets or sets amount of the transaction.
            /// </summary>
            /// <remarks>
            /// <para>Amounts should be integers. No decimals are allowed.</para>
            /// </remarks>
            /// <example>
            /// <para>$ 0.50 = 50</para>
            /// <para>$ 1.50 = 150</para>
            /// <para>$100.00 = 10000</para>
            /// </example>
            public decimal Amount { get; set; }

            public string CurrencyCode { get; set; }

        }
        public Result RequestResult
        {
            get; set;
        }

        public class Result
        {
            public string ResultValue { get; set; }
            public string ResultCode { get; set; }
            public string ResultMessage { get; set; }
        }

        public async Task<AddCardResponse> CreateCustomerProfile(GetPaymentRequest model)
        {
            var finalResponse = new AddCardResponse();
            var expMonth = model.ExpMonth < 10 ? $"0{model.ExpMonth}" : model.ExpMonth.ToString();
            string jsonData;

            var customer = _customerService.GetCustomer(model.CustomerId).Result.Customers[0];
            jsonData = JsonConvert.SerializeObject(new AuthorizeModel
            {
                createCustomerProfileRequest = new CreateCustomerProfileRequest
                {
                    merchantAuthentication = new DTO.MerchantAuthentication
                    {
                        name = _config.GetSection("AppSettings:APIKey").Value,
                        transactionKey = _config.GetSection("AppSettings:TransactionKey").Value,
                    },
                    profile = new DTO.Profile
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

            return finalResponse;
        }

        public async Task<AddCardResponse> CreatePaymentUsingAuthorizeNet(AddPaymentModel addPaymentModel)
        {
            var finalResponse = new AddCardResponse();

            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX;

            // define the merchant information (authentication / transaction id)
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                name = _config.GetSection("AppSettings:APIKey").Value,
                ItemElementName = ItemChoiceType.transactionKey,
                Item = _config.GetSection("AppSettings:TransactionKey").Value,
            };

            var creditCard = new creditCardType
            {
                cardNumber = addPaymentModel.CardNumber,
                expirationDate = addPaymentModel.ExpMonth + "" + addPaymentModel.ExpYear,  //"1028",
                cardCode = addPaymentModel.CVV
            };

            var billingAddress = new customerAddressType
            {
                firstName = addPaymentModel.Name,
                address = addPaymentModel.Address,
                city = addPaymentModel.city,
                zip = addPaymentModel.Zip
            };

            //standard api call to retrieve response
            var paymentType = new paymentType { Item = creditCard };

            var transactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.authCaptureTransaction.ToString(),    // charge the card
                amount = addPaymentModel.Amount,
                payment = paymentType,
                billTo = billingAddress
            };

            var request = new createTransactionRequest { transactionRequest = transactionRequest };

            // instantiate the controller that will call the service
            var controller = new createTransactionController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            var response = controller.GetApiResponse();

            // validate response
            if (response != null)
            {
                if (response.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (response.transactionResponse.messages != null)
                    {
                        Console.WriteLine("Successfully created transaction with Transaction ID: " + response.transactionResponse.transId);
                        Console.WriteLine("Response Code: " + response.transactionResponse.responseCode);
                        Console.WriteLine("Message Code: " + response.transactionResponse.messages[0].code);
                        Console.WriteLine("Description: " + response.transactionResponse.messages[0].description);
                        Console.WriteLine("Success, Auth Code : " + response.transactionResponse.authCode);

                        finalResponse.Message = response.transactionResponse.messages[0].description;
                        finalResponse.TransId = response.transactionResponse.transId;
                        finalResponse.AuthCode = response.transactionResponse.authCode;
                    }
                    else
                    {
                        Console.WriteLine("Failed Transaction.");
                        if (response.transactionResponse.errors != null)
                        {
                            Console.WriteLine("Error Code: " + response.transactionResponse.errors[0].errorCode);
                            Console.WriteLine("Error message: " + response.transactionResponse.errors[0].errorText);

                            finalResponse.Message = response.transactionResponse.errors[0].errorText;
                            finalResponse.TransId = response.transactionResponse.errors[0].errorCode;
                            finalResponse.AuthCode = "";
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Failed Transaction.");
                    if (response.transactionResponse != null && response.transactionResponse.errors != null)
                    {
                        Console.WriteLine("Error Code: " + response.transactionResponse.errors[0].errorCode);
                        Console.WriteLine("Error message: " + response.transactionResponse.errors[0].errorText);

                        finalResponse.Message = response.transactionResponse.errors[0].errorText;
                        finalResponse.TransId = response.transactionResponse.errors[0].errorCode;
                        finalResponse.AuthCode = "";
                    }
                    else
                    {
                        Console.WriteLine("Error Code: " + response.messages.message[0].code);
                        Console.WriteLine("Error message: " + response.messages.message[0].text);

                        finalResponse.Message = response.transactionResponse.errors[0].errorText;
                        finalResponse.TransId = response.transactionResponse.errors[0].errorCode;
                        finalResponse.AuthCode = "";
                    }
                }
            }
            else
            {
                Console.WriteLine("Null Response.");
            }

            return finalResponse;
        }
    }
}
