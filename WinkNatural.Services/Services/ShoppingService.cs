using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using WinkNatural.Services.DTO.Shopping;
using WinkNatural.Services.DTO.Shopping.CalculateOrder;
using WinkNatural.Services.Interfaces;
using WinkNaturals.Models;
using Microsoft.AspNetCore.Mvc;
using WinkNatural.Services.Utilities;
using System.IO;
using System.Threading.Tasks;
using Exigo.Api.Client;
using WinkNatural.Services.DTO.Shopping.CreateOrder;


namespace WinkNatural.Services.Services
{
    public class ShoppingService : IShoppingService
    {
        private readonly ExigoApiClient exigoApiClient = new("WinkNaturals", "API_Web", "PB45DY5J5pmq9anE");


        /// <summary>
        /// GetItemCategory
        /// </summary>
        /// <param name="int categoryID, int pageSize, int pageIndex, string[] sizes = null, int sortBy = 0"></param>
        /// <returns>List of ItemCategoryResponse</returns>
        public List<ItemCategoryResponse> GetItemCategory(int webCategoryID)
        {

            webCategoryID = webCategoryID == 0 ? 1 : webCategoryID;
            var categories = new List<ItemCategoryResponse>();

            using (var context = Common.Utils.DbConnection.Sql())
            {
                var data = context.Query<ItemCategoryResponse>(QueryUtility.itemCategoryList_Query, new
                {
                    webid = 1,
                    webcategoryid = webCategoryID
                }).ToList();

                categories = GetWebCategoriesRecursively(categories, data, webCategoryID);
            }

            return categories.OrderBy(c => c.SortOrder).ToList();
        }

        //mk

        //public ShopProductsResponse GetShopProducts()
        /// <summary>
        /// GetShopProducts
        /// </summary>
        /// <param name="int categoryID, int pageSize, int pageIndex, string[] sizes = null, int sortBy = 0"></param>
        /// <returns>List of ShopProductsResponse</returns>
        public List<ShopProductsResponse> GetShopProducts(int categoryID, int pageSize, int pageIndex, string[] sizes = null, int sortBy = 0)
        {
            categoryID = categoryID == 0 ? 1 : categoryID;
            var categories = new List<ShopProductsResponse>();
            GetItemListRequest itemsRequest;
            var items = new List<ShopProductsResponse>();
            var newItems = new List<ShopProductsResponse>();

            itemsRequest = new GetItemListRequest
            {
                IncludeChildCategories = true,
                CategoryID = categoryID
            };
            items = GetItems(itemsRequest, false).OrderBy(c => c.SortOrder).ToList();
            return items;
        }

       

        //public ShopProductsResponse GetProductDetailById(int[] productIds)
        //{
        //    //dynamic response;
        //        using (var context = Common.Utils.DbConnection.Sql())
        //        {
        //      var  response= context.Query<ShopProductsResponse>(@"
        //            SELECT 
        //                 i.ItemID
        //                ,i.ItemCode
        //                ,i.ItemTypeID
        //                ,ISNULL(il.ItemDescription, i.ItemDescription) as ItemDescription
        //                ,ISNULL(il.ShortDetail, i.ShortDetail) as 'ShortDetail1'
        //                ,ISNULL(il.ShortDetail2, i.ShortDetail2) as 'ShortDetail2'
        //                ,ISNULL(il.ShortDetail3, i.ShortDetail3) as 'ShortDetail3'
        //                ,ISNULL(il.ShortDetail4, i.ShortDetail4) as 'ShortDetail4'
        //                ,ISNULL(il.LongDetail, i.LongDetail) as 'LongDetail1'
        //                ,ISNULL(il.LongDetail2, i.LongDetail2) as 'LongDetail2'
        //                ,ISNULL(il.LongDetail3, i.LongDetail3) as 'LongDetail3'
        //                ,ISNULL(il.LongDetail4, i.LongDetail4) as 'LongDetail4'
        //                ,i.TinyImageName as 'TinyImageUrl'
        //                ,i.SmallImageName as 'SmallImageUrl'
        //                ,i.LargeImageName as 'LargeImageUrl'
        //              FROM Items i
        //            LEFT JOIN ItemLanguages il
        //                ON il.ItemID = i.ItemID
        //                AND il.LanguageID = @languageID
        //              WHERE i.ItemID in @ids
        //        ", new
        //        {
        //            ids = productIds,
        //            languageID = (int)0
        //        }).ToList();
        //        //ShopProductsResponse shopProducts = new ShopProductsResponse();
        //        //shopProducts = response[0];
        //        return response[0];
        //    }
        //}

        /// <summary>
        /// GetProductDetailById
        /// </summary>
        /// <param name="itemCode of string type"></param>
        /// <returns>Product Detail</returns>
        public ShopProductsResponse GetProductDetailById(string[] itemCodes)
        {
            //dynamic response;
            using (var context = Common.Utils.DbConnection.Sql())
            {
                var response = context.Query<ShopProductsResponse>(QueryUtility.getProductDetailById_Query, new
                {
                    warehouse = 1,
                    currencyCode = "usd",
                    languageID = 0,
                    priceTypeID = 1,
                    itemCodes = itemCodes
                }).ToList();
                return response[0];
            }
        }

        /// <summary>
        /// GetProductImage
        /// </summary>
        /// <param name="imageName of string type"></param>
        /// <returns>byte[]</returns>
        public byte[] GetProductImage(string imageName)
        {
            try
            {
                object bytes;
                using (var context = Common.Utils.DbConnection.Sql())
                {
                    var query = QueryUtility.productImage_Query;

                    bytes = context.ExecuteScalar(query, new { Name = imageName });
                }
                var extension = Path.GetExtension(imageName).ToLower();
                string contentType = "image/jpeg";

                switch (extension)
                {
                    case ".gif":
                        contentType = "image/gif";
                        break;
                    case ".jpg":
                        contentType = "image/jpeg";
                        break;
                    case ".jpeg":
                        contentType = "image/png";
                        break;
                    case ".bmp":
                        contentType = "image/bmp";
                        break;
                    case ".png":
                        contentType = "image/png";
                        break;
                }

                return (byte[])bytes;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }



        public ShopProductsResponse AddToCart(ShopProductsResponse shopProducts)
        {
            ShopProductsResponse productsResponse = new ShopProductsResponse();
            productsResponse = null;

            return productsResponse;

        }

        /// <summary>
        /// SubmitCheckout
        /// </summary>
        /// <param name="TransactionalRequestModel"></param>
        /// <returns>TransactionalResponse</returns>
        public async Task<TransactionalResponse> SubmitCheckout(TransactionalRequestModel transactionRequest)
        {
            var res = new TransactionalResponse();
            try
            {
                var req = new TransactionalRequest();
                //Reserve 5 spots for the child requests
                req.TransactionRequests = new ITransactionMember[5];

                //Create the new customer Request
                // var custReq = new CreateCustomerRequest();
                var custReq = transactionRequest.createCustomerRequest;

                //Supply data for new order
                //var ordReq = new CreateOrderRequest();
                var ordReq = transactionRequest.createOrderRequest;


                ///Supply data for credit card token charge
                //var payReq = new ChargeCreditCardTokenRequest();
                var payReq = transactionRequest.chargeCreditCardTokenRequest;


                //Supply data for auto order record
                //var aoReq = new CreateAutoOrderRequest();
                var aoReq = transactionRequest.createAutoOrderRequest;


                //Supply data for card on file 
                //var ccReq = new SetAccountCreditCardTokenRequest();
                var ccReq = transactionRequest.setAccountCreditCardTokenRequest;


                //Supply data for Credit card on File

                //Pass requests into transaction request

                req.TransactionRequests[0] = custReq;

                req.TransactionRequests[1] = ordReq;

                req.TransactionRequests[2] = payReq;

                req.TransactionRequests[3] = aoReq;

                req.TransactionRequests[4] = ccReq;

                res = await exigoApiClient.ProcessTransactionAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;

        }

        /// <summary>
        /// CalculateOrder
        /// </summary>
        /// <param name="CalculateOrderRequest"></param>
        /// <returns>CalculateOrderResponse</returns>
        public async Task<CalculateOrderResponse> CalculateOrder(CalculateOrderRequest request)
        {
            var res = new CalculateOrderResponse();
            try
            {
                //if (request.Items.Count() == 0 || request.Address == null || string.IsNullOrEmpty(request.Address.Country) || string.IsNullOrEmpty(request.Address.State)) return new CalculateOrderResponse();
                //if (request.ShipMethodID == 0) request.ShipMethodID = request.Configuration.DefaultShipMethodID;
                //if (request.OrderTypeID == 0) request.OrderTypeID = OrderTypes.ShoppingCart;
                //CalculateOrderRequest calculateOrder = new CalculateOrderRequest();
                var req = new CalculateOrderRequest
                {
                    CustomerID = request.CustomerID,
                    WarehouseID = request.WarehouseID,
                    CurrencyCode = request.CurrencyCode,
                    PriceType = request.PriceType,
                    ShipMethodID = request.ShipMethodID,
                    City = request.City,
                    State = request.State,
                    Zip = request.Zip,
                    Country = request.Country,

                    Address1 = request.Address1,
                    Address2 = request.Address2,
                    Address3 = " ",

                    // CustomerID = request.CustomerID.Value,
                    OrderType = request.OrderType,
                    //County=calculateOrder.

                    //CurrencyCode = "usd", 
                    //WarehouseID = 1,            //Unique location for orders 
                    //ShipMethodID = 8, 
                    //PriceType = 1,              //Controls which price band to use 
                    //Address1 = "Some Address", 
                    //Address2 = "Some Address",  
                    //Address3 = "Some Address", 
                    //City = "Dallas", 
                    //State = "TX", 
                    //Zip = "1", 
                    //Country = "US",

                    //County = "1",

                    //CustomerID = 103082,             //Unique numeric identifier for a customer record.

                    //OrderType = Exigo.Api.Client.OrderType.Default
                };

                //Add Details

                var details = new List<Exigo.Api.Client.OrderDetailRequest>();

                foreach (var item in request.Details)
                {
                    var detail = new Exigo.Api.Client.OrderDetailRequest();
                    detail.ItemCode = item.ItemCode;
                    detail.Quantity = item.Quantity;
                    details.Add(detail);
                }

                //var detail1 = new Exigo.Api.Client.OrderDetailRequest();

                //detail1.ItemCode = "SK-Q1KIT3-21";

                //detail1.Quantity = 1;

                //details.Add(detail1);



                //var detail2 = new Exigo.Api.Client.OrderDetailRequest();

                //detail2.ItemCode = "SK-Q1KIT3-21";

                //detail2.Quantity = 2;

                //details.Add(detail2);



                //Now attach the list to the request

                req.Details = details.ToArray();

                req.ReturnShipMethods = true;

                req.PartyID = 1;

                req.CustomerKey = request.CustomerKey;//Unique alpha numeric identifier for customer record. Exeption will occur if CustomerID & CustomerKey are provided.

                res = await exigoApiClient.CalculateOrderAsync(req);

                var Details = res.Details;
                var Subtotal = res.SubTotal;
                var Shipping = res.ShippingTotal;
                var Tax = res.TaxTotal;
                var Discount = res.DiscountTotal;
                var Total = res.Total;


                // Assemble the ship methods, if requested
                //if (request.ReturnShipMethods)
                //{
                //    var shipMethods = new List<ShipMethod>();
                //    if (apiresponse.ShipMethods != null && apiresponse.ShipMethods.Length > 0)
                //    {
                //        var webShipMethods = new List<ShipMethod>();

                //        using (var context = Common.Utils.DbConnection.Sql())
                //        {
                //            webShipMethods = context.Query<ShipMethod>(@"
                //                                            SELECT 
                //                                                [ShipMethodID]
                //                                                ,[ShipMethodDescription]
                //                                                ,[WarehouseID]
                //                                                ,[ShipCarrierID]
                //                                                ,[DisplayOnWeb]
                //                                            FROM [dbo].[ShipMethods]
                //                                            WHERE DisplayOnWeb = 1 
                //                                                AND WarehouseID = @wid",
                //                                 new
                //                                 {
                //                                     wid = request.Configuration.WarehouseID
                //                                 }).ToList();
                //        }


                //        if (webShipMethods.Count() > 0)
                //        {
                //            var webShipMethodIds = webShipMethods.Select(s => s.ShipMethodID);
                //            foreach (var shipMethod in apiresponse.ShipMethods.Where(x => webShipMethodIds.Contains(x.ShipMethodID)))
                //            {
                //                shipMethods.Add(shipMethod as ShipMethod);
                //            }

                //            if (shipMethods.Any())
                //            {
                //                // Ensure that at least one ship method is selected
                //                var shipMethodID = (request.ShipMethodID != 0) ? request.ShipMethodID : request.Configuration.DefaultShipMethodID;
                //                if (shipMethods.Any(c => c.ShipMethodID == shipMethodID))
                //                {
                //                    shipMethods.First(c => c.ShipMethodID == shipMethodID).Selected = true;
                //                }
                //                else
                //                {
                //                    shipMethods.First().Selected = true;
                //                }
                //            }
                //        }
                //        else
                //        {
                //            throw new Exception("Error: You need at least one Ship Method set up as DisplayOnWeb.");
                //        }
                //    }
                //    //result.ShipMethods = shipMethods.AsEnumerable();
                //}


            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;

        }

        /// <summary>
        /// Create Order
        /// </summary>
        /// <param name="CreateOrderRequest"></param>
        /// <returns>CreateOrderResponse</returns>
        public async Task<CreateOrderResponse> CreateOrder(CreateOrderRequest orderRequest)
        {
            var res = new CreateOrderResponse();
            try
            {
                //var res = new CreateOrderDetailResponse();
                var req = new CreateOrderRequest();

                req.CustomerID = orderRequest.CustomerID;             //Unique numeric identifier for a customer record.

                req.OrderStatus = orderRequest.OrderStatus;           // OrderStatusType.Incomplete; 

                req.OrderDate = DateTime.Today;

                req.CurrencyCode = orderRequest.CurrencyCode;         // "1";

                req.WarehouseID = orderRequest.WarehouseID;                 //1;            //Unique location for orders

                req.ShipMethodID = orderRequest.ShipMethodID;             //1;

                req.PriceType = orderRequest.PriceType;                    //1;              //Controls which price band to use

                req.FirstName = orderRequest.FirstName; //"1";

                req.LastName = orderRequest.LastName; //"1";

                req.Company = orderRequest.Company; //"1";

                req.Address1 = orderRequest.Address1;  // "1";

                req.Address2 = orderRequest.Address2;  // "1";

                req.Address3 = orderRequest.Address3; // "1";

                req.City = orderRequest.City;  // "1";

                req.Zip = orderRequest.Zip;  // "1";

                req.County = orderRequest.Country; //"1";

                req.Email = orderRequest.Email; //"1";

                req.Phone = orderRequest.Phone; //"1";

                req.Notes = orderRequest.Notes; //"1";

                req.Other11 = orderRequest.Other11; //"1";

                req.Other12 = orderRequest.Other12;// //"1";

                req.Other13 = orderRequest.Other13; //"1";

                req.Other14 = orderRequest.Other14; //"1";

                req.Other15 = orderRequest.Other15; //"1";

                req.Other16 = orderRequest.Other16;// "1";

                req.Other17 = orderRequest.Other17; //"1";

                req.Other18 = orderRequest.Other18; //"1";

                req.Other19 = orderRequest.Other19;// "1";

                req.Other20 = orderRequest.Other20; //"1";

                req.OrderType = orderRequest.OrderType;  //OrderType.Default;

                req.TransferVolumeToID = orderRequest.TransferVolumeToID; // 1;     //Only pass in if you want volume to goto another CustomerID

                req.ReturnOrderID = orderRequest.ReturnOrderID;  //1;          //Unique numeric identifier for return order record.

                req.OverwriteExistingOrder = true;

                req.ExistingOrderID = orderRequest.ExistingOrderID; //1;        //Unique numeric identifier for existing order record.

                req.PartyID = orderRequest.PartyID;//1;



                //Add Details

                var details = new List<OrderDetailRequest>();

                foreach (var item in orderRequest.Details)
                {
                    var detail = new OrderDetailRequest();
                    detail.ItemCode = item.ItemCode;
                    detail.Quantity = detail.Quantity;

                    details.Add(detail);
                }

                //var detail1 = new OrderDetailRequest();

                //detail1.ItemCode = "1";

                //detail1.Quantity = 1;

                //details.Add(detail1);



                //var detail2 = new OrderDetailRequest();

                //detail2.ItemCode = "2";

                //detail2.Quantity = 2;

                //details.Add(detail2);



                //Now attach the list to the request

                req.Details = details.ToArray();

                req.SuppressPackSlipPrice = true;

                req.TransferVolumeToKey = orderRequest.TransferVolumeToKey;  //"1";  //Only pass in if you want volume to goto another Customer Key

                req.ReturnOrderKey = orderRequest.ReturnOrderKey; //"1";       //Unique alpha numeric identifier for customer record. Exeption will occur if ReturnOrderID & ReturnOrderKey are provided.

                req.ExistingOrderKey = orderRequest.ExistingOrderKey;// "1";     //Unique alpha numeric identifier for existing order record. Exeption will occur if ExistingOrderID & ExistingOrderKey are provided.

                req.CustomerKey = orderRequest.CustomerKey;  //"1";          //Unique alpha numeric identifier for customer record. Exeption will occur if OrderID & OrderKey are provided.



                //Send Request to Server and Get Response

                res = await exigoApiClient.CreateOrderAsync(req);



            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;
        }


        /// <summary>
        /// CreateOrderImport
        /// </summary>
        /// <param name="CreateOrderImportRequest"></param>
        /// <returns>CreateOrderImportResponse</returns>
        public async Task<CreateOrderImportResponse> CreateOrderImport(CreateOrderImportRequest createOrderImport)
        {
            var res = new CreateOrderImportResponse();
            try
            {
                var req = new CreateOrderImportRequest();
                req.CustomerID = createOrderImport.CustomerID;      //1;             //Unique numeric identifier for customer record.
                req.FirstName = createOrderImport.FirstName;
                req.LastName = createOrderImport.LastName;
                req.Company = createOrderImport.Company;
                req.Phone = createOrderImport.Phone;
                req.Email = createOrderImport.Email;
                req.ShipMethodID = createOrderImport.ShipMethodID;
                req.Address1 = createOrderImport.Address1;
                req.Address2 = createOrderImport.Address2;
                req.Address3 = createOrderImport.Address3;
                req.City = createOrderImport.City;
                req.State = createOrderImport.State;
                req.Zip = createOrderImport.Zip;
                req.Country = createOrderImport.Country;
                req.County = createOrderImport.County;
                req.Notes = createOrderImport.Notes;
                req.WarehouseID = createOrderImport.WarehouseID;            //Unique location for orders
                req.CurrencyCode = createOrderImport.CurrencyCode;
                req.ShippingStateTax = createOrderImport.ShippingStateTax;
                req.ShippingFedTax = createOrderImport.ShippingFedTax;
                req.ShippingCountyLocalTax = createOrderImport.ShippingCountyLocalTax;
                req.ShippingCountyTax = createOrderImport.ShippingCountyTax;
                req.ShippingCityLocalTax = createOrderImport.ShippingCityLocalTax;
                req.ShippingCityTax = createOrderImport.ShippingCityTax;
                req.Shipping = createOrderImport.Shipping;
                req.PriceType = createOrderImport.PriceType;              //Controls which price band to use
                req.OrderStatus = OrderStatusType.Incomplete;
                req.OrderDate = DateTime.Today;
                req.Other11 = createOrderImport.Other11;
                req.Other12 = createOrderImport.Other12;
                req.Other13 = createOrderImport.Other13;
                req.Other14 = createOrderImport.Other14;
                req.Other15 = createOrderImport.Other15;
                req.Other16 = createOrderImport.Other16;
                req.Other17 = createOrderImport.Other17;
                req.Other18 = createOrderImport.Other18;
                req.Other19 = createOrderImport.Other19;
                req.Other20 = createOrderImport.Other20;
                req.OrderType = createOrderImport.OrderType;

                var orderDetails = new List<OrderImportDetail>();
                foreach (var item in req.OrderDetails)
                {
                    var orderDetail = new OrderImportDetail();
                    orderDetail.ParentItemCode = item.ParentItemCode;
                    orderDetail.ItemCode = item.ItemCode;
                    orderDetail.Description = item.Description;
                    orderDetail.Qty = item.Qty;
                    orderDetail.WeightEach = item.WeightEach;
                    orderDetail.CountyLocalTax = item.CountyLocalTax;
                    orderDetail.CountyTax = item.CountyTax;
                    orderDetail.CityTax = item.CityTax;
                    orderDetail.StateTax = item.StateTax;
                    orderDetail.FedTax = item.FedTax;
                    orderDetail.TaxablePriceEach = item.TaxablePriceEach;
                    orderDetail.CVEach = item.CVEach;
                    orderDetail.BVEach = item.BVEach;
                    orderDetail.PriceEach = item.PriceEach;
                    orderDetail.Other10Each = item.Other10Each;
                    orderDetail.Other9Each = item.Other9Each;
                    orderDetail.Other8Each = item.Other8Each;
                    orderDetail.Other7Each = item.Other7Each;
                    orderDetail.Other6Each = item.Other6Each;
                    orderDetail.Other5Each = item.Other5Each;
                    orderDetail.Other4Each = item.Other4Each;
                    orderDetail.Other3Each = item.Other3Each;
                    orderDetail.Other2Each = item.Other2Each;
                    orderDetail.Other1Each = item.Other1Each;
                    orderDetail.Reference1 = item.Reference1;
                    orderDetails.Add(orderDetail);
                }
                req.OrderDetails = orderDetails.ToArray();
                req.PartyID = createOrderImport.PartyID;
                req.ManualOrderKey = createOrderImport.ManualOrderKey;       //Unique alpha numeric identifier for order record. Exeption will occur if OrderID & OrderKey are provided.
                req.ReturnOrderKey = createOrderImport.ReturnOrderKey;       //Unique alpha numeric identifier for return order record. Exeption will occur if ReturnOrderID & ReturnOrderKey are provided.
                req.CustomerKey = createOrderImport.CustomerKey;          //Unique alpha numeric identifier for customer record. Exeption will occur if CustomerID & CustomerKey are provided.
                req.OverwriteExistingOrder = createOrderImport.OverwriteExistingOrder ? createOrderImport.OverwriteExistingOrder : true;
                req.ExistingOrderID = createOrderImport.ExistingOrderID;        //Unique numeric identifier for existing order record.
                req.ExistingOrderKey = createOrderImport.ExistingOrderKey;     //Unique alpha numeric identifier for existing order record. Exeption will occur if ExistingOrderID & ExistingOrderKey are provided.
                req.IsCommissionable = createOrderImport.IsCommissionable ? createOrderImport.IsCommissionable : true;

                //Send Request to Server and Get Response

                res = await exigoApiClient.CreateOrderImportAsync(req);


            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;

        }

        /// <summary>
        /// UpdateOrder
        /// </summary>
        /// <param name="UpdateOrderRequest"></param>
        /// <returns>UpdateOrderResponse</returns>
        public async Task<UpdateOrderResponse> UpdateOrder(UpdateOrderRequest updateOrderRequest)
        {
            var res = new UpdateOrderResponse();
            try
            {
                var req = new UpdateOrderRequest();
                req.OrderID = updateOrderRequest.OrderID;                //Unique numeric identifier for order record.
                req.OrderDate = DateTime.Today;
                req.DeclineCount = updateOrderRequest.DeclineCount;
                req.OrderTy = updateOrderRequest.OrderTy;
                req.OrderStatus = updateOrderRequest.OrderStatus;
                req.PriceTy = updateOrderRequest.PriceTy;                //Controls which price band to use
                req.Total = updateOrderRequest.Total;
                req.SubTotal = updateOrderRequest.SubTotal;
                req.Shipping = updateOrderRequest.Shipping;
                req.OrderTax = updateOrderRequest.OrderTax;
                req.ShippingTax = updateOrderRequest.ShippingTax;
                req.FedShippingTax = updateOrderRequest.FedShippingTax;
                req.StateShippingTax = updateOrderRequest.StateShippingTax;
                req.CityShippingTax = updateOrderRequest.CityShippingTax;
                req.CityLocalShippingTax = updateOrderRequest.CityLocalShippingTax;
                req.CountyShippingTax = updateOrderRequest.CountyShippingTax;
                req.CountyLocalShippingTax = updateOrderRequest.CountyLocalShippingTax;
                req.ManualTaxRate = updateOrderRequest.ManualTaxRate;
                req.TotalTax = updateOrderRequest.TotalTax;
                req.CurrencyCode = updateOrderRequest.CurrencyCode;
                req.PaymentMethod = updateOrderRequest.PaymentMethod;
                req.WarehouseID = updateOrderRequest.WarehouseID;            //Unique location for orders
                req.BatchID = updateOrderRequest.BatchID;
                req.PreviousBalance = updateOrderRequest.PreviousBalance;
                req.OverrideShipping = updateOrderRequest.OverrideShipping;
                req.OverrideTax = updateOrderRequest.OverrideTax;
                req.BusinessVolume = updateOrderRequest.BusinessVolume;         //Price amount
                req.CommissionableVolume = updateOrderRequest.CommissionableVolume;   //Price amount
                req.Discount = updateOrderRequest.Discount;
                req.DiscountPercent = updateOrderRequest.DiscountPercent;
                req.Weight = updateOrderRequest.Weight;
                req.Sourcety = updateOrderRequest.Sourcety;
                req.Notes = updateOrderRequest.Notes;
                req.Usr = updateOrderRequest.Usr;
                req.FirstName = updateOrderRequest.FirstName;
                req.LastName = updateOrderRequest.LastName;
                req.Company = updateOrderRequest.Company;
                req.Address1 = updateOrderRequest.Address1;
                req.Address2 = updateOrderRequest.Address2;
                req.City = updateOrderRequest.City;
                req.Zip = updateOrderRequest.Zip;
                req.Email = updateOrderRequest.Email;
                req.Phone = updateOrderRequest.Phone;
                req.SuppressPackSlipPrice = updateOrderRequest.SuppressPackSlipPrice;
                req.ShipMethodID = updateOrderRequest.ShipMethodID;
                req.AutoOrderID = updateOrderRequest.AutoOrderID;
                req.CreatedBy = updateOrderRequest.CreatedBy;
                req.ReturnOrderID = updateOrderRequest.ReturnOrderID;          //Unique numeric identifier for transfer to order record.
                req.OrderRankID = updateOrderRequest.OrderRankID;
                req.OrderPayRankID = updateOrderRequest.OrderPayRankID;
                req.AddressIsVerified = updateOrderRequest.AddressIsVerified;
                req.County = updateOrderRequest.County;
                req.IsRMA = updateOrderRequest.IsRMA;
                req.BackOrderFromID = updateOrderRequest.BackOrderFromID;        //Unique numeric identifier for transfer to order record.
                req.CreditsEarned = updateOrderRequest.CreditsEarned;
                req.TotalFedTax = updateOrderRequest.TotalFedTax;
                req.TotalStateTax = updateOrderRequest.TotalStateTax;
                req.ManualShippingTax = updateOrderRequest.ManualShippingTax;
                req.ReplacementOrderID = updateOrderRequest.ReplacementOrderID;     //Unique numeric identifier for transfer to order record.
                req.LockedDate = DateTime.Today;
                req.CommissionedDate = DateTime.Today;
                req.Flag1 = updateOrderRequest.Flag1;
                req.Flag2 = updateOrderRequest.Flag2;
                req.Flag3 = updateOrderRequest.Flag3;
                req.OriginalWarehouseID = updateOrderRequest.OriginalWarehouseID;
                req.PickupName = updateOrderRequest.PickupName;
                req.TransferToID = updateOrderRequest.TransferToID;           //Unique numeric identifier for transfer to order record.
                req.IsCommissionable = updateOrderRequest.IsCommissionable;
                req.FulfilledBy = updateOrderRequest.FulfilledBy;
                req.CreditApplied = updateOrderRequest.CreditApplied;
                req.ShippedDate = DateTime.Today;
                req.TaxLockDate = DateTime.Today;
                req.TotalTaxable = updateOrderRequest.TotalTaxable;
                req.ReturnCategoryID = updateOrderRequest.ReturnCategoryID;
                req.ReplacementCategoryID = updateOrderRequest.ReplacementCategoryID;
                req.CalculatedShipping = updateOrderRequest.CalculatedShipping;
                req.HandlingFee = updateOrderRequest.HandlingFee;
                req.OrderProcessTy = updateOrderRequest.OrderProcessTy;
                req.ActualCarrier = updateOrderRequest.ActualCarrier;
                req.ParentOrderID = updateOrderRequest.ParentOrderID;          //Unique numeric identifier for transfer to order record.
                req.CustomerTy = updateOrderRequest.CustomerTy;             //Classification of a customer as defined by the company
                req.Reference = updateOrderRequest.Reference;
                req.MiddleName = updateOrderRequest.MiddleName;
                req.NameSuffix = updateOrderRequest.NameSuffix;
                req.Address3 = updateOrderRequest.Address3;
                req.PartyID = updateOrderRequest.PartyID;
                req.TrackingNumber1 = updateOrderRequest.TrackingNumber1;
                req.TrackingNumber2 = updateOrderRequest.TrackingNumber2;
                req.TrackingNumber3 = updateOrderRequest.TrackingNumber3;
                req.TrackingNumber4 = updateOrderRequest.TrackingNumber4;
                req.TrackingNumber5 = updateOrderRequest.TrackingNumber5;
                req.WebCarrierID = OrderShipCarrier.FedEx;
                req.WebCarrierID2 = OrderShipCarrier.FedEx;
                req.WebCarrierID3 = OrderShipCarrier.FedEx;
                req.WebCarrierID4 = OrderShipCarrier.FedEx;
                req.WebCarrierID5 = OrderShipCarrier.FedEx;
                req.Other11 = updateOrderRequest.Other11;
                req.Other12 = updateOrderRequest.Other12;
                req.Other13 = updateOrderRequest.Other13;
                req.Other14 = updateOrderRequest.Other14;
                req.Other15 = updateOrderRequest.Other15;
                req.Other16 = updateOrderRequest.Other16;
                req.Other17 = updateOrderRequest.Other17;
                req.Other18 = updateOrderRequest.Other18;
                req.Other19 = updateOrderRequest.Other19;
                req.Other20 = updateOrderRequest.Other20;
                req.TransferToKey = updateOrderRequest.TransferToKey;        //Unique alpha numeric identifier for transfer to order record. Exeption will occur if TransferToID & TransferToKey are provided.
                req.OrderKey = updateOrderRequest.OrderKey; //Unique alpha numeric identifier for order record. Exeption will occur if OrderID & OrderKey are provided.
                req.ReturnOrderKey = updateOrderRequest.ReturnOrderKey;       //Unique alpha numeric identifier for transfer to order record. Exeption will occur if ReturnOrderID & ReturnOrderKey are provided.
                req.ReplacementOrderKey = updateOrderRequest.ReplacementOrderKey;  //Unique alpha numeric identifier for transfer to order record. Exeption will occur if ReplacementOrderID & ReplacementOrderKey are provided.
                req.ParentOrderKey = updateOrderRequest.ParentOrderKey;       //Unique alpha numeric identifier for transfer to order record. Exeption will occur if ParentOrderID & ParentOrderKey are provided.
                req.BackOrderFromKey = updateOrderRequest.BackOrderFromKey;
                res = await exigoApiClient.UpdateOrderAsync(req);//
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }


            return res;
        }

        /// <summary>
        /// Update the status of an order
        /// </summary>
        /// <param name="ChangeOrderStatusRequest"></param>
        /// <returns>ChangeOrderStatusResponse</returns>
        public async Task<ChangeOrderStatusResponse> ChangeOrderStatus(ChangeOrderStatusRequest changeOrderStatusRequest)
        {
            var res = new ChangeOrderStatusResponse();
            try
            {
                var req = new ChangeOrderStatusRequest();
                req.OrderStatus = changeOrderStatusRequest.OrderStatus;
                req.OrderKey = changeOrderStatusRequest.OrderKey;
                res = await exigoApiClient.ChangeOrderStatusAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;

        }

        /// <summary>
        ///ChangeOrderStatusBatch
        /// </summary>
        /// <param name="ChangeOrderStatusBatchRequest"></param>
        /// <returns>ChangeOrderStatusBatchResponse</returns>
        public async Task<ChangeOrderStatusBatchResponse> ChangeOrderStatusBatch(ChangeOrderStatusBatchRequest changeOrderStatusBatchRequest)
        {
            var res = new ChangeOrderStatusBatchResponse();
            try
            {
                var req = new ChangeOrderStatusBatchRequest();
                req.OrderStatus = changeOrderStatusBatchRequest.OrderStatus;

                var details = new List<OrderBatchDetailRequest>();
                foreach (var item in changeOrderStatusBatchRequest.Details)
                {
                    var detail = new OrderBatchDetailRequest();
                    detail.TrackingNumber1 = item.TrackingNumber1;
                    detail.OrderKey = item.OrderKey;
                    details.Add(detail);
                }
                req.Details = details.ToArray();
                res = await exigoApiClient.ChangeOrderStatusBatchAsync(req);

            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;

        }

        /// <summary>
        ///CreatePayment
        /// </summary>
        /// <param name="CreatePaymentRequest"></param>
        /// <returns>CreatePaymentResponse</returns>
        public async Task<CreatePaymentResponse> CreatePayment(CreatePaymentRequest createPaymentRequest)
        {
            var res = new CreatePaymentResponse();
            try
            {
                //Create Request
                var req = new CreatePaymentRequest();
                //req.PaymentDate = DateTime.Today;
                //req.Amount = 1;
                //req.PaymentType = PaymentType.Cash;
                //req.OrderKey = "1";             //Unique alpha numeric identifier for order record. Exeption will occur if OrderID & OrderKey are provided.

                req.PaymentDate = DateTime.Today;
                req.Amount = createPaymentRequest.Amount;
                req.PaymentType = createPaymentRequest.PaymentType;
                req.OrderKey = createPaymentRequest.OrderKey;
                //Send Request to Server and Get Response
                res = await exigoApiClient.CreatePaymentAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;

        }

        /// <summary>
        ///CreatePaymentCreditCard
        /// </summary>
        /// <param name="CreatePaymentCreditCardRequest"></param>
        /// <returns>CreatePaymentCreditCardResponse</returns>
        public async Task<CreatePaymentCreditCardResponse> CreatePaymentCreditCard(CreatePaymentCreditCardRequest createPaymentCreditCardRequest)
        {
            var res = new CreatePaymentCreditCardResponse();
            try
            {
                //Create Request

                var req = new CreatePaymentCreditCardRequest();
                //req.OrderID = 1;                //Unique numeric identifier for order record.
                //req.PaymentDate = DateTime.Today;
                //req.Amount = 1;
                //req.CreditCardNumber = "1";
                //req.ExpirationMonth = 1;
                //req.ExpirationYear = 1;
                //req.BillingName = "1";
                //req.BillingAddress = "1";
                //req.BillingAddress2 = "1";
                //req.BillingCity = "1";
                //req.BillingZip = "1";
                //req.AuthorizationCode = "1";
                //req.Memo = "1";
                //req.OrderKey = "1";             //Unique alpha numeric identifier for order record. Exeption will occur if OrderID & OrderKey are provided.
                //req.MerchantTransactionKey = "1";

                req.OrderID = createPaymentCreditCardRequest.OrderID;                //Unique numeric identifier for order record.
                req.PaymentDate = DateTime.Today;
                req.Amount = createPaymentCreditCardRequest.Amount;
                req.CreditCardNumber = createPaymentCreditCardRequest.CreditCardNumber;
                req.ExpirationMonth = createPaymentCreditCardRequest.ExpirationMonth;
                req.ExpirationYear = createPaymentCreditCardRequest.ExpirationYear;
                req.BillingName = createPaymentCreditCardRequest.BillingName;
                req.BillingAddress = createPaymentCreditCardRequest.BillingAddress;
                req.BillingAddress2 = createPaymentCreditCardRequest.BillingAddress2;
                req.BillingCity = createPaymentCreditCardRequest.BillingCity;
                req.BillingZip = createPaymentCreditCardRequest.BillingZip;
                req.AuthorizationCode = createPaymentCreditCardRequest.AuthorizationCode;
                req.Memo = createPaymentCreditCardRequest.Memo;
                req.OrderKey = createPaymentCreditCardRequest.OrderKey;             //Unique alpha numeric identifier for order record. Exeption will occur if OrderID & OrderKey are provided.
                req.MerchantTransactionKey = createPaymentCreditCardRequest.MerchantTransactionKey;



                //Send Request to Server and Get Response

                res = await exigoApiClient.CreatePaymentCreditCardAsync(req);
            }
            catch (Exception)
            {

                throw;
            }
            return res;

        }

        /// <summary>
        ///CreatePaymentWallet
        /// </summary>
        /// <param name="CreatePaymentWalletRequest"></param>
        /// <returns>CreatePaymentResponse</returns>
        public async Task<CreatePaymentResponse> CreatePaymentWallet(CreatePaymentWalletRequest createPaymentWalletRequest)
        {
            var res = new CreatePaymentResponse();
            try
            {
                var req = new CreatePaymentWalletRequest();
                req.PaymentDate = DateTime.Today;
                req.Amount = createPaymentWalletRequest.Amount;
                req.WalletAccount = createPaymentWalletRequest.WalletAccount;
                req.AuthorizationCode = createPaymentWalletRequest.AuthorizationCode;
                req.Memo = createPaymentWalletRequest.Memo;
                req.BillingName = createPaymentWalletRequest.BillingName;
                req.OrderKey = createPaymentWalletRequest.OrderKey;             //Unique alpha numeric identifier for order record. Exeption will occur if OrderID & OrderKey are provided.

                //Send Request to Server and Get Response

                res = await exigoApiClient.CreatePaymentWalletAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }

            return res;
        }

        /// <summary>
        ///ValidateCreditCardToken
        /// </summary>
        /// <param name="ValidateCreditCardTokenRequest"></param>
        /// <returns>ValidateCreditCardTokenResponse</returns>
        public async Task<ValidateCreditCardTokenResponse> ValidateCreditCardToken(ValidateCreditCardTokenRequest creditCardTokenRequest)
        {
            var res = new ValidateCreditCardTokenResponse();
            try
            {
                var req = new ValidateCreditCardTokenRequest();
                //req.CreditCardIdentifier = "1";
                //req.ExpirationYear = 1;
                //req.ExpirationMonth = 1;
                //req.CvcCode = "1";
                //req.BillingName = "1";
                //req.BillingAddress1 = "1";
                //req.BillingAddress2 = "1";
                //req.BillingCity = "1";
                //req.BillingZip = "1";
                //req.CustomerID = 1;             //Unique numeric identifier for a customer record.
                //req.CustomerKey = "DDks8235txcid";//Unique alpha numeric identifier for customer record. Exeption will occur if CustomerID & CustomerKey are provided.
                //req.OrderID = 1;                //Unique numeric identifier for order record.
                //req.OrderKey = "DDks8235txcid"; //Unique alpha numeric identifier for order record. Exeption will occur if OrderID & OrderKey are provided.
                //req.ValidateTokenOnFile = true;    //Optional. Use this option to validate information already on file
                //req.AccountOnFile = TokenAccount.Primary;          //Optional. To be used when ValidateTokenOnFile = true
                //req.WarehouseID = 1;            //Optional
                //req.CurrencyCode = "1";         //Optional
                //req.Email = "1";                //Optional
                //req.Phone = "1";                //Optional
                //req.ClientIpAddress = "1";      //Optional


                req.CreditCardIdentifier = creditCardTokenRequest.CreditCardIdentifier;
                req.ExpirationYear = creditCardTokenRequest.ExpirationYear;
                req.ExpirationMonth = creditCardTokenRequest.ExpirationMonth;
                req.CvcCode = creditCardTokenRequest.CvcCode;
                req.BillingName = creditCardTokenRequest.BillingName;
                req.BillingAddress1 = creditCardTokenRequest.BillingAddress1;
                req.BillingAddress2 = creditCardTokenRequest.BillingAddress2;
                req.BillingCity = creditCardTokenRequest.BillingCity;
                req.BillingZip = creditCardTokenRequest.BillingZip;
                req.CustomerID = creditCardTokenRequest.CustomerID;             //Unique numeric identifier for a customer record.
                req.CustomerKey = creditCardTokenRequest.CustomerKey;//Unique alpha numeric identifier for customer record. Exeption will occur if CustomerID & CustomerKey are provided.
                req.OrderID = creditCardTokenRequest.OrderID;                //Unique numeric identifier for order record.
                req.OrderKey = creditCardTokenRequest.OrderKey; //Unique alpha numeric identifier for order record. Exeption will occur if OrderID & OrderKey are provided.
                req.ValidateTokenOnFile = creditCardTokenRequest.ValidateTokenOnFile;    //Optional. Use this option to validate information already on file
                req.AccountOnFile = creditCardTokenRequest.AccountOnFile;          //Optional. To be used when ValidateTokenOnFile = true
                req.WarehouseID = creditCardTokenRequest.WarehouseID;            //Optional
                req.CurrencyCode = creditCardTokenRequest.CurrencyCode;         //Optional
                req.Email = creditCardTokenRequest.Email;                //Optional
                req.Phone = creditCardTokenRequest.Phone;                //Optional
                req.ClientIpAddress = creditCardTokenRequest.ClientIpAddress;      //Optional

                //Send Request to Server and Get Response

                res = await exigoApiClient.ValidateCreditCardTokenAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
                throw;
            }


            return res;
        }

        /// <summary>
        ///CreatePaymentPointAccount
        /// </summary>
        /// <param name="CreatePaymentPointAccountRequest"></param>
        /// <returns>CreatePaymentPointAccountResponse</returns>
        public async Task<CreatePaymentPointAccountResponse> CreatePaymentPointAccount(CreatePaymentPointAccountRequest createPaymentPointAccountRequest)
        {
            var res = new CreatePaymentPointAccountResponse();
            try
            {
                //Create Request
                var req = new CreatePaymentPointAccountRequest();
                req.PointAccountID = createPaymentPointAccountRequest.PointAccountID;
                req.PaymentDate = DateTime.Today;
                req.Amount = createPaymentPointAccountRequest.Amount;
                req.OrderKey = createPaymentPointAccountRequest.OrderKey;             //Unique alpha numeric identifier for order record. Exeption will occur if OrderID & OrderKey are provided.
                                                //Send Request to Server and Get Response
                res = await exigoApiClient.CreatePaymentPointAccountAsync(req);
                //Now examine the results:
                //Console.WriteLine("PaymentID: {0}", res.PaymentID);
                //Console.WriteLine("Message: {0}", res.Message);
                //Console.WriteLine("DisplayMessage: {0}", res.DisplayMessage);
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            return res;
        }

        /// <summary>
        ///CreatePaymentCheck
        /// </summary>
        /// <param name="CreatePaymentCheckRequest"></param>
        /// <returns>CreatePaymentCheckResponse</returns>
        public async Task<CreatePaymentCheckResponse> CreatePaymentCheck(CreatePaymentCheckRequest createPaymentCheckRequest)
        {
            var res = new CreatePaymentCheckResponse();
            try
            {
                var req = new CreatePaymentCheckRequest();
                req.PaymentDate = DateTime.Today;
                req.Amount = createPaymentCheckRequest.Amount;
                req.OrderKey = createPaymentCheckRequest.OrderKey;             //Unique alpha numeric identifier for order record. Exeption will occur if OrderID & OrderKey are provided.
                                                                               //Send Request to Server and Get Response
                res = await exigoApiClient.CreatePaymentCheckAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;
        }

        /// <summary>
        ///ChargeCreditCardToken
        /// </summary>
        /// <param name="ChargeCreditCardTokenRequest"></param>
        /// <returns>ChargeCreditCardResponse</returns>
        public async Task<ChargeCreditCardResponse> ChargeCreditCardToken(ChargeCreditCardTokenRequest chargeCreditCardTokenRequest)
        {
            var res = new ChargeCreditCardResponse();
            try
            {
                var req = new ChargeCreditCardTokenRequest();
                req.CreditCardToken = chargeCreditCardTokenRequest.CreditCardToken;
                req.BillingName = chargeCreditCardTokenRequest.BillingName;
                req.BillingAddress = chargeCreditCardTokenRequest.BillingAddress;
                req.BillingAddress2 = chargeCreditCardTokenRequest.BillingAddress2;
                req.BillingCity = chargeCreditCardTokenRequest.BillingCity;
                req.BillingZip = chargeCreditCardTokenRequest.BillingZip;
                req.ExpirationMonth = chargeCreditCardTokenRequest.ExpirationMonth;
                req.ExpirationYear = chargeCreditCardTokenRequest.ExpirationYear;
                req.OrderKey = chargeCreditCardTokenRequest.OrderKey;             //Unique alpha numeric identifier for order record. Exeption will occur if OrderID & OrderKey are provided.
                                                                                  //Send Request to Server and Get Response
                res = await exigoApiClient.ChargeCreditCardTokenAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
                //throw;
            }
            return res;


        }


        #region Private methods

        [NonAction]
        public static IEnumerable<ShopProductsResponse> GetItems(GetItemListRequest request, bool includeItemDescriptions = true)
        {
            var tempCategoryIDs = new List<int>();
            var categoryIDs = new List<int>();
            if (request.CategoryID != null)
            {
                // Get all category ids underneath the request's category id
                if (request.IncludeChildCategories)
                {
                    using (var context = Common.Utils.DbConnection.Sql())
                    {


                        categoryIDs.AddRange(context.Query<int>(QueryUtility.categoryIdList_Query, new
                        {
                            webid = 1,
                            masterCategoryID = request.CategoryID
                        }).ToList());
                    }
                }
                else
                {
                    categoryIDs.Add(Convert.ToInt32(request.CategoryID));
                }
            }

            // If we requested specific categories, get the item codes in the categories
            if (categoryIDs.Count > 0)
            {
                var categoryItemCodes = new List<string>();

                using (var context = Common.Utils.DbConnection.Sql())
                {
                    categoryItemCodes = context.Query<string>(QueryUtility.categoryItemCodesList_Query, new
                    {
                        webid = 1,
                        webcategoryids = categoryIDs
                    }).ToList();
                }

                var existingItemCodes = request.ItemCodes.ToList();
                existingItemCodes.AddRange(categoryItemCodes);
                request.ItemCodes = existingItemCodes.ToArray();
            }

            // Do a final check to ensure if the category we are looking at does not contain a item directly nested within it, we pull back the first child category
            if (request.ItemCodes.Length == 0 && request.CategoryID != null)
            {
                var tempItemCodeList = new List<string>();
                using (var context = Common.Utils.DbConnection.Sql())
                {
                    tempItemCodeList = context.Query<string>(QueryUtility.tempItemCodeList_Query, new
                    {
                        webid = 1,
                        masterCategoryID = request.CategoryID
                    }).ToList();
                }

                request.ItemCodes = tempItemCodeList.ToArray();
            }


            // If we don't have any items, stop here.
            if (request.ItemCodes.Length == 0) yield break;

            // get the item information             
            var priceTypeID = request.PriceTypeID;

            var items = GetItemInformation(request, priceTypeID);  //: GetItemList(request, priceTypeID);

            // Populate the group members and dynamic kits
            if (items.Any())
            {
                // PopulateAdditionalItemData(items, request);
            }

            if (request.SortBy == 1)
            {
                // Newest Arrivals
                items = items.OrderByDescending(x => x.ItemID).ToList();
            }
            if (request.SortBy == 2)
            {
                // Price: $ - $$
                items = items.OrderBy(x => x.Price).ToList();
            }
            else if (request.SortBy == 3)
            {
                // Price: $$ - $
                items = items.OrderByDescending(x => x.Price).ToList();
            }
            else if (request.SortBy == 4)
            {
                // Name: A - Z
                items = items.OrderBy(x => x.ItemDescription).ToList();
            }
            else
            {
                // Featured          
            }

            // Return the data
            foreach (var item in items)
            {
                yield return item;
            }
        }


        [NonAction]
        private static List<ShopProductsResponse> GetItemInformation(GetItemListRequest request, int priceTypeID)
        {
            try
            {
                var apiItems = new List<ShopProductsResponse>();
                string sorting = string.Empty;
                sorting = request.SortBy switch
                {
                    2 => "ip.price, i.itemcode desc",// Price: $ - $$
                    3 => "ip.price desc, i.itemcode desc",// Price: $$ - $
                    4 => "i.ItemDescription, i.itemcode",// Name: A - Z
                    _ => "i.itemId desc",// Newest Arrivals
                };
                int languageID = request.LanguageID;
                List<string> itemCodes = request.ItemCodes.ToList();

                using var context = Common.Utils.DbConnection.Sql();
                apiItems = context.Query<ShopProductsResponse>(QueryUtility.apiItemsListForProducts_Query + sorting + @"
                            ", new
                {
                    warehouse = 1,
                    currencyCode = "usd",
                    languageID = languageID,
                    priceTypeID = 1,
                    itemCodes = itemCodes
                }).ToList();

                var length = request.ItemCodes.Count();
                var orderedItems = new List<ShopProductsResponse>();
                // Handle Sorting here, the sort order was based on the order of the Item Codes passed in originally
                for (var i = 0; i < length; i++)
                {
                    var matchingItem = apiItems.FirstOrDefault(c => c.ItemCode == request.ItemCodes[i]);

                    if (matchingItem != null)
                    {
                        orderedItems.Add(matchingItem);
                    }
                }

                foreach (var item in orderedItems)
                {
                    //item.ProductImage = ProductImageUtility.GetProductImageUtility(item.LargeImageUrl);
                    item.ProductImage = null;
                }


                var data = orderedItems;
                return data;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        [NonAction]
        private static List<ItemCategoryResponse> GetWebCategoriesRecursively(List<ItemCategoryResponse> categories, List<ItemCategoryResponse> data, int parentID)
        {
            foreach (var category in data.Where(c => c.ParentID == parentID))
            {
                categories.Add(category);
                if (data.Count(c => c.ParentID == category.WebCategoryID) > 0)
                {
                    GetWebCategoriesRecursively(category.Subcategories, data, category.WebCategoryID);
                }
            }

            return categories.OrderBy(c => c.SortOrder).ToList();
        }
        #endregion
    }
}
