using System.Collections.Generic;
using WinkNatural.Services.DTO.Shopping;
using WinkNaturals.Models;


namespace WinkNatural.Services.Interfaces
{
    public interface IShoppingService
    {
        //Get shop products list
        List<ShopProductsResponse> GetShopProducts(int categoryID = 0, int pageSize = 18, int pageIndex = 0, string[] sizes = null, int sortBy = 0);

        //Get item category
        List<ItemCategoryResponse> GetItemCategory(int webCategoryID);
        byte[] GetProductImage(string imageName);
    }
}
