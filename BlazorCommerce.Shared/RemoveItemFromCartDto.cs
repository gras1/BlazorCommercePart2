namespace BlazorCommerce.Shared
{
    public class RemoveItemFromCartDto
    {
        public string CartGuid {get;set;}

        public int ProductOptionProductInstanceId {get;set;}
    }
}