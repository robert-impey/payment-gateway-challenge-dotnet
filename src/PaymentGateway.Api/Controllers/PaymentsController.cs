using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(
    PaymentsRepository paymentsRepository,
    AcquiringBankClient bankClient,
    PaymentsValidator paymentsValidator
    ) : Controller
{
    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse>> PostPaymentAsync(PostPaymentRequest request, CancellationToken token)
    {
        if (!ModelState.IsValid)
        {
            // In line with the spec, I am returning a rejected payment.
            // The null suppressions and the default values are a bit of 
            // a smell.
            // A more idiomatic response would be to return a 400 (Bad Request).
            return new PostPaymentResponse
            {
                Status = PaymentStatus.Rejected,
                Id = Guid.Empty,
                CardNumberLastFour = null!,
                ExpiryMonth = 0,
                ExpiryYear = 0,
                Currency = null!,
                Amount = 0
            };
        }

        if (!paymentsValidator.Validate(request))
        {
            return BadRequest("Invalid payment request.");
        }

        try
        {
            var bankResponse = await bankClient.MakePayment(request, token);

            if (bankResponse is null)
            {
                return BadRequest("Failed to process payment.");
            }

            if (bankResponse.Status != Enums.PaymentStatus.Rejected)
            {
                paymentsRepository.Add(bankResponse, token);
            }

            return new OkObjectResult(bankResponse);
        }
        catch (Exception)
        {
            return BadRequest("Failed to process payment.");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id, CancellationToken token)
    {
        var payment = await paymentsRepository.Get(id, token);

        // If the payment is not found, return a 404 (Not Found)
        if (payment is null)
        {
            return NotFound();
        }

        return new OkObjectResult(payment);
    }
}