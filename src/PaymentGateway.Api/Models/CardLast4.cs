using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Api.Models;

public record CardLast4
{
    private readonly string _value;

    public string Value => _value;

    public CardLast4([StringLength(maximumLength: 4, MinimumLength = 4)] string value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "Value cannot be null.");
        }

        if (value.Length != 4 || value.Any(c => c is < '0' or > '9'))
        {
            throw new ArgumentException("Must be exactly 4 numeric digits.", nameof(value));
        }

        _value = value;
    }

    public static implicit operator CardLast4(string v)
    {
        throw new NotImplementedException();
    }
}
