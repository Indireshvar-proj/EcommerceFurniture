using AngularProject.Models;
using DotNetWebAPI.Models;
using DotNetWebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity.Infrastructure;

namespace DotNetWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepo;

        public OrdersController(IOrderRepository orderRepo)
        {
            _orderRepo = orderRepo;
        }

        // ✅ GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders([FromQuery] OrderSearchModel orderSearchModel)
        {
            User? user = HttpContext.Items["User"] as User;
            return await _orderRepo.ReturnAllOrders(user.Id, orderSearchModel);
        }

        // ✅ GET: api/Orders/AdminOrders
        [HttpGet("AdminOrders")]
        public async Task<ActionResult<IEnumerable<Order>>> ReturnAdminOrders([FromQuery] OrderSearchModel orderSearchModel)
        {
            User? user = HttpContext.Items["User"] as User;
            return await _orderRepo.ReturnAllAdminOrders(user.Id, orderSearchModel);
        }

        // ✅ GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _orderRepo.ReturnOrderById(id);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        // ✅ PUT: api/Orders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, Order order)
        {
            if (id != order.Id)
            {
                return BadRequest();
            }

            try
            {
                await _orderRepo.PutOrder(id, order);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // ✅ POST: api/Orders
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            await _orderRepo.CreateOrderAsync(order);
            return CreatedAtAction("GetOrder", new { id = order.Id }, order);
        }

        // ✅ DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            await _orderRepo.DeleteOrderAsync(id);
            return NoContent();
        }

        // ✅ GET: api/Orders/PendingOrders
        [HttpGet("PendingOrders")]
        public async Task<IReadOnlyList<Order>> GetPendingOrders()
        {
            return await _orderRepo.ReturnPendingOrders();
        }

        // ✅ PUT: api/Orders/acceptOrder/5
        [HttpPut("acceptOrder/{id}")]
        public async Task<IActionResult> AcceptOrder(int id)
        {
            await _orderRepo.AcceptOrder(id);
            return Ok();
        }

        // ✅ PUT: api/Orders/pendingOrder/5
        [HttpPut("pendingOrder/{id}")]
        public async Task<IActionResult> PendingOrder(int id)
        {
            await _orderRepo.PendingOrder(id);
            return Ok();
        }

        // ✅ PUT: api/Orders/rejectOrder/5
        [HttpPut("rejectOrder/{id}")]
        public async Task<IActionResult> RejectOrder(int id)
        {
            await _orderRepo.RejectOrder(id);
            return Ok();
        }

        // ✅ NEW: GET: api/Orders/current
        [HttpGet("current")]
        public async Task<ActionResult<Order>> GetCurrentOrder()
        {
            User? user = HttpContext.Items["User"] as User;

            if (user == null)
            {
                return Unauthorized();
            }

            var order = await _orderRepo.GetCurrentOrderForUser(user.Id);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        private bool OrderExists(int id)
        {
            return _orderRepo.IsOrderExixtsAsync(id);
        }
    }
}