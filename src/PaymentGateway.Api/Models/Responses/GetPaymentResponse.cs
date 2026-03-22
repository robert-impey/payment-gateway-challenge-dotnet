using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using PaymentGateway.Api.Enums;

namespace PaymentGateway.Api.Models.Responses;

public record GetPaymentResponse
{
    [Required]
    public required Guid Id { get; init; }

    // Note that data annotations for outgoing DTO fields
    // are not validated by the framework, but I have added
    // them here for clarity and to potentially help serializers.
    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
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