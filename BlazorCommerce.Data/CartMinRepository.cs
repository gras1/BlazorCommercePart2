using System;
using System.Collections.Generic;
using System.Linq;
using BlazorCommerce.Shared;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace BlazorCommerce.Data
{
    public class CartMinRepository : BaseRepository, ICartMinRepository
    {
        public CartMinRepository(IOptions<BlazorCommerce.Data.DatabaseOptions> databaseOptions) : base(databaseOptions) { }

        public CartMinDto Get(string friendlyUrl)
        {
            var cart = new CartMinDto();

            using (var con = new SqliteConnection(base.ConnectionString))
            {
                con.Open();

                int? cartId = null;

                string stm = $"SELECT Id FROM Carts WHERE Guid = '{friendlyUrl}' LIMIT 1;";

                using (var cmd = new SqliteCommand(stm, con))
                {
                    object cartIdObj = cmd.ExecuteScalar();
                    if (cartIdObj != null)
                    {
                        if (int.TryParse(cartIdObj.ToString(), out int result))
                        {
                            cartId = result;
                        }
                    }
                }

                if (cartId == null || cartId.Value == 0)
                {
                    stm = $"INSERT INTO Carts (Guid, CreatedDateTime) VALUES ('{friendlyUrl}', strftime ('%s', 'now'));";

                    using (var cmd = new SqliteCommand(stm, con))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    stm = $"SELECT Id FROM Carts WHERE Guid = '{friendlyUrl}' LIMIT 1;";

                    using (var cmd = new SqliteCommand(stm, con))
                    {
                        object cartIdObj = cmd.ExecuteScalar();
                        if (cartIdObj != null)
                        {
                            if (int.TryParse(cartIdObj.ToString(), out int result))
                            {
                                cartId = result;
                            }
                        }
                    }
                }

                cart.Id = cartId.Value;
                cart.Guid = friendlyUrl;

                var cartItems = new List<CartItemMinDto>();

                stm = $"SELECT ProductOptionProductInstanceId, Quantity FROM CartItems WHERE CartId = {cartId.Value};";
                using (var cmd = new SqliteCommand(stm, con))
                {
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            var cartItem = new CartItemMinDto();
                            cartItem.ProductOptionProductInstanceId = rdr.GetInt32(rdr.GetOrdinal("ProductOptionProductInstanceId"));
                            cartItem.Quantity = rdr.GetInt32(rdr.GetOrdinal("Quantity"));
                            cartItems.Add(cartItem);
                        }
                    }
                }

                if (cartItems.Any())
                {
                    var salesTaxes = new List<SalesTax>();
                    stm = "SELECT SalesTaxTypeId, Amount FROM SalesTaxes WHERE EffectiveFromDate <= strftime ('%s', 'now') AND (EffectiveToDate IS NULL OR EffectiveToDate > strftime ('%s', 'now'));";
                    using (var cmd = new SqliteCommand(stm, con))
                    {
                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                var salesTax = new SalesTax
                                {
                                    SalesTaxTypeId = rdr.GetInt32(rdr.GetOrdinal("SalesTaxTypeId")),
                                    Amount = rdr.GetDecimal(rdr.GetOrdinal("Amount"))
                                };
                                salesTaxes.Add(salesTax);
                            }
                        }
                    }

                    var priceTotal = 0.0m;
                    var totalSalesTax = 0.0m;

                    foreach (var cartItem in cartItems)
                    {
                        stm = @"SELECT popi.Price, P.Title, p.CartImageUrl, p.CartThumbnailImageUrl, po.SalesTaxTypeId, p.FriendlyUrl, po.Option, pot.Name, pop.Stock
                                FROM ProductOptionProductInstances popi
                                    INNER JOIN ProductOptionProducts pop ON popi.ProductOptionProductId = pop.Id
                                    INNER JOIN Products p ON pop.ProductId = p.Id
                                    INNER JOIN ProductOptions po ON pop.ProductOptionId = po.Id
                                    INNER JOIN ProductOptionTypes pot ON po.ProductOptionTypeId = pot.Id
                                WHERE popi.Id = " + cartItem.ProductOptionProductInstanceId + ";";
                        using (var cmd = new SqliteCommand(stm, con))
                        {
                            using (var rdr = cmd.ExecuteReader())
                            {
                                while (rdr.Read())
                                {
                                    cartItem.FriendlyUrl = rdr.GetString(rdr.GetOrdinal("FriendlyUrl"));
                                    cartItem.CartThumbnailImageUrl = rdr.GetString(rdr.GetOrdinal("CartThumbnailImageUrl"));
                                    cartItem.CartImageUrl = rdr.GetString(rdr.GetOrdinal("CartImageUrl"));
                                    cartItem.ProductName = rdr.GetString(rdr.GetOrdinal("Title"));
                                    cartItem.Price = rdr.GetDecimal(rdr.GetOrdinal("Price"));
                                    cartItem.ProductOptionDescription = rdr.GetString(rdr.GetOrdinal("Name")) + " " + rdr.GetString(rdr.GetOrdinal("Option"));
                                    cartItem.Stock = rdr.GetInt32(rdr.GetOrdinal("Stock"));
                                    var amount = cartItem.Price;
                                    var salesTaxTypeId = rdr.GetInt32(rdr.GetOrdinal("SalesTaxTypeId"));
                                    var salesTaxAmount = salesTaxes.First(st => st.SalesTaxTypeId == salesTaxTypeId).Amount;
                                    cartItem.TotalAmount = Math.Round(amount + (amount * 0.01m * salesTaxAmount), 2, MidpointRounding.AwayFromZero) * cartItem.Quantity;
                                    priceTotal += (cartItem.Price * cartItem.Quantity);
                                    totalSalesTax += (cartItem.TotalAmount - (cartItem.Price * cartItem.Quantity));
                                }
                            }
                        }
                    }
                    cart.TotalPrice = priceTotal;
                    cart.TotalSalesTax = totalSalesTax;
                }

                cart.CartItems = cartItems;

                con.Close();
            }

            return cart;
        }

        public void AddItemToCart(string cartGuid, int productOptionProductInstanceId, int quantity)
        {
            using (var con = new SqliteConnection(base.ConnectionString))
            {
                con.Open();

                int cartId = 0;

                string stm = $"SELECT Id FROM Carts WHERE Guid = '{cartGuid}' LIMIT 1;";

                using (var cmd = new SqliteCommand(stm, con))
                {
                    object cartIdObj = cmd.ExecuteScalar();
                    if (cartIdObj != null)
                    {
                        if (int.TryParse(cartIdObj.ToString(), out int result))
                        {
                            cartId = result;
                        }
                    }
                }

                if (cartId != 0)
                {
                    stm = $"SELECT Quantity FROM CartItems WHERE cartId = {cartId} AND ProductOptionProductInstanceId = {productOptionProductInstanceId};";
                    int existingQuantity = 0;

                    using (var cmd = new SqliteCommand(stm, con))
                    {
                        object quantityObj = cmd.ExecuteScalar();
                        if (quantityObj != null)
                        {
                            if (int.TryParse(quantityObj.ToString(), out int result))
                            {
                                existingQuantity = result;
                            }
                        }
                    }

                    if (existingQuantity == 0)
                    {
                        stm = $"INSERT INTO CartItems (CartId, ProductOptionProductInstanceId, Quantity) VALUES ({cartId}, {productOptionProductInstanceId}, {quantity});";

                        using (var cmd = new SqliteCommand(stm, con))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        stm = $"UPDATE CartItems SET Quantity = {quantity + existingQuantity} WHERE CartId = {cartId} AND ProductOptionProductInstanceId = {productOptionProductInstanceId};";

                        using (var cmd = new SqliteCommand(stm, con))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                con.Close();
            }
        }

        public void RemoveItemFromCart(string cartGuid, int productOptionProductInstanceId)
        {
            using (var con = new SqliteConnection(base.ConnectionString))
            {
                con.Open();

                int cartId = 0;

                string stm = $"SELECT Id FROM Carts WHERE Guid = '{cartGuid}' LIMIT 1;";

                using (var cmd = new SqliteCommand(stm, con))
                {
                    object cartIdObj = cmd.ExecuteScalar();
                    if (cartIdObj != null)
                    {
                        if (int.TryParse(cartIdObj.ToString(), out int result))
                        {
                            cartId = result;
                        }
                    }
                }

                if (cartId != 0)
                {
                    stm = $"DELETE FROM CartItems WHERE cartId = {cartId} AND ProductOptionProductInstanceId = {productOptionProductInstanceId};";

                    using (var cmd = new SqliteCommand(stm, con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                con.Close();
            }
        }
    }
}