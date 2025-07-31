using AngularAPI.Dtos;
using AngularAPI.Services;
using AngularProject.Enums;
using AngularProject.Models;
using DotNetWebAPI.DTOs;
using DotNetWebAPI.Models;
using DotNetWebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotNetWebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CheckOutController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;

        public CheckOutController(IOrderRepository orderRepository, ICartRepository cartRepository)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
        }

        /// <summary>
        /// Checkout from shopping cart
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Order>> Post()
        {
            User? user = HttpContext.Items["User"] as User;

            if (user == null)
            {
                return Unauthorized();
            }

            var cartId = _cartRepository.GetCart(user.Id);
            var myCartItems = _cartRepository.GetProductsAvailableInCart(cartId); // List of cart Items
            List<OrderProducts> orderProducts = new();

            decimal total = 0;

            foreach (var item in myCartItems)
            {
                item.Product.Quantity -= item.Quantity;

                orderProducts.Add(new OrderProducts()
                {
                    ProductId = item.Product.Id,
                    Quantity = item.Quantity,
                });

                total += (item.Quantity * item.Product.price);
            }

            var myOrder = await _orderRepository.CreateOrderAsync(new Order()
            {
                UserID = user.Id,
                Date = DateTime.Now,
                State = OrderState.Pending,
                TotalPrice = total,
                OrderProducts = orderProducts
            });

            //_cartRepository.ClearCart(user.Id); // Uncomment if needed

            return CreatedAtAction(nameof(GetCurrentOrder), new { id = myOrder.Id }, myOrder);
        }

        /// <summary>
        /// Get current user's pending order
        /// </summary>
        /// <returns></returns>
        [HttpGet("test")]
        public IActionResult Test() => Ok("API is working");
        [HttpGet("current")]
        public async Task<ActionResult<Order>> GetCurrentOrder()
        {
            User? user = HttpContext.Items["User"] as User;

            if (user == null)
            {
                return Unauthorized();
            }

            var order = await _orderRepository.GetCurrentOrderForUser(user.Id);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }
    }
}