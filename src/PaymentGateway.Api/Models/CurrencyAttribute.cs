using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Api.Models;

public class CurrencyAttribute : ValidationAttribute
{
    private static readonly ISet<string> Currencies = new HashSet<string>
    {
        "GBP",
        "USD",
        "EUR"
    };

    public override bool IsValid(object? value)
    {
        if (value is not string currency) return false;

        return Currencies.Contains(currency);
    }
}
