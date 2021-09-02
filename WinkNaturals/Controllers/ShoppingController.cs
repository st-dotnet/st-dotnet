using Microsoft.AspNetCore.Mvc;
using Resources;
using System;
using WinkNatural.Services.Interfaces;
using WinkNatural.Services.Utilities;
using WinkNatural.Services.DTO.Shopping;
using WinkNaturals.Models;
using Exigo.Api.Client;
using AutoMapper;


namespace WinkNaturals.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingController : ControllerBase
    {
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
        [HttpGet("GetProductList/{categoryID:int}")]
        public IActionResult GetProductList(int categoryID)
        {
            return Ok(_shoppingService.GetShopProducts(categoryID));
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
        public IActionResult SubmitCheckout(TransactionalRequestModel transactionRequest)
        {
            return Ok(_shoppingService.SubmitCheckout(transactionRequest));
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
        public IActionResult CreatePaymentCheck(CreatePaymentCheckRequest  createPaymentCheckRequest)
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


        
    }
}
