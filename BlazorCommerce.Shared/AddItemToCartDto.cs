namespace BlazorCommerce.Shared
{
    public class AddItemToCartDto
    {
        public int ProductOptionProductInstanceId {get;set;}

        public string CartGuid {get;set;}

        public int Quantity {get;set;}
    }
}