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
global using MiroPaySDK.Exceptions;

var client = new PaymentRestClient(privateKey, secretKey, isTest);
```

> **Note:**  
> The switch between sandbox and production mode will be manually throw the `isTest` boolean value `(true = sandbox, false = production mode)`.

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

bool isTest = false // or true.

var client = new PaymentRestClient(privateKey, secretKey, isTest);
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
    GATEWAY[] gateways; // e.g. [GATEWAY.ZAIN, GATEWAY.FIB]
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
    PAYMENT_STATUS Status, 
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
  gateways: [GATEWAY.FIB],
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
    PAYMENT_STATUS Status, 
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
    PAYMENT_STATUS Status, 
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

## 🏷️ Types

```C#
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
```

---

## 💬 Need Help?

Contact the payment provider or open an issue on the internal GitHub repo.
