using System.Collections.Generic;

namespace BlazorCommerce.Shared
{
    public class CartMinDto : IDataModel
    {
        public int Id {get;set;}

        public string Guid {get;set;}

        public List<CartItemMinDto> CartItems {get;set;}

        public decimal TotalPrice {get;set;}

        public decimal TotalSalesTax {get;set;}
    }
}