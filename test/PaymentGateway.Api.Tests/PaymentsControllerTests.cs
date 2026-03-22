using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

using Shouldly;

namespace PaymentGateway.Api.Tests;

public class PaymentsControllerTests
{
    private readonly Random _random = new();
    private readonly WebApplicationFactory<PaymentsController> _webApplicationFactory = new();

    [Fact]
    public async Task RetrievesAPaymentSuccessfully()
    {
        // Arrange
        var payment = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Authorized,
            ExpiryYear = _random.Next(2023, 2030),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumberLastFour = new Models.CardLast4(_random.Next(0, 9999).ToString()),
            Currency = "GBP"
        };

        var paymentsRepository = new PaymentsRepository();
        paymentsRepository.Add(payment, TestContext.Current.CancellationToken);

        var client = _webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => ((ServiceCollection)services)
                .AddSingleton(paymentsRepository)))
            .CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Payments/{payment.Id}", TestContext.Current.CancellationToken);

        await ValidatePaymentResponse(response);
    }

    [Fact]
    public async Task Returns404IfPaymentNotFound()
    {
        // Arrange
        var client = _webApplicationFactory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostAndRetrieveAPayment()
    {
        var client = _webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => ((ServiceCollection)services)
                .AddSingleton(new PaymentsRepository()))) // Providing an empty repository
            .CreateClient();

        var currentNow = DateTime.UtcNow;

        // Act
        var postRequest = new PostPaymentRequest
        {
            CardNumber = "1234567890123456",
            ExpiryMonth = currentNow.Month,
            ExpiryYear = currentNow.Year + 3,
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };

        var postResponse = await client.PostAsJsonAsync($"/api/Payments/", postRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);

        var postedPayment = await postResponse.Content.ReadFromJsonAsync<PostPaymentResponse>(
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(postedPayment);
        var paymentId = postedPayment.Id;

        var getResponse = await client.GetAsync($"/api/Payments/{paymentId}", TestContext.Current.CancellationToken);

        await ValidatePaymentResponse(getResponse);
    }

    private async Task ValidatePaymentResponse(HttpResponseMessage? response)
    {
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(paymentResponse);

        // This should not be possible, but as the spec says that
        // not adhering to this would be a serious compliance risk, 
        // I am checking this explicitly.
        paymentResponse.CardNumberLastFour.Value.Length.ShouldBe(4);
    }
}