using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

// For the code test, the Mountebank simulator is running in Docker when
// tests are run.
// In a real system, this class might have an interface or inherit from an
// abstract class. This would allow mocks to be used in tests.
// There might also be different implementations for different banks, or versions of
// the bank's API.
public class AcquiringBankClient
{
    public Task<PostPaymentResponse> MakePayment(PostPaymentRequest request, CancellationToken token)
    {
        var response = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Authorized,
            CardNumberLastFour = request.CardNumber[^4..],
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency,
            Amount = request.Amount
        };

        return Task.FromResult(response);
    }
}
