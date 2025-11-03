namespace TiendaUCN.src.Domain.Models
{
    public class Cart
    {
        /// <summary>
        /// Unique identifier of the shopping cart.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Total of the shopping cart including discounts.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Subtotal of the shopping cart without discounts.
        /// </summary>
        public int SubTotal { get; set; }

        /// <summary>
        /// User who has the cart (without authentication).
        /// </summary>
        public string BuyerId { get; set; } = null!;

        /// <summary>
        /// Identifier of the user who owns the shopping cart (authenticated).
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// List of items in the shopping cart.
        /// </summary>
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        /// <summary>
        /// Creation date of the shopping cart.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Update date of the shopping cart.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
