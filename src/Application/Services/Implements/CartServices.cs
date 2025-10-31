using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TiendaUCN.src.Application.DTO.CartDTO;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Application.Services.Implements
{
    public class CartServices : ICartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ConcurrentDictionary<string, Cart> _carts =
            new ConcurrentDictionary<string, Cart>();
        private int _nextCartId = 1;
        private int _nextCartItemId = 1;

        public CartServices(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetCurrentBuyerId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                return userId ?? "anonymous";
            }
            return "anonymous";
        }

        public async Task<CartDTO> GetCartAsync()
        {
            var buyerId = GetCurrentBuyerId();
            var cart = _carts.GetOrAdd(
                buyerId,
                _ => new Cart
                {
                    Id = Interlocked.Increment(ref _nextCartId),
                    BuyerId = buyerId,
                    CartItems = new List<CartItem>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                }
            );

            return MapToCartDTO(cart);
        }

        public async Task<CartDTO> AddItemAsync(AddCartItemDTO addCartItemDTO)
        {
            var buyerId = GetCurrentBuyerId();
            var cart = await GetCartAsync();

            // For simplicity, assume Product exists and has price/stock
            // In real implementation, inject IProductService or repository
            var product = new Product
            {
                Id = addCartItemDTO.ProductId,
                Title = $"Product {addCartItemDTO.ProductId}",
                Description = $"Description for product {addCartItemDTO.ProductId}",
                Price = 100m, // Mock price
                Stock = 10, // Mock stock
                Status = "Active",
                CategoryId = 1,
                BrandId = 1,
                Category = new Category { Id = 1, Name = "Mock Category" },
                Brand = new Brand { Id = 1, Name = "Mock Brand" },
            };

            var existingItem = cart.CartItems.FirstOrDefault(ci =>
                ci.ProductId == addCartItemDTO.ProductId
            );
            if (existingItem != null)
            {
                existingItem.Quantity += addCartItemDTO.Quantity;
            }
            else
            {
                var newItem = new CartItemDTO
                {
                    Id = Interlocked.Increment(ref _nextCartItemId),
                    ProductId = addCartItemDTO.ProductId,
                    Quantity = addCartItemDTO.Quantity,
                    Product = new ProductDTO
                    {
                        Id = product.Id,
                        Title = product.Title,
                        Description = product.Description,
                        Price = product.Price,
                        Stock = product.Stock,
                        ImageUrl = "", // Mock
                    },
                    CartId = cart.Id,
                };
                cart.CartItems.Add(newItem);
            }

            // Update totals (mock calculation)
            cart.SubTotal = cart.CartItems.Sum(ci => ci.Quantity * (int)ci.Product.Price);
            cart.Total = cart.SubTotal; // No discounts for now
            cart.UpdatedAt = DateTime.UtcNow;

            // Update in-memory cart
            _carts[buyerId] = MapFromCartDTO(cart);

            return cart;
        }

        public async Task RemoveItemAsync(int productId)
        {
            var buyerId = GetCurrentBuyerId();
            if (_carts.TryGetValue(buyerId, out var cart))
            {
                cart.CartItems = cart.CartItems.Where(ci => ci.ProductId != productId).ToList();
                cart.UpdatedAt = DateTime.UtcNow;
            }
        }

        public async Task<CartDTO> UpdateItemQuantityAsync(int productId, int quantity)
        {
            var buyerId = GetCurrentBuyerId();
            var cart = await GetCartAsync();

            var item = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (item != null)
            {
                item.Quantity = quantity;
                cart.SubTotal = cart.CartItems.Sum(ci => ci.Quantity * (int)ci.Product.Price);
                cart.Total = cart.SubTotal;
                cart.UpdatedAt = DateTime.UtcNow;

                _carts[buyerId] = MapFromCartDTO(cart);
            }

            return cart;
        }

        public async Task<CartDTO> CheckoutAsync()
        {
            var buyerId = GetCurrentBuyerId();
            var cart = await GetCartAsync();

            // Mock checkout: clear cart after "checkout"
            if (_carts.TryRemove(buyerId, out _))
            {
                // In real implementation, create order, process payment, etc.
            }

            return cart;
        }

        private CartDTO MapToCartDTO(Cart cart)
        {
            return new CartDTO
            {
                Id = cart.Id,
                Total = cart.Total,
                SubTotal = cart.SubTotal,
                BuyerId = cart.BuyerId,
                UserId = cart.UserId,
                CartItems = cart
                    .CartItems.Select(ci => new CartItemDTO
                    {
                        Id = ci.Id,
                        Quantity = ci.Quantity,
                        ProductId = ci.ProductId,
                        Product = new ProductDTO
                        {
                            Id = ci.Product.Id,
                            Title = ci.Product.Title,
                            Description = ci.Product.Description,
                            Price = ci.Product.Price,
                            Stock = ci.Product.Stock,
                            ImageUrl = "", // Mock
                        },
                        CartId = ci.CartId,
                    })
                    .ToList(),
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt,
            };
        }

        private Cart MapFromCartDTO(CartDTO cartDto)
        {
            return new Cart
            {
                Id = cartDto.Id,
                Total = cartDto.Total,
                SubTotal = cartDto.SubTotal,
                BuyerId = cartDto.BuyerId,
                UserId = cartDto.UserId,
                CartItems = cartDto
                    .CartItems.Select(ci => new CartItem
                    {
                        Id = ci.Id,
                        Quantity = ci.Quantity,
                        ProductId = ci.ProductId,
                        Product = new Product
                        {
                            Id = ci.Product.Id,
                            Title = ci.Product.Title,
                            Description = ci.Product.Description,
                            Price = ci.Product.Price,
                            Stock = ci.Product.Stock,
                            Status = "Active",
                            CategoryId = 1,
                            BrandId = 1,
                            Category = new Category { Id = 1, Name = "Mock Category" },
                            Brand = new Brand { Id = 1, Name = "Mock Brand" },
                        },
                        CartId = ci.CartId,
                    })
                    .ToList(),
                CreatedAt = cartDto.CreatedAt,
                UpdatedAt = cartDto.UpdatedAt,
            };
        }
    }
}
