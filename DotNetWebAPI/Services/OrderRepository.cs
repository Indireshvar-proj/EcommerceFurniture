using AngularProject.Data;
using AngularProject.Enums;
using AngularProject.Models;
using DotNetWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotNetWebAPI.Services
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ShoppingDbContext _context;

        public OrderRepository(ShoppingDbContext context)
        {
            _context = context;
        }

        public async Task<ActionResult<IEnumerable<Order>>> ReturnAllOrders(string userId, OrderSearchModel orderSearchModel)
        {
            IQueryable<Order> query = _context.Orders
                .Where(w => w.UserID == userId)
                .Include(o => o.OrderProducts)
                .ThenInclude(P => P.Product);

            if (orderSearchModel != null)
            {
                if (!string.IsNullOrEmpty(orderSearchModel.OrdeState))
                {
                    switch (orderSearchModel.OrdeState)
                    {
                        case "Pending":
                            query = query.Where(x => x.State == OrderState.Pending);
                            break;
                        case "Accepted":
                            query = query.Where(x => x.State == OrderState.Accepted);
                            break;
                        case "Rejected":
                            query = query.Where(x => x.State == OrderState.Rejected);
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(orderSearchModel.orderDate))
                {
                    switch (orderSearchModel.orderDate)
                    {
                        case "Newest":
                            query = query.OrderByDescending(x => x.Date);
                            break;
                        case "Oldest":
                            query = query.OrderBy(x => x.Date);
                            break;
                    }
                }
            }
            return await query.ToListAsync();
        }

        public async Task<ActionResult<IEnumerable<Order>>> ReturnAllAdminOrders(string userId, OrderSearchModel orderSearchModel)
        {
            IQueryable<Order> query = _context.Orders
                .Where(x => x.OrderProducts.Any(y => y.Product.OwnerId == userId))
                .Include(o => o.OrderProducts)
                .ThenInclude(P => P.Product);

            if (orderSearchModel != null)
            {
                if (!string.IsNullOrEmpty(orderSearchModel.OrdeState))
                {
                    switch (orderSearchModel.OrdeState)
                    {
                        case "Pending":
                            query = query.Where(x => x.State == OrderState.Pending);
                            break;
                        case "Accepted":
                            query = query.Where(x => x.State == OrderState.Accepted);
                            break;
                        case "Rejected":
                            query = query.Where(x => x.State == OrderState.Rejected);
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(orderSearchModel.orderDate))
                {
                    switch (orderSearchModel.orderDate)
                    {
                        case "Newest":
                            query = query.OrderByDescending(x => x.Date);
                            break;
                        case "Oldest":
                            query = query.OrderBy(x => x.Date);
                            break;
                    }
                }
            }
            return await query.ToListAsync();
        }

        public async Task<ActionResult<Order>> ReturnOrderById(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderProducts)
                .ThenInclude(P => P.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            return order!;
        }

        public async Task<Order> PutOrder(int orderId, Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return await _context.Orders
                .Include(o => o.OrderProducts)
                .ThenInclude(P => P.Product)
                .FirstOrDefaultAsync(o => o.Id == order.Id);
        }

        public async Task DeleteOrderAsync(int OrderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == OrderId);
            _context.Remove(order!);
            await _context.SaveChangesAsync();
        }

        public bool IsOrderExixtsAsync(int id)
        {
            return _context.Orders.Any(p => p.Id == id);
        }

        public async Task<IReadOnlyList<Order>> ReturnPendingOrders()
        {
            return await _context.Orders
                .Where(x => x.State == OrderState.Pending)
                .ToListAsync();
        }

        public async Task AcceptOrder(int orderId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            order!.State = OrderState.Accepted;
            await _context.SaveChangesAsync();
        }

        public async Task PendingOrder(int orderId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            order!.State = OrderState.Pending;
            await _context.SaveChangesAsync();
        }

        public async Task RejectOrder(int orderId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            order!.State = OrderState.Rejected;
            await _context.SaveChangesAsync();
        }

        /// ✅ Added: Get current pending order for user
        public async Task<Order?> GetCurrentOrderForUser(string userId)
        {
            return await _context.Orders
                .Include(o => o.OrderProducts)
                .ThenInclude(p => p.Product)
                .FirstOrDefaultAsync(o => o.UserID == userId && o.State == OrderState.Pending);
        }
    }
}