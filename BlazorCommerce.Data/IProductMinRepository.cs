using System.Collections.Generic;
using BlazorCommerce.Shared;

namespace BlazorCommerce.Data
{
    public interface IProductMinRepository
    {
        IEnumerable<CategoryProductDto> GetProductsByLeafCategoryId(int leafCategoryId);
        TrendingProductsDto GetTrendingProducts();
    }
}