using BlazorCommerce.Shared;

namespace BlazorCommerce.Data
{
    public interface ICartMinRepository
    {
        void AddItemToCart(string cartGuid, int productOptionProductInstanceId, int quantity);
        CartMinDto Get(string friendlyUrl);
        void RemoveItemFromCart(string cartGuid, int productOptionProductInstanceId);
    }
}