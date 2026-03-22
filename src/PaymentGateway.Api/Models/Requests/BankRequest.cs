using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Models.Requests;

public record BankRequest
{
    private const string AllDigitsRegex = @"^\d+$";

    [JsonPropertyName("card_number")]
    [Required, MinLength(14), MaxLength(19), RegularExpression(AllDigitsRegex)]
    public required string CardNumber { get; init; }

    [JsonIgnore]
    [Required, Range(1, 12)]
    public int ExpiryMonth { get; init; }

    [JsonIgnore]
    [Required]
    public int ExpiryYear { get; init; }

    // This property generates the "04/2025" format for the JSON
    [JsonPropertyName("expiry_date")]
    public string ExpiryDate => $"{ExpiryMonth:D2}/{ExpiryYear}";

    [Required]
    public required string Currency { get; init; }

    [Required]
    public required int Amount { get; init; }

    [Required, MinLength(3), MaxLength(4), RegularExpression(AllDigitsRegex)]
    public required string Cvv { get; init; }
}