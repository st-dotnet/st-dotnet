using Microsoft.AspNetCore.Mvc;
using Resources;
using System;
using WinkNatural.Services.Interfaces;
using WinkNatural.Services.Utilities;
using WinkNatural.Services.DTO.Shopping;
using WinkNaturals.Models;
using Exigo.Api.Client;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using WinkNatural.Common;
using WinkNatural.Services.DTO;
using static WinkNatural.Services.DTO.Shopping.CreditCard;
using WinkNatural.Services.ExigoServices;
using WinkNatural.Common.Utils;

namespace WinkNaturals.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingController : ControllerBase
    {
        private readonly ExigoApiClient exigoApiClient = new ExigoApiClient(ExigoConfig.Instance.CompanyKey, ExigoConfig.Instance.LoginName, ExigoConfig.Instance.Password);

        private readonly IShoppingService _shoppingService;
        private readonly IMapper _mapper;
        public ShoppingController(IShoppingService shoppingService, IMapper mapper)
        {
            _shoppingService = shoppingService;
            _mapper = mapper;
        }

        /// <summary>
        /// Get item category
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetItemCategory/{webCategoryID:int}")]
        public IActionResult GetItemCategory(int webCategoryID)
        {
            return Ok(_shoppingService.GetItemCategory(webCategoryID));
        }

        /// <summary>
        /// GetProductList by categoryID
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetProductList/{categoryID:int}/{sortBy:int}")]
        public IActionResult GetProductList(int categoryID, int sortBy)
        {
            return Ok(_shoppingService.GetShopProducts(categoryID, sortBy));
        }

        /// <summary>
        /// GetProductDetailById by itemCode
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetProductDetailById/{itemCode}")]///{itemCode:string}
        public IActionResult GetProductDetailById(string itemCode)
        {
            string[] itemCodes = new string[1];
            itemCodes[0] = itemCode;
            return Ok(_shoppingService.GetProductDetailById(itemCodes));
        }

        /// <summary>
        /// GetProductImage by imageName
        /// </summary>
        /// <returns></returns>
        [HttpGet("ProductImage/{imageName}")]
        public IActionResult GetProductImage(string imageName)
        {
            try
            {
                var imageResponse = _shoppingService.GetProductImage(imageName);
                return File(imageResponse, "image/jpeg");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }


        [HttpGet("AddToCart")]
        public IActionResult AddToCart(ShopProductsResponse shopProducts)
        {
            var cartdata = _shoppingService.AddToCart(shopProducts);
            return Ok(cartdata);
        }

        /// <summary>
        /// SubmitCheckout
        /// </summary>
        /// <returns></returns>
        [HttpPost("SubmitCheckout")]
        public IActionResult SubmitCheckout(TransactionalRequestModel transactionRequests)
        {
            return Ok(_shoppingService.SubmitCheckout(transactionRequests));
        }







        /// <summary>
        /// CalculateOrder
        /// </summary>
        /// <returns></returns>
        [HttpPost("CalculateOrder")]
        public IActionResult CalculateOrder(CalculateOrderRequest calculateOrderReq, int shipMethodID = 0)
        {
            return Ok(_shoppingService.CalculateOrder(calculateOrderReq));
        }

        /// <summary>
        /// CreateOrder
        /// </summary>
        /// <returns></returns>
        [HttpPost("CreateOrder")]
        public IActionResult CreateOrder(CreateOrderRequest createOrderRequest)
        {
            return Ok(_shoppingService.CreateOrder(createOrderRequest));
        }

        /// <summary>
        /// CreateOrderImport
        /// </summary>
        /// <returns></returns>
        [HttpPost("CreateOrderImport")]
        public IActionResult CreateOrderImport(CreateOrderImportRequest createOrderImportRequest)
        {
            return Ok(_shoppingService.CreateOrderImport(createOrderImportRequest));
        }

        /// <summary>
        /// UpdateOrder
        /// </summary>
        /// <returns></returns>
        [HttpPost("UpdateOrder")]
        public IActionResult UpdateOrder(UpdateOrderRequest updateOrderRequest)
        {
            return Ok(_shoppingService.UpdateOrder(updateOrderRequest));
        }

        /// <summary>
        /// ChangeOrderStatus
        /// </summary>
        /// <returns></returns>
        [HttpPost("ChangeOrderStatus")]
        public IActionResult ChangeOrderStatus(ChangeOrderStatusRequest changeOrderStatusRequest)
        {
            return Ok(_shoppingService.ChangeOrderStatus(changeOrderStatusRequest));
        }

        /// <summary>
        /// ChangeOrderStatusBatch
        /// </summary>
        /// <returns></returns>
        [HttpPost("ChangeOrderStatusBatch")]
        public IActionResult ChangeOrderStatusBatch(ChangeOrderStatusBatchRequest changeOrderStatusBatchRequest)
        {
            return Ok(_shoppingService.ChangeOrderStatusBatch(changeOrderStatusBatchRequest));
        }

        /// <summary>
        /// ValidateCreditCardToken
        /// </summary>
        /// <returns></returns>
        [HttpPost("ValidateCreditCardToken")]
        public IActionResult ValidateCreditCardToken(ValidateCreditCardTokenRequest creditCardTokenRequest)
        {
            return Ok(_shoppingService.ValidateCreditCardToken(creditCardTokenRequest));
        }

        /// <summary>
        /// CreatePayment
        /// </summary>
        /// <returns></returns>
        [HttpPost("CreatePayment")]
        public IActionResult CreatePayment(CreatePaymentRequest createPaymentRequest)
        {
            return Ok(_shoppingService.CreatePayment(createPaymentRequest));
        }

        /// <summary>
        /// CreatePaymentCreditCard
        /// </summary>
        /// <returns></returns>
        [HttpPost("CreatePaymentCreditCard")]
        public IActionResult CreatePaymentCreditCard(CreatePaymentCreditCardRequest createPaymentCreditCardRequest)
        {
            return Ok(_shoppingService.CreatePaymentCreditCard(createPaymentCreditCardRequest));
        }

        /// <summary>
        /// CreatePaymentWallet
        /// </summary>
        /// <returns></returns>
        [HttpPost("CreatePaymentWallet")]
        public IActionResult CreatePaymentWallet(CreatePaymentWalletRequest createPaymentWalletRequest)
        {
            return Ok(_shoppingService.CreatePaymentWallet(createPaymentWalletRequest));
        }

        /// <summary>
        /// CreatePaymentPointAccount
        /// </summary>
        /// <returns></returns>
        [HttpPost("CreatePaymentPointAccount")]
        public IActionResult CreatePaymentPointAccount(CreatePaymentPointAccountRequest createPaymentPointAccountRequest)
        {
            return Ok(_shoppingService.CreatePaymentPointAccount(createPaymentPointAccountRequest));
        }

        /// <summary>
        /// CreatePaymentCheck
        /// </summary>
        /// <returns></returns>
        [HttpPost("CreatePaymentCheck")]
        public IActionResult CreatePaymentCheck(CreatePaymentCheckRequest createPaymentCheckRequest)
        {
            return Ok(_shoppingService.CreatePaymentCheck(createPaymentCheckRequest));
        }

        /// <summary>
        /// ChargeCreditCardToken
        /// </summary>
        /// <returns></returns>
        [HttpPost("ChargeCreditCardToken")]
        public IActionResult ChargeCreditCardToken(ChargeCreditCardTokenRequest chargeCreditCardTokenRequest)
        {
            return Ok(_shoppingService.ChargeCreditCardToken(chargeCreditCardTokenRequest));
        }

        /// <summary>
        /// ChargeCreditCardTokenOnFile
        /// </summary>
        /// <returns></returns>
        [HttpPost("ChargeCreditCardTokenOnFile")]
        public IActionResult ChargeCreditCardTokenOnFile(ChargeCreditCardTokenOnFileRequest chargeCreditCardTokenOnFileRequest)
        {
            return Ok(_shoppingService.ChargeCreditCardTokenOnFile(chargeCreditCardTokenOnFileRequest));
        }

        /// <summary>
        /// ChargeGroupOrderCreditCardToken
        /// </summary>
        /// <returns></returns>
        [HttpPost("ChargeGroupOrderCreditCardToken")]
        public IActionResult ChargeGroupOrderCreditCardToken(ChargeGroupOrderCreditCardTokenRequest chargeGroupOrderCredit)
        {
            return Ok(_shoppingService.ChargeGroupOrderCreditCardToken(chargeGroupOrderCredit));
        }

        /// <summary>
        /// RefundPriorCreditCardCharge
        /// </summary>
        /// <returns></returns>
        [HttpPost("RefundPriorCreditCardCharge")]
        public IActionResult RefundPriorCreditCardCharge(RefundPriorCreditCardChargeRequest refundPriorCredit)
        {
            return Ok(_shoppingService.RefundPriorCreditCardCharge(refundPriorCredit));
        }
        /// <summary>
        /// RefundPriorCreditCardCharge
        /// </summary>F
        /// <returns></returns>
        [HttpPost]
        [Route("checkout/shipping")]
        public IActionResult Shipping(VerifyAddressRequest addressRequest)
        {
            return Ok(_shoppingService.Shipping(addressRequest));
        }

        [HttpPost]
        [Route("checkout/GetshippingAddress")]
        public IActionResult GetCustomerAddress(int CustomerID)
        {
            return Ok(_shoppingService.GetCustomerAddress(CustomerID));
        }

        [HttpPost("AddUpdateCustomerAddress/{CustomerID:int}")]
        public IActionResult AddUpdateCustomerAddress(int CustomerID, ShippingAddress address)
        {
            //if ( Address.AddressType == AddressType.New)
            //{
            //    DAL.SetCustomerAddressOnFile(Identity.Customer.CustomerID, address as Address);
            //}
            return Ok(_shoppingService.AddUpdateCustomerAddress(CustomerID, address));

        }

        [HttpPost("GetWarehouses")]
        public IActionResult GetWarehouses(GetWarehousesRequest warehousesRequest)
        {
            return Ok(_shoppingService.GetWarehouses(warehousesRequest));
        }

        [HttpGet("GetOrder")]
        public IActionResult GetOrder(GetOrdersRequest ordersRequest)
        {
            return Ok(_shoppingService.GetOrder(ordersRequest));
        }
        [HttpGet("SearchProducts/{query}")]
        public IActionResult SearchProducts(string query)
        {
            return Ok(_shoppingService.SearchProducts(query));
        }

        // To implement Special Item block in Cart
        [HttpGet("GetSpecialItem")]
        public IActionResult GetSpecialItem()
        {
            return Ok(_shoppingService.GetSpecialItem());
        }

        [HttpGet("GetCustomerRealTime/{customerID:int}")]
        //To Get customer detail for editing.
        public IActionResult GetCustomerRealTime(int customerID)
        {
            return Ok(_shoppingService.GetCustomerRealTime(customerID));
        }

        //To update customer detail
        [HttpPost("UpdateCustomer")]
        public IActionResult UpdateCustomer(UpdateCustomerRequest updateCustomerRequest)
        {
            return Ok(_shoppingService.UpdateCustomer(updateCustomerRequest));
        }

        //Apply promocode
        [HttpGet("PromoCode/{promoCode}")]
        public IActionResult GetPromoCode(string promoCode)
        {
            return Ok(_shoppingService.GetPromoDetail(promoCode));
        }





    }
}
