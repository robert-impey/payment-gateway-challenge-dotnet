using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Models.Responses;

public record BankResponse
{
    [JsonPropertyName("authorized")]
    public bool Authorized { get; init; } = false;

    [JsonPropertyName("authorization_code")]
    public string? AuthorizationCode { get; init; } = string.Empty;
}