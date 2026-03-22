using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models;
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
        var lastFour = _random.Next(0, 9999);

        var lastFourObj = lastFour switch
        {
            < 10 => new CardLast4($"000{lastFour}"),
            < 100 => new CardLast4($"00{lastFour}"),
            < 1000 => new CardLast4($"0{lastFour}"),
            _ => new CardLast4(lastFour.ToString())
        };

        var payment = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Authorized,
            ExpiryYear = _random.Next(2023, 2030),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumberLastFour = lastFourObj,
            Currency = "GBP"
        };

        var paymentsRepository = new PaymentsRepository();
        paymentsRepository.Add(payment, TestContext.Current.CancellationToken);

        var client = _webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => ((ServiceCollection)services)
                .AddSingleton(paymentsRepository)
                .AddHttpClient<AcquiringBankClient>()))
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

    [Theory]
    [InlineData("0123456789012341", true)] // Triggers accepted from the bank 
    [InlineData("0123456789012342", false)] // Triggers declined from the bank 
    public async Task PostAndRetrieveAPayment(string cardNumber, bool isAccepted)
    {
        var client = _webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => ((ServiceCollection)services)
                .AddSingleton(new PaymentsRepository())
                .AddHttpClient<AcquiringBankClient>()))
            .CreateClient();

        var currentNow = DateTime.UtcNow;

        // Act
        var postRequest = new PostPaymentRequest
        {
            CardNumber = cardNumber,
            ExpiryMonth = currentNow.Month,
            ExpiryYear = currentNow.Year + 3,
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };

        var postResponse =
            await client.PostAsJsonAsync($"/api/Payments/", postRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);

        var postedPayment = await postResponse.Content.ReadFromJsonAsync<PostPaymentResponse>(
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(postedPayment);
        var paymentId = postedPayment.Id;

        var getResponse = await client.GetAsync($"/api/Payments/{paymentId}", TestContext.Current.CancellationToken);

        await ValidatePaymentResponse(getResponse, isAccepted);
    }

    [Theory]
    [InlineData("0123456789012")] // Too short
    [InlineData("01234567890123456789")] // Too long
    [InlineData("0123456789012340")] // Triggers 503 from the bank
    public async Task PostPaymentToBeRejected(string cardNumber)
    {
        var client = _webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => ((ServiceCollection)services)
                .AddSingleton(new PaymentsRepository())
                .AddHttpClient<AcquiringBankClient>()))
            .CreateClient();

        var currentNow = DateTime.UtcNow;

        // Act
        var postRequest = new PostPaymentRequest
        {
            CardNumber = cardNumber,
            ExpiryMonth = currentNow.Month,
            ExpiryYear = currentNow.Year + 3,
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };

        var postResponse =
            await client.PostAsJsonAsync($"/api/Payments/", postRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);

        var postedPayment = await postResponse.Content.ReadFromJsonAsync<PostPaymentResponse>(
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(postedPayment);
        postedPayment.Status.ShouldBe(PaymentStatus.Rejected);
    }

    private async Task ValidatePaymentResponse(HttpResponseMessage? response, bool isAccepted = true)
    {
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var paymentResponse =
            await response.Content.ReadFromJsonAsync<PostPaymentResponse>(
                cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(paymentResponse);

        // This should not be possible, but as the spec says that
        // not adhering to this would be a serious compliance risk, 
        // I am checking this explicitly.
        paymentResponse.CardNumberLastFour.Value.Length.ShouldBe(4);

        paymentResponse.Status.ShouldBe(isAccepted ? PaymentStatus.Authorized : PaymentStatus.Declined);
    }
}