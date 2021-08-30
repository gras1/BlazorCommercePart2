using System.Collections.Generic;
using BlazorCommerce.Shared;

namespace BlazorCommerce.Data
{
    public interface ICategoryMinRepository
    {
        CategoryMinDto Get(string friendlyUrl);
        IEnumerable<CategoryMinDto> GetAllSiblingCategories(string friendlyUrl);
        IEnumerable<CategoryMinDto> GetFeaturedCategories();
    }
}