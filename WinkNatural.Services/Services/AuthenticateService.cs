using Exigo.Api.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WinkNatural.Common.Utils.Enum;
using WinkNatural.Services.DTO.Customer;
using WinkNatural.Services.Interfaces;

namespace WinkNatural.Services.Services
{
    public class AuthenticateService : IAuthenticateService
    {
        private readonly ExigoApiClient exigoApiClient = new("WinkNaturals", "API_Web", "PB45DY5J5pmq9anE");
        private readonly IConfiguration _config;
        private readonly ICustomerService _customerService;

        #region constructor

        public AuthenticateService(IConfiguration config, ICustomerService customerService)
        {
            _config = config;
            _customerService = customerService;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Create customer
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<CreateCustomerResponse> CreateCustomer(CreateCustomerRequest request)
        {
            try
            {
                return await exigoApiClient.CreateCustomerAsync(request);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// Signin customer
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<CustomerCreateResponse> SignInCustomer(AuthenticateCustomerRequest request)
        {
            try
            {
                //Exigo service login request

                var result = await exigoApiClient.AuthenticateCustomerAsync(request);
                if (result.CustomerID == 0)
                {
                    return new CustomerCreateResponse { ErrorMessage = "User is not authenticated." };
                }

                // Get customer
                var customer = await _customerService.GetCustomer(result.CustomerID);

                var token = GenerateJwtToken(result);
                return new CustomerCreateResponse
                {
                    Email = customer.Customers[0].Email,
                    LoginName = customer.Customers[0].LoginName,
                    Phone = customer.Customers[0].Phone,
                    Token = token.ToString()
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// Update customer password
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<CustomerUpdateResponse> UpdateCustomerPassword(CustomerUpdateRequest request)
        {
            try
            {
                //Customer update password request
                var customerUpdateRequest = new UpdateCustomerRequest { CustomerID = request.CustomerId, LoginPassword = request.NewPassword };
                var result = await exigoApiClient.UpdateCustomerAsync(customerUpdateRequest);
                return new CustomerUpdateResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new CustomerUpdateResponse { Success = false, ErrorMessage = "Error occurred during update the password!" };
            }
        }

        /// <summary>
        /// Send forgot password email
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<CustomerUpdateResponse> SendForgotPasswordEmail(CustomerUpdateRequest request)
        {
            try
            {
                //Get customer by login name
                var getCustomerRequest = new GetCustomersRequest { Email = request.Email };
                var customer = await exigoApiClient.GetCustomersAsync(getCustomerRequest);

                var body = $"To reset your password click this link! <a href={request.Url}/{customer.Customers[0].CustomerID}>Reset Password</a>";

                var sendEmail = await exigoApiClient.SendEmailAsync(new SendEmailRequest
                {
                    CustomerID = customer.Customers[0].CustomerID,
                    Body = body,
                    MailFrom = _config.GetSection("EmailConfiguration:NoReplyEmail").Value,
                    MailTo = request.Email,
                    Subject = $"{_config.GetSection("EmailConfiguration:CompanyName").Value} - Forgot Password"
                });
                return new CustomerUpdateResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new CustomerUpdateResponse { Success = false, ErrorMessage = "Forgot password email not sent!" };
            }

        }

        /// <summary>
        /// Validate username/email
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<bool> IsEmailOrUsernameExists(CustomerValidationRequest request)
        {
            try
            {
                //Check if Email is exists or not
                if (!string.IsNullOrEmpty(request.Email))
                {
                    var customerEmailResult = await exigoApiClient.GetCustomersAsync(new GetCustomersRequest { Email = request.Email });
                    if (customerEmailResult.Customers.Length != 0) return true;
                }
                else //Check if username is exists or not
                {
                    var customerUsernameResult = await exigoApiClient.GetCustomersAsync(new GetCustomersRequest { LoginName = request.Username });
                    if (customerUsernameResult.Customers.Length != 0) return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #region private methods

        //Get CST datetime
        private static DateTime GetCSTSateTime()
        {
            try
            {
                DateTime datetimeNow = DateTime.Now;
                TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                return TimeZoneInfo.ConvertTime(datetimeNow, cstZone);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        //Generate JWT token
        private string GenerateJwtToken(AuthenticateCustomerResponse customer)
        {
            try
            {
                // generate token that is valid for 7 days
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_config["JwtSettings:Key"]);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                    new Claim("customerId", customer.CustomerID.ToString()),
                    new Claim("firstName", customer.FirstName.ToString()),
                    new Claim("lastName", customer.LastName.ToString())
                }),
                    Expires = DateTime.UtcNow.AddDays(30),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        #endregion
    }
}
