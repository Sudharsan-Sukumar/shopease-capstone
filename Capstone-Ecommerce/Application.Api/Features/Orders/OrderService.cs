using Application.Api.Common.Repository;
using Application.Api.Common.Results;
using Application.Api.Features.Cart;
using Application.Api.Features.Orders.Payment;
using Application.Api.Features.Products;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Registry;

namespace Application.Api.Features.Orders;

public class OrderService(IUnitOfWork uow, IPaymentGateway paymentGateway, ResiliencePipelineProvider<string> pipelineProvider) : IOrderService
{
    public async Task<Result<OrderDto>> CheckoutAsync(int userId, CancellationToken ct = default)
    {
        var cart = await uow.Repository<Cart.Cart>().Query()
            .Include(c => c.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);

        if (cart is null || cart.Items.Count == 0)
            return Error.Validation("Order.EmptyCart", "Your cart is empty.");

        // Everything below runs as one retriable unit - the execution
        // strategy (SQL Server's EnableRetryOnFailure) owns the transaction
        // boundaries, since a raw BeginTransactionAsync can't safely be
        // retried on a transient failure. Commits only if the Result
        // returned here is successful; rolls back otherwise (including for
        // expected failures like insufficient stock or a concurrency
        // conflict, not just exceptions).
        return await uow.ExecuteInTransactionAsync<OrderDto>(async innerCt =>
        {
            foreach (var item in cart.Items)
            {
                var product = await uow.Repository<Product>().GetByIdAsync(item.ProductId, innerCt);
                if (product is null || product.Stock < item.Quantity)
                    return Error.Conflict("Order.InsufficientStock", $"Not enough stock for \"{item.Product.Name}\".");

                // Mutating Stock here means Product's [Timestamp] RowVersion
                // is checked against the DB's current value at
                // SaveChangesAsync below - if another checkout already
                // changed this same row (e.g. bought the last unit a moment
                // ago), that check fails.
                product.Stock -= item.Quantity;
            }

            var order = new Order
            {
                UserId = userId,
                Status = OrderStatus.Confirmed, // corrected below once payment is attempted
                Total = cart.Items.Sum(i => i.Quantity * i.Product.Price),
            };
            foreach (var item in cart.Items)
            {
                order.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    UnitPrice = item.Product.Price,
                    Quantity = item.Quantity,
                });
            }
            await uow.Repository<Order>().AddAsync(order, innerCt);

            try
            {
                await uow.SaveChangesAsync(innerCt);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Error.Conflict(
                    "Order.ConcurrencyConflict",
                    "Stock changed while you were checking out - please review your cart and try again.");
            }

            // Payment: retry -> circuit breaker -> fallback, all in one
            // pipeline (see PaymentResilience.Configure). ExecuteAsync never
            // throws for the simulated gateway's transient failures - the
            // fallback strategy catches those and returns Succeeded=false.
            var pipeline = pipelineProvider.GetPipeline<PaymentChargeResult>(PaymentResilience.PipelineKey);
            var paymentResult = await pipeline.ExecuteAsync(
                async pCt => await paymentGateway.ChargeAsync(order.Total, pCt), innerCt);

            order.Status = paymentResult.Succeeded ? OrderStatus.Confirmed : OrderStatus.PaymentPending;
            order.PaymentTransactionId = paymentResult.TransactionId;
            order.PaymentAttempts = 1;

            foreach (var item in cart.Items.ToList())
                uow.Repository<CartItem>().Remove(item);

            await uow.SaveChangesAsync(innerCt);

            return Result.Success(ToDto(order));
        }, ct);
    }

    public async Task<Result<List<OrderDto>>> GetMyOrdersAsync(int userId, CancellationToken ct = default)
    {
        var orders = await uow.Repository<Order>().Query()
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(ct);

        return Result.Success(orders.Select(ToDto).ToList());
    }

    public async Task<Result<OrderDto>> GetMyOrderByIdAsync(int userId, int orderId, CancellationToken ct = default)
    {
        var order = await uow.Repository<Order>().Query()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, ct);

        if (order is null)
            return Error.NotFound("Order.NotFound", $"Order {orderId} was not found.");

        return Result.Success(ToDto(order));
    }

    public async Task<Result<List<OrderDto>>> GetAllOrdersAsync(CancellationToken ct = default)
    {
        var orders = await uow.Repository<Order>().Query()
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(ct);

        return Result.Success(orders.Select(ToDto).ToList());
    }

    private static OrderDto ToDto(Order o) => new(
        o.Id,
        o.Status.ToString(),
        o.Total,
        o.CreatedAtUtc,
        o.PaymentTransactionId,
        o.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)).ToList());
}
