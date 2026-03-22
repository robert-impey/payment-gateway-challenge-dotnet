using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Api.Models.Requests;

public record PostPaymentRequest
{
    private const string AllDigitsRegex = @"^\d+$";

    // I've changed this as the requirements said "Card Number",
    // not just the last four digits in the request.
    // Also, an int would trim leading zeros.
    [Required]
    [MinLength(14), MaxLength(19)]
    [RegularExpression(AllDigitsRegex)]
    public required string CardNumber { get; init; }

    [Required]
    public required int ExpiryMonth { get; init; }

    [Required]
    public required int ExpiryYear { get; init; }

    [Required]
    [Currency]
    public required string Currency { get; init; }

    [Required]
    public required int Amount { get; init; }

    [Required]
    [MinLength(3), MaxLength(4)]
    [RegularExpression(AllDigitsRegex)]
    public required string Cvv { get; init; }
}