using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TiendaUCN.src.Application.DTO.AuthDTO;
using TiendaUCN.src.Application.DTO.BaseResponse;
using TiendaUCN.src.Application.DTO.CartDTO;
using TiendaUCN.src.Application.Services.Interfaces;

[Authorize] // R37: Se aplica a TODO el controlador
[ApiController]
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        // El servicio ya sabe quién es el usuario
        var cart = await _cartService.GetCartAsync();
        return Ok(new GenericResponse<CartDTO>("Carrito obtenido", cart));
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromForm] AddCartItemDTO addCartItemDTO)
    {
        // El servicio ya sabe quién es el usuario
        var result = await _cartService.AddItemAsync(addCartItemDTO);

        // R38/R44: Deberías devolver 201 Created aquí, no 200 OK
        return CreatedAtAction(
            nameof(GetCart),
            new { id = result.Id },
            new GenericResponse<CartDTO>("Item agregado", result)
        );
    }

    // R41: El requisito pide DELETE /api/cart/items/{itemId}
    [HttpDelete("items/{productId}")]
    public async Task<IActionResult> RemoveItem(int productId)
    {
        await _cartService.RemoveItemAsync(productId);

        // R41: Devolver 204 No Content es lo estándar
        return NoContent();
    }

    // R40: El requisito pide PUT /api/cart/items/{itemId}
    // Tu endpoint es [HttpPatch("items")], lo cual es una desviación.
    [HttpPut("items/{productId}")] // <-- Ajustado a R40
    public async Task<IActionResult> UpdateItemQuantity(
        int productId,
        [FromForm] UpdateQuantityDTO dto
    )
    {
        var result = await _cartService.UpdateItemQuantityAsync(productId, dto.Quantity);

        // R40: Devolver 200 OK o 204 No Content
        return Ok(new GenericResponse<CartDTO>("Cantidad actualizada", result));
    }

    // ... otros endpoints ...

    [HttpPost("checkout")]
    [Authorize(Roles = "Customer")] // Esto estaba bien
    public async Task<IActionResult> CheckoutAsync()
    {
        var result = await _cartService.CheckoutAsync();
        return Ok(new GenericResponse<CartDTO>("Checkout realizado", result));
    }
}
