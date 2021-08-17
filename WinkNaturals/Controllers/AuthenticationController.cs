using AutoMapper;
using Exigo.Api.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WinkNatural.Services.DTO.Customer;
using WinkNatural.Services.Interfaces;
using WinkNaturals.Models;

namespace WinkNatural.Web.WinkNaturals.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticateService _authenticateService;
        private readonly IMapper _mapper;
        public AuthenticationController(IAuthenticateService authenticate, IMapper mapper)
        {
            _authenticateService = authenticate;
            _mapper = mapper;
        }

        #region Customer

        /// <summary>
        /// Create customer
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("CreateCustomer")]
        public async Task<IActionResult> CreateCustomer(CustomerCreateModel model)
        {
            try
            {
                var createCustomerRequest = _mapper.Map<CreateCustomerRequest>(model);

                //Create customer in Exigo service
                await _authenticateService.CreateCustomer(createCustomerRequest);

                //Authenticate customer
                var result = await _authenticateService.SignInCustomer(new AuthenticateCustomerRequest { LoginName = model.LoginName, Password = model.LoginPassword });

                return Ok(new CustomerCreateResponse
                {
                    Email = model.Email,
                    LoginName = model.LoginName,
                    Phone = model.Phone,
                    Token = result.Token.ToString()
                });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// Signin customer
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("SignInCustomer")]
        public async Task<IActionResult> SignInCustomer(CustomerSignInModel model)
        {
            try
            {
                var signinRequest = _mapper.Map<AuthenticateCustomerRequest>(model);

                //Signin customer in Exigo service
                return Ok(await _authenticateService.SignInCustomer(signinRequest));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// Send forgot password email
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("SendForgotPasswordEmail")]
        public async Task<IActionResult> SendForgotPasswordEmail(CustomerUpdateModel model)
        {
            try
            {
                var customerEmailRequest = _mapper.Map<CustomerUpdateRequest>(model);

                //Send email with Exigo service
                return Ok(await _authenticateService.SendForgotPasswordEmail(customerEmailRequest));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// Update customer password
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("UpdateCustomer")]
        public async Task<IActionResult> UpdateCustomer(CustomerUpdateModel model)
        {
            try
            {
                var customerUpdateRequest = _mapper.Map<CustomerUpdateRequest>(model);

                //Update customer password with Exigo service
                return Ok(await _authenticateService.UpdateCustomerPassword(customerUpdateRequest));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// Validate customer email/username
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("ValidateCustomer")]
        public async Task<IActionResult> ValidateCustomer(CustomerValidationModel model)
        {
            try
            {
                var customerValidationRequest = _mapper.Map<CustomerValidationRequest>(model);

                //Validate username/email with Exigo service
                return Ok(await _authenticateService.IsEmailOrUsernameExists(customerValidationRequest));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        #endregion
    }
}
