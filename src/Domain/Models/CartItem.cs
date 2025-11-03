namespace TiendaUCN.src.Domain.Models
{
    public class CartItem
    {
        /// <summary>
        /// Unique identifier of the item in the shopping cart.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Quantity of the product in the shopping cart.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Id of the product associated with the cart item.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Product associated with the cart item.
        /// </summary>
        public Product Product { get; set; } = null!;

        /// <summary>
        /// Id of the shopping cart to which the item belongs.
        /// </summary>
        public int CartId { get; set; }

        /// <summary>
        /// Shopping cart to which the item belongs.
        /// </summary>
        public Cart Cart { get; set; } = null!;
    }
}
