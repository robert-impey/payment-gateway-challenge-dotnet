using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public class PaymentsRepository
{
    public List<PostPaymentResponse> Payments = new();
    
    public void Add(PostPaymentResponse payment, CancellationToken token)
    {
        Payments.Add(payment);
    }

    // I made this nullable as there may be no payment with that id.
    public Task<PostPaymentResponse?> Get(Guid id, CancellationToken token)
    {
        // With a real system, this code would most likely have
        // an "await" in it and be properly async, so I would not need the Task.FromResult.
        return Task.FromResult(Payments.FirstOrDefault(p => p.Id == id));
    }
}