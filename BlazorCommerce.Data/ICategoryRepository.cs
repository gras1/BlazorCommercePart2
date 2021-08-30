using System.Collections.Generic;
using BlazorCommerce.Shared;

namespace BlazorCommerce.Data
{
    public interface ICategoryRepository
    {
        CategoryDto Get(string friendlyUrl);
        IEnumerable<CategoryDto> GetAllSiblingCategories(string friendlyUrl);
        BestSellingCategoriesDto GetBestSellingCategoriesProducts();
        IEnumerable<CategoryDto> GetFeaturedCategories();
        IEnumerable<MenuCategoryDto> GetMenuCategories();
    }
}