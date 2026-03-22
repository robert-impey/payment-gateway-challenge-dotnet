using System.ComponentModel.DataAnnotations;

using PaymentGateway.Api.Enums;

namespace PaymentGateway.Api.Models.Responses;

public record PostPaymentResponse
{
    [Required]
    public required Guid Id { get; init; }

    [Required]
    public required PaymentStatus Status { get; init; }

    [Required]
    public required CardLast4 CardNumberLastFour { get; init; }

    [Required]
    public required int ExpiryMonth { get; init; }

    [Required]
    public required int ExpiryYear { get; init; }

    [Required]
    public required string Currency { get; init; }

    [Required]
    public required int Amount { get; init; }
}
