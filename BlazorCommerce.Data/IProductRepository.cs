using BlazorCommerce.Shared;

namespace BlazorCommerce.Data
{
    public interface IProductRepository
    {
        ProductDto Get(string friendlyUrl);
        void IncrementNumberOfTimesViewed(string friendlyUrl);
    }
}