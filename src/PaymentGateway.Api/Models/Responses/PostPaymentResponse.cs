using System.ComponentModel.DataAnnotations;

using PaymentGateway.Api.Enums;

namespace PaymentGateway.Api.Models.Responses;

public record PostPaymentResponse
{
    public Guid? Id { get; init; }

    [Required]
    public required PaymentStatus Status { get; init; }

    public CardLast4? CardNumberLastFour { get; init; }

    public int? ExpiryMonth { get; init; }

    public int? ExpiryYear { get; init; }

    public string? Currency { get; init; }

    public int? Amount { get; init; }
}
