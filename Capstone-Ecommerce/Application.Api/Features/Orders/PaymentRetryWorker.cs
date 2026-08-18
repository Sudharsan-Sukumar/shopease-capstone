using Application.Api.Common.Data;
using Application.Api.Features.Orders.Payment;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Registry;

namespace Application.Api.Features.Orders;

/// <summary>
/// Closes the loop on the Fallback strategy in PaymentResilience: an order
/// that ended up PaymentPending (payment failed even after retry + circuit
/// breaker) isn't lost - this periodically re-attempts it through the SAME
/// resilience pipeline, same shape as Track A's A15 background services
/// (PeriodicTimer + a DI scope per tick, since this class itself is a
/// singleton and can't hold a scoped DbContext directly).
/// </summary>
public class PaymentRetryWorker(IServiceScopeFactory scopeFactory, ILogger<PaymentRetryWorker> logger) : BackgroundService
{
    private const int MaxAttempts = 5;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var gateway = scope.ServiceProvider.GetRequiredService<IPaymentGateway>();
            var pipelineProvider = scope.ServiceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();
            var pipeline = pipelineProvider.GetPipeline<PaymentChargeResult>(PaymentResilience.PipelineKey);

            var pending = await db.Orders
                .Where(o => o.Status == OrderStatus.PaymentPending && o.PaymentAttempts < MaxAttempts)
                .ToListAsync(stoppingToken);

            if (pending.Count == 0) continue;

            foreach (var order in pending)
            {
                try
                {
                    var result = await pipeline.ExecuteAsync(
                        async ct => await gateway.ChargeAsync(order.Total, ct), stoppingToken);

                    order.PaymentAttempts++;

                    if (result.Succeeded)
                    {
                        order.Status = OrderStatus.Confirmed;
                        order.PaymentTransactionId = result.TransactionId;
                        logger.LogInformation(
                            "Order {OrderId} payment succeeded on retry attempt {Attempt}",
                            order.Id, order.PaymentAttempts);
                    }
                    else
                    {
                        logger.LogWarning(
                            "Order {OrderId} still pending after retry attempt {Attempt}",
                            order.Id, order.PaymentAttempts);
                    }
                }
                catch (Exception ex)
                {
                    // One order's failure shouldn't stop the rest of the batch.
                    logger.LogError(ex, "Unexpected error retrying payment for order {OrderId}", order.Id);
                }
            }

            await db.SaveChangesAsync(stoppingToken);
        }
    }
}
