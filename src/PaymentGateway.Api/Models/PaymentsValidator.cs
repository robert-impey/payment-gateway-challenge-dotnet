using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Models;

/// <summary>
/// Where fields can be validated individually, I have used attributes.
/// However, the expiry month and year need to be validated together,
/// so I have created this class to handle that logic.
/// </summary>
public class PaymentsValidator
{
    protected virtual DateTimeOffset Now => DateTimeOffset.UtcNow;

    public bool Validate(PostPaymentRequest request)
    {
        if (request.ExpiryYear < Now.Year)
        {
            return false;
        }

        if (request.ExpiryYear == Now.Year && request.ExpiryMonth < Now.Month)
        {
            return false;
        }

        return true;
    }
}
