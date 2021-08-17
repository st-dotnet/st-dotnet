using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using WinkNatural.Services.DTO.Shopping;
using WinkNatural.Services.Interfaces;
using WinkNaturals.Models;
using Microsoft.AspNetCore.Mvc;
using WinkNatural.Services.Utilities;
using System.IO;

namespace WinkNatural.Services.Services
{
    public class ShoppingService : IShoppingService
    {
        public List<ItemCategoryResponse> GetItemCategory(int webCategoryID)
        { 
            webCategoryID = webCategoryID == 0 ? 1 : webCategoryID; 
            var categories = new List<ItemCategoryResponse>();

            using (var context = Common.Utils.DbConnection.Sql())
            {
                var data = context.Query<ItemCategoryResponse>(@"
			        ;WITH webcat (WebCategoryID, WebCategoryDescription, ParentID, NestedLevel, SortOrder) 
				         AS (SELECT WebCategoryID, 
							        WebCategoryDescription, 
							        ParentID = COALESCE(ParentID, 0), 
							        NestedLevel,
                                    SortOrder
					         FROM   WebCategories 
					         WHERE  WebCategoryID = @webcategoryid
							        AND WebID = @webid 
					         UNION ALL 
					         SELECT w.WebCategoryID, 
							        w.WebCategoryDescription, 
							        w.ParentID, 
							        w.NestedLevel,
                                    w.SortOrder
					         FROM   WebCategories w 
							        INNER JOIN webcat c 
									        ON c.WebCategoryID = w.ParentID) 
			        SELECT * 
			        FROM   webcat 
		        ", new
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
        public List<ShopProductsResponse> GetShopProducts(int categoryID , int pageSize, int pageIndex , string[] sizes = null, int sortBy = 0)
        {
            categoryID = categoryID == 0 ? 1 : categoryID;
            var categories = new List<ShopProductsResponse>();
            GetItemsRequest itemsRequest;
            var items = new List<ShopProductsResponse>();
            var newItems = new List<ShopProductsResponse>();

            itemsRequest = new GetItemsRequest
            { 
                IncludeChildCategories = true,
                CategoryID = categoryID
            };
            items = GetItems(itemsRequest, false).OrderBy(c => c.SortOrder).ToList();
            return items;
        }

        [NonAction]

        public static IEnumerable<ShopProductsResponse> GetItems(GetItemsRequest request, bool includeItemDescriptions = true)
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


                            categoryIDs.AddRange(context.Query<int>(@"
                            WITH webcat (WebCategoryID, WebCategoryDescription, ParentID, NestedLevel) 
                                 AS (SELECT WebCategoryID, 
                                            WebCategoryDescription, 
                                            ParentID, 
                                            NestedLevel 
                                     FROM   WebCategories 
                                     WHERE  WebCategoryID = @masterCategoryID
                                            AND WebID = @webid
                                     UNION ALL 
                                     SELECT w.WebCategoryID, 
                                            w.WebCategoryDescription, 
                                            w.ParentID, 
                                            w.NestedLevel 
                                     FROM   WebCategories w 
                                            INNER JOIN webcat c 
                                                    ON c.WebCategoryID = w.ParentID) 
                            SELECT WebCategoryID
                            FROM   webcat
                        ", new
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
                    categoryItemCodes = context.Query<string>(@"
                        SELECT DISTINCT
	                        i.ItemCode
                            ,c.SortOrder
                        FROM 
                            WebCategoryItems c
	                        INNER JOIN Items i
		                        on c.ItemID = i.ItemID
	                        INNER JOIN WebCategories w
		                        on w.WebID = c.WebID
		                        and w.WebCategoryID = c.WebCategoryID
                        WHERE 
	                        c.WebID = @webid
	                        and c.WebCategoryID in @webcategoryids
                        ORDER By c.SortOrder
                    ", new
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
                    tempItemCodeList = context.Query<string>(@"                
                    ;WITH 
                        webcat 
                     (
                        WebCategoryID
                        ,WebCategoryDescription
                        ,ParentID
                        ,NestedLevel
                        ,SortOrder
                     ) 
				     AS 
                     (
                        SELECT 
                            WebCategoryID 
						    ,WebCategoryDescription
						    ,ParentID 
						    ,NestedLevel
                            ,SortOrder 
					    FROM   
                            WebCategories 
					    WHERE  
                            WebCategoryID = @masterCategoryID
						    AND WebID = @webid
					    
                        UNION ALL
                         
					    SELECT 
                            w.WebCategoryID 
						    ,w.WebCategoryDescription 
						    ,w.ParentID
						    ,w.NestedLevel
                            ,w.SortOrder
					    FROM   
                            WebCategories w 
					        INNER JOIN webcat c 
						        ON c.WebCategoryID = w.ParentID
                    ) 
                    SELECT 
                        i.ItemCode
                    FROM 
                        WebCategoryItems c
	                    INNER JOIN Items i
		                    ON c.ItemID = i.ItemID
                    WHERE 
                        c.WebCategoryID = (
                                            SELECT TOP 1 
                                                WebCategoryID 
					                        FROM 
                                                webcat 
                                            WHERE 
                                                ParentID = @masterCategoryID 
					                        ORDER BY 
                                                SortOrder
                                          )
                    ORDER BY
                        c.SortOrder
                    ", new
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
            var priceTypeID = request.PriceTypeID  ;

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
        private static List<ShopProductsResponse> GetItemInformation(GetItemsRequest request, int priceTypeID)
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
                apiItems = context.Query<ShopProductsResponse>(@"
                			    SELECT
	                                ItemID = i.ItemID,
	                                ItemCode = i.ItemCode,
	                                ItemDescription = 
		                                case 
			                                when i.IsGroupMaster = 1 then COALESCE(i.GroupDescription, il.ItemDescription, i.ItemDescription)
			                                when il.ItemDescription != '' then COALESCE(il.ItemDescription, i.ItemDescription)
							                else i.ItemDescription
		                                end,
	                                Weight = i.Weight,
	                                ItemTypeID = i.ItemTypeID,
	                                TinyImageUrl = i.TinyImageName,
	                                SmallImageUrl = i.SmallImageName,
	                                LargeImageUrl = i.LargeImageName,
	                                ShortDetail1 = COALESCE(il.ShortDetail, i.ShortDetail),
	                                ShortDetail2 = COALESCE(il.ShortDetail2, i.ShortDetail2),
	                                ShortDetail3 = COALESCE(il.ShortDetail3, i.ShortDetail3),
	                                ShortDetail4 = COALESCE(il.ShortDetail4, i.ShortDetail4),"
                          + (true
                            ? @"LongDetail1 = COALESCE(il.LongDetail, i.LongDetail),
	                                LongDetail2 = COALESCE(il.LongDetail2, i.LongDetail2),
	                                LongDetail3 = COALESCE(il.LongDetail3, i.LongDetail3),
	                                LongDetail4 = COALESCE(il.LongDetail4, i.LongDetail4),"
                            : string.Empty)
                          + @"IsVirtual = i.IsVirtual,
	                                AllowOnAutoOrder = i.AllowOnAutoOrder,
	                                IsGroupMaster = i.IsGroupMaster,
	                                IsDynamicKitMaster = cast(case when i.ItemTypeID = 2 then 1 else 0 end as bit),
	                                GroupMasterItemDescription = i.GroupDescription,
	                                GroupMembersDescription = i.GroupMembersDescription,
	                                Field1 = i.Field1,
	                                Field2 = i.Field2,
	                                Field3 = i.Field3,
	                                Field4 = i.Field4,
	                                Field5 = i.Field5,
	                                Field6 = i.Field6,
	                                Field7 = i.Field7,
	                                Field8 = i.Field8,
	                                Field9 = i.Field9,
	                                Field10 = i.Field10,
	                                OtherCheck1 = i.OtherCheck1,
	                                OtherCheck2 = i.OtherCheck2,
	                                OtherCheck3 = i.OtherCheck3,
	                                OtherCheck4 = i.OtherCheck4,
	                                OtherCheck5 = i.OtherCheck5,
	                                Price = ip.Price,
	                                CurrencyCode = ip.CurrencyCode,
	                                BV = ip.BusinessVolume,
	                                CV = ip.CommissionableVolume,
	                                OtherPrice1 = ip.Other1Price,
	                                OtherPrice2 = ip.Other2Price,
	                                OtherPrice3 = ip.Other3Price,
	                                OtherPrice4 = ip.Other4Price,
	                                OtherPrice5 = ip.Other5Price,
	                                OtherPrice6 = ip.Other6Price,
	                                OtherPrice7 = ip.Other7Price,
	                                OtherPrice8 = ip.Other8Price,
	                                OtherPrice9 = ip.Other9Price,
	                                OtherPrice10 = ip.Other10Price
                                FROM Items i
	                                INNER JOIN ItemPrices ip
		                                ON ip.ItemID = i.ItemID
		                                    AND ip.PriceTypeID = @priceTypeID
						                        AND ip.CurrencyCode = @currencyCode                                
	                                INNER JOIN ItemWarehouses iw
		                                ON iw.ItemID = i.ItemID
		                                    AND iw.WarehouseID = @warehouse
						            LEFT JOIN ItemLanguages il
		                                ON il.ItemID = i.ItemID
						                    AND il.LanguageID = @languageID
					            WHERE i.ItemCode in @itemCodes
                          ORDER BY " + sorting + @"
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

        public byte[] GetProductImage(string imageName)
        {
            try
            {
                object bytes;
                using (var context = Common.Utils.DbConnection.Sql())
                {
                    var query = @"SELECT TOP 1 
                                    ImageData 
                                  FROM 
                                    ItemImages 
                                  WHERE 
                                    ImageName = @Name";

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

        #region Private methods

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
