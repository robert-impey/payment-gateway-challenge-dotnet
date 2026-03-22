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
public class AcquiringBankClient(HttpClient httpClient)
{
    public async Task<PostPaymentResponse> MakePayment(PostPaymentRequest request, CancellationToken token)
    {
        var bankRequest = new BankRequest
        {
            CardNumber = request.CardNumber,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency,
            Amount = request.Amount,
            Cvv = request.Cvv
        };
        
        try
        {
            // In a production system, the bank's location and
            // security credentials would be stored in configuration
            // and secret stores.
            var response = await httpClient.PostAsJsonAsync(
                "http://localhost:8080/payments",
                bankRequest,
                token);

            response.EnsureSuccessStatusCode();

            var bankResponse = await response.Content.ReadFromJsonAsync<BankResponse>(cancellationToken: token);

            if (bankResponse is null)
            {
                return MakeRejectedResponse(request);
            }

            if (string.IsNullOrWhiteSpace(bankResponse.AuthorizationCode))
            {
                if (bankResponse.Authorized)
                {
                    // If the bank said that the payment was authorized,
                    // but the authorization code is missing, then we will 
                    // count this as a rejection.
                    return MakeRejectedResponse(request);
                }

                return new PostPaymentResponse
                {
                    Id = Guid.Empty,
                    Status = PaymentStatus.Declined,
                    CardNumberLastFour = request.CardNumber[^4..],
                    ExpiryMonth = request.ExpiryMonth,
                    ExpiryYear = request.ExpiryYear,
                    Currency = request.Currency,
                    Amount = request.Amount
                };
            }

            if (Guid.TryParse(bankResponse.AuthorizationCode, out var authCode))
            {
                if (bankResponse.Authorized)
                {
                    return new PostPaymentResponse
                    {
                        Id = authCode,
                        Status = PaymentStatus.Authorized,
                        CardNumberLastFour = request.CardNumber[^4..],
                        ExpiryMonth = request.ExpiryMonth,
                        ExpiryYear = request.ExpiryYear,
                        Currency = request.Currency,
                        Amount = request.Amount
                    };
                }
            }

            return MakeRejectedResponse(request);
        }
        catch (Exception)
        {
            // In a production system, we would
            //  - Log the exception
            //  - Return an HTTP error code
            return MakeRejectedResponse(request);
        }
    }

    private static PostPaymentResponse MakeRejectedResponse(PostPaymentRequest request)
    {
        return new PostPaymentResponse
        {
            Id = Guid.Empty,
            Status = PaymentStatus.Rejected,
            CardNumberLastFour = request.CardNumber[^4..],
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency,
            Amount = request.Amount
        };
    }
}
