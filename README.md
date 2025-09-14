# C#.NET SDK

A lightweight SDK for interacting with the Miropay Payment API, built on top of [HttpClient in System.Net.Http and System.Net.Http.Headers] for fast, native HTTP calls and private key-based request signing.

---

## 📦 Installation

```bash
dotnet add package MiroPaySDK --version x.x.x
```

---

## 🚀 Usage

```C#
using MiroPaySDK.Rest;
using MiroPaySDK.Rest.Enums;
using MiroPaySDK.Rest.Interfaces;
using MiroPaySDK.Exceptions;

var client = new PaymentRestClient(privateKey, secretKey);
```

> **Note:**  
> The switch between sandbox and production mode will be automatically based on (test) and (live) keyword inside secretKey `(test = sandbox mode, live = production mode)`.

---

## 🔐 Authentication

The SDK uses a private key and a secret to sign every request with HMAC. Signature logic is handled internally.

---

## 🧪 Example

```C#
string privateKey = `-----BEGIN ENCRYPTED PRIVATE KEY-----
...
-----END ENCRYPTED PRIVATE KEY-----`;

string secretKey = "live_xxx..."; // or "test_xxx..."

var client = new PaymentRestClient(privateKey, secretKey);
```

---

## 📘 API Reference

#### `new PaymentRestClient(string privateKey, string secretKey)`

Creates a new client instance.

---

#### `async Task<ICreatePaymentResponse> CreatePaymentAsync(ICreatePayment payload)`

Creates a new payment session.

**Hint:** The `amount` parameter cannot be a decimal value.

**Parameters:**

```C#
{
    string amount; // e.g. "1000"
    GateWays[] gateways; // e.g. [GateWays.ZAIN, GateWays.FIB]
    string title;
    string description;
    string callbackUrl;
    bool collectFeeFromCustomer;
    bool collectCustomerEmail;
    bool collectCustomerPhoneNumber;
}
```

**Returns:**

```C#
<CreatePaymentResponseBody>(Body:
  {
    string ReferenceCode,
    string Amount,
    string? PaidVia,
    string? PaidAt,
    string? CallbackUrl,
    PaymentStatuses Status, 
    string? PayoutAmount
  });
  return new CreatePaymentResponse
    {
      StatusCode = response.StatusCode,
      Body = response.Body,
      Headers = response.Headers
    };
```

**Example:**

```C#
await client.CreatePaymentAsync({
  amount: "1000",
  gateways: [GateWays.FIB],
  title: "Test",
  description: "Desc",
  callbackUrl: "https://google.com",
  collectFeeFromCustomer: false,
  collectCustomerEmail: false,
  collectCustomerPhoneNumber: false,
});
```

---

#### `async Task<IPaymentDetailsResponse> GetPaymentByIdAsync(string referenceCode)`

Checks the status of a payment.

**Returns:**

```C#
<CreatePaymentResponseBody>(Body:
  {
    string ReferenceCode,
    string Amount,
    string? PaidVia,
    string? PaidAt,
    string? CallbackUrl,
    PaymentStatuses Status, 
    string? PayoutAmount

  });
  return new CreatePaymentResponse
  {
    StatusCode = response.StatusCode,
    Body = response.Body,
    Headers = response.Headers
  };
```

**Example:**

```C#
var status = await client.GetPaymentByIdAsync("your-reference-code");
Console.WriteLine(status.body.status);
```

---

#### `async Task<ICancelPaymentResponse> CancelPaymentAsync(string referenceCode)`

Cancels an existing payment.

**Example:**

```C#
await client.CancelPaymentAsync("your-reference-code");
```

**Returns:**

```C#
<CancelPaymentResponseBody>(Body:
  {
    string ReferenceCode,
    string Amount,
    string? PaidVia,
    string? PaidAt,
    string? CallbackUrl,
    PaymentStatuses Status, 
    string? PayoutAmount

  });
  return new CreatePaymentResponse
  {
    StatusCode = response.StatusCode,
    Body = response.Body,
    Headers = response.Headers
  };
```

---
#### `Using MiroPaySDK.Exception to catch different type of exception`

The exceptions help you to catch more specific and error-related issues.

**Example:**

```C#
try
  {
      // 1. Try to get a public key by ID
      ...................................

      // 2. Validate payload content
      ...................................

      // 3. More SDK calls...............
  }
  catch (PublicKeyNotFoundException ex)
  {
      Console.WriteLine("Handle missing key specifically: " + ex.Message);
  }
  catch (InvalidPayloadException ex)
  {
      Console.WriteLine("Handle invalid payload specifically: " + ex.Message);
  }
  catch (JwtValidationException ex)
  {
      Console.WriteLine("Handle JWT validation errors: " + ex.Message);
  }
  catch (PayloadDeserializationException ex)
  {
      Console.WriteLine("Handle deserialization errors: " + ex.Message);
  }
  catch (PemFormatException ex)
  {
      Console.WriteLine("Handle PEM format errors: " + ex.Message);
  }
  catch (MiroPayException ex)
  {
      // Catch any other SDK-related errors not explicitly caught above
      Console.WriteLine("General MiroPay SDK error: " + ex.Message);
  }
  catch (Exception ex)
  {
      // Catch unexpected errors not related to SDK
      Console.WriteLine("Unexpected error: " + ex.Message);
  }
```

---

## 🏷️ Types

```C#
public enum GateWays
{
    [EnumMember(Value = "ZAIN")]
    ZAIN,

    [EnumMember(Value = "FIB")]
    FIB,

    [EnumMember(Value = "ASIA_PAY")]
    ASIA_PAY,

    [EnumMember(Value = "FAST_PAY")]
    FAST_PAY,

    [EnumMember(Value = "SUPER_QI")]
    SUPER_QI
}
```
```
public enum PaymentStatuses
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
```

---

## 💬 Need Help?

Contact the payment provider or open an issue on the internal GitHub repo.

---
## 🎯 Testing SDK

You can test the SDK [ ➡️ here](https://github.com/MirotechTeam/miropay-c-sharp-sdk/tree/master/Test).

---
