using System.ComponentModel.DataAnnotations;

using PaymentGateway.Api.Enums;

namespace PaymentGateway.Api.Models.Responses;

public record GetPaymentResponse
{
    public Guid? Id { get; init; }

    // Note that data annotations for outgoing DTO fields
    // are not validated by the framework, but I have added
    // them here for clarity and to potentially help serializers.
    [Required]
    public required PaymentStatus Status { get; init; }

    public CardLast4? CardNumberLastFour { get; init; }

    public int? ExpiryMonth { get; init; }

    public int? ExpiryYear { get; init; }

    public string? Currency { get; init; }

    public int? Amount { get; init; }
}