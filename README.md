# C#.NET SDK
A lightweight C# SDK for interacting with the Miropay Payment API, built on .NET Core for fast HTTP calls and private key-based request signing.

# Installation
===============================
dotnet add package MiroPaySDK --version 1.0.0


# Usage
using MiroPaySDK.Rest;
using MiroPaySDK.Contracts;
using MiroPaySDK.Rest.Enums;


var client = new PaymentRestClient("privateKey", "secret");

Note:
The secret should contain "test" or "live" to auto-switch between sandbox and production mode.

# Authentication
The SDK uses a private key and a secret to sign every request with HMAC. Signature logic is handled internally.

 Example
string pvKey = `-----BEGIN ENCRYPTED PRIVATE KEY-----
...
-----END ENCRYPTED PRIVATE KEY-----`;

string secret = "live_xxx..."; // or "test_xxx..."

var client = new PaymentRestClient(pvKey, secret);

# API Reference
new PaymentRestClient(string key, string secret)
Creates a new client instance.

createPayment(ICreatePayment payload): CreatePayment : ICreatePayment
Creates a new payment session.

Parameters:

{
    public string Amount { get; set; }            // e.g. Price as string, e.g. "1000"
    public IList<GATEWAY> Gateways { get; set; }  // e.g. [GATEWAY.FIB, GATEWAY.ZAIN]
    public string Title { get; set; }             // Max 63 characters
    public string Description { get; set; }       // Max 255 characters
    public string RedirectUrl { get; set; }       // CallbackUrl or RedirectURL
    public bool CollectFeeFromCustomer { get; set; }
    public bool CollectCustomerEmail { get; set; }
    public bool CollectCustomerPhoneNumber { get; set; }
}

Returns:

IHttpResponse<CreatePaymentResponseBody> response = await CallAsync<CreatePaymentResponseBody>(requestBody: new
{
    amount = payload.Amount,
    gateways = payload.Gateways,
    title = payload.Title,
    description = payload.Description,
    redirectUrl = payload.RedirectUrl,
    collectFeeFromCustomer = payload.CollectFeeFromCustomer,
    collectCustomerEmail = payload.CollectCustomerEmail,
    collectCustomerPhoneNumber = payload.CollectCustomerPhoneNumber
}, path: "/create", method: HttpMethod.Post);
return new CreatePaymentResponse
{
    StatusCode = response.StatusCode,
    Body = response.Body,
    Headers = response.Headers
};


Example:

await client.CreatePaymentAsync({
  amount: "1000",
  gateways: [GATEWAY.FIB, GATEWAY.ZAIN],
  title: "Test",
  description: "Desc",
  callbackUrl: "https://google.com",
  collectFeeFromCustomer: false,
  collectCustomerEmail: false,
  collectCustomerPhoneNumber: false,
});


Task<IPaymentDetailsResponse> GetPaymentByIdAsync(string referenceCode)
Checks the status of a payment.

Returns:

IHttpResponse<PaymentDetailsResponseBody> response = await CallAsync<PaymentDetailsResponseBody>($"/status/{referenceCode}", HttpMethod.Get, null);

return new PaymentDetailsResponse
{
    StatusCode = 200,
    Body = response.Body,
    Headers = { }
};

Example:

var status = await client.GetPaymentByIdAsync("your-reference-code")
Console.WriteLine(status.body.status);


Task<ICancelPaymentResponse> CancelPaymentAsync(string referenceCode)
Cancels an existing payment.

Example:

await clint.CancelPaymentAsync(string referenceCode)

Returns:
IHttpResponse<CancelPaymentResponseBody> response = await CallAsync<CancelPaymentResponseBody>($"/cancel/{referenceCode}", HttpMethod.Patch, null);

return new CancelPaymentResponse
{
    StatusCode = response.StatusCode,
    Body = response.Body,
    Headers = response.Headers
};


# Types
public enum GATEWAY
{
    [EnumMember(Value = "ZAIN")]
    ZAIN,

    [EnumMember(Value = "FIB")]
    FIB
}


public enum PAYMENT_STATUS
{
    [EnumMember(Value = "TIMED_OUT")]
    TIMED_OUT,

    [EnumMember(Value = "PENDING")]
    PENDING,

    [EnumMember(Value = "PAID")]
    PAID,

    [EnumMember(Value = "CANCELED")]
    CANCELED,

    [EnumMember(Value = "FAILED")]
    FAILED,

    [EnumMember(Value = "SETTLED")]
    SETTLED
}

# Need Help?
Contact the payment provider or open an issue on the internal GitHub repo.


# License


# Contributing
