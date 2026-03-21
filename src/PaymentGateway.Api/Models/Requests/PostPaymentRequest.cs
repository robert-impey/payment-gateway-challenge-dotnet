namespace PaymentGateway.Api.Models.Requests;

public record PostPaymentRequest
{
    // I've changed this as the requirements said "Card Number",
    // not just the last four digits in the request.
    //public int CardNumberLastFour { get; set; }
    public required IReadOnlyList<int> CardNumber { get; init; }

    public required int ExpiryMonth { get; init; }
    public required int ExpiryYear { get; init; }
    public required string Currency { get; init; }
    public required int Amount { get; init; }
    public required int Cvv { get; init; }
}