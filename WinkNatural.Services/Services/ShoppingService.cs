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
        public List<ShopProductsResponse> GetShopProducts(int categoryID, int pageSize, int pageIndex, string[] sizes = null, int sortBy = 0)
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

                    //ids = productIds,
                    //languageID = (int)0
                }).ToList();
                //ShopProductsResponse shopProducts = new ShopProductsResponse();
                //shopProducts = response[0];
                return response[0];
            }
        }

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
