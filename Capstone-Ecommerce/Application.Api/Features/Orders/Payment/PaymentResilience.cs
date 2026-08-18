using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace Application.Api.Features.Orders.Payment;

public static class PaymentResilience
{
    public const string PipelineKey = "payment-gateway";

    /// <summary>
    /// One pipeline, three strategies, applied in order: Retry (transient
    /// blips get a couple of quick extra attempts) -> CircuitBreaker (stop
    /// hammering a gateway that's clearly down) -> Fallback (once retries
    /// AND the circuit breaker have both given up, return a "not charged"
    /// result instead of throwing - the caller decides what that means,
    /// here: mark the order Payment Pending instead of failing checkout).
    /// </summary>
    public static void Configure(ResiliencePipelineBuilder<PaymentChargeResult> pb)
    {
        pb.AddRetry(new RetryStrategyOptions<PaymentChargeResult>
        {
            ShouldHandle = new PredicateBuilder<PaymentChargeResult>().Handle<PaymentGatewayException>(),
            MaxRetryAttempts = 2,
            BackoffType = DelayBackoffType.Exponential,
            Delay = TimeSpan.FromMilliseconds(200),
        });

        pb.AddCircuitBreaker(new CircuitBreakerStrategyOptions<PaymentChargeResult>
        {
            ShouldHandle = new PredicateBuilder<PaymentChargeResult>().Handle<PaymentGatewayException>(),
            FailureRatio = 0.5,
            MinimumThroughput = 4,
            SamplingDuration = TimeSpan.FromSeconds(30),
            BreakDuration = TimeSpan.FromSeconds(10),
        });

        pb.AddFallback(new Polly.Fallback.FallbackStrategyOptions<PaymentChargeResult>
        {
            ShouldHandle = new PredicateBuilder<PaymentChargeResult>().Handle<Exception>(),
            FallbackAction = _ => Outcome.FromResultAsValueTask(new PaymentChargeResult(false, null)),
        });
    }
}
