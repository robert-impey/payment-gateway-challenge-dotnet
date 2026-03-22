using System.ComponentModel.DataAnnotations;

using PaymentGateway.Api.Models;

using Shouldly;

namespace PaymentGateway.Api.Tests;

public class PaymentsValidatorTests
{
    private class TestPaymentsValidator(DateTimeOffset now) : PaymentsValidator
    {
        protected override DateTimeOffset Now => now;
    }

    [Theory]
    [InlineData(2026, 3, 2026, 4, true)]
    [InlineData(2026, 3, 2026, 2, false)]
    [InlineData(2025, 12, 2026, 1, true)]
    [InlineData(2025, 12, 2025, 11, false)]
    public void ValidatesExpiry(int nowYear, int nowMonth, int expiryYear, int expiryMonth, bool isValid)
    {
        // Arrange
        var now = new DateTimeOffset(nowYear, nowMonth, 22, 0, 0, 0, TimeSpan.Zero);
        var validator = new TestPaymentsValidator(now);
        var request = new Models.Requests.PostPaymentRequest
        {
            CardNumber = "01234567890123",
            ExpiryMonth = expiryMonth,
            ExpiryYear = expiryYear,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123"
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.ShouldBe(isValid);
    }

    [Theory]
    [InlineData("GBP", true)]
    [InlineData("USD", true)]
    [InlineData("EUR", true)]
    [InlineData("US", false)]
    [InlineData("GBPP", false)]
    [InlineData("XYZ", false)]
    public void ValidatesCurrencyAttribute(string currency, bool isValid)
    {
        // Arrange
        var request = new Models.Requests.PostPaymentRequest
        {
            CardNumber = "01234567890123",
            ExpiryMonth = 4,
            ExpiryYear = 2026,
            Currency = currency,
            Amount = 100,
            Cvv = "123"
        };

        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        // Act
        var isValidModel = Validator.TryValidateObject(request, context, results, validateAllProperties: true);

        // Assert
        isValidModel.ShouldBe(isValid);
        if (!isValid)
        {
            results.ShouldContain(r => r.MemberNames.Contains(nameof(Models.Requests.PostPaymentRequest.Currency)));
        }
    }
}
