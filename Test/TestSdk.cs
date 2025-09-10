using MiroPaySDK.Contracts;
using MiroPaySDK.Rest;
using MiroPaySDK.Rest.Enums;
using System;

namespace MiroPay.Test
{
    static class TestSdk
    {
        public static async Task Main(string[] args)
        {

            var client = new PaymentRestClient("Your private key", "Your secret key");



            var request = new CreatePayment
            {
                Amount = "2500",
                Gateways = new[] { GateWays.FIB, GateWays.ZAIN, GateWays.ASIA_PAY, GateWays.FAST_PAY, GateWays.SUPER_QI },
                Title = "Title ...",
                Description = "Description ...",
                RedirectUrl = "https://www.yoursite.com/sample",
                CollectFeeFromCustomer = true,
                CollectCustomerEmail = true,
                CollectCustomerPhoneNumber = true,
            };

            var response = await client.CreatePaymentAsync(request);

            Console.WriteLine($"Status Code: {response.StatusCode}");
            Console.WriteLine($"Reference Code: {response.Body?.ReferenceCode}");
            Console.WriteLine($"Amount: {response.Body?.Amount}");
            Console.WriteLine($"Status: {response.Body?.Status}");
            Console.WriteLine($"Redirect URL: {response.Body?.RedirectUrl}");
            Console.WriteLine($"Paid At: {response.Body?.PaidAt}");
            Console.WriteLine($"Paid Via: {response.Body?.PaidVia}");
        }
    }
}
