using System;
using System.Collections.Generic;

namespace RasediSDK.Contracts
{
    // ======================== Create Payment ======================= //
    public interface ICreatePayment
    {
        
        /// <summary>
        /// Price as string, e.g. "1000"
        /// </summary>
        public string Amount { get; set; }


        /// <summary>
        /// List of allowed gateways; empty array means all are allowed
        /// </summary>
        public IList<GateWays> Gateways { get; set; }

        /// <summary>
        /// Max 63 characters
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Max 255 characters
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// RedirectUrl
        /// </summary>
        public string RedirectUrl { get; set; }

        /// <summary>
        /// CallbackUrl
        /// </summary>
        public string CallbackUrl { get; set; }

        public bool CollectFeeFromCustomer { get; set; }

        public bool CollectCustomerEmail { get; set; }

        public bool CollectCustomerPhoneNumber { get; set; }
    }

    public interface ICreatePaymentResponseBody
    {
        public string ReferenceCode { get; set; }
        public string Amount { get; set; }
        public string? PaidVia { get; set; }
        public string? PaidAt { get; set; }
        public string RedirectUrl { get; set; }
        public string CallbackUrl { get; set; }
        public PaymentStatuses Status { get; set; }
        public string? PayoutAmount { get; set; }
    }

    public class CreatePaymentResponseBody : ICreatePaymentResponseBody
    {
        public string ReferenceCode { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string? PaidVia { get; set; }
        public string? PaidAt { get; set; }
        public string RedirectUrl { get; set; } = string.Empty;
        public string CallbackUrl { get; set; } = string.Empty;
        public PaymentStatuses Status { get; set; }
        public string? PayoutAmount { get; set; }
    }
    public interface ICreatePaymentResponse : IHttpResponse<ICreatePaymentResponseBody>
    {
    }

    public class CreatePaymentResponse : ICreatePaymentResponse
    {
        public int StatusCode { get; set; }
        public ICreatePaymentResponseBody? Body { get; set; }
        public IDictionary<string, string[]> Headers { get; set; } = new Dictionary<string, string[]>();
    }

    

    public class CreatePayment : ICreatePayment
    {
        public string Amount { get; set; } = string.Empty;
        public IList<GateWays> Gateways { get; set; } = new List<GateWays>();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RedirectUrl { get; set; } = string.Empty;
        public string CallbackUrl { get; set; } = string.Empty;
        public bool CollectFeeFromCustomer { get; set; }
        public bool CollectCustomerEmail { get; set; }
        public bool CollectCustomerPhoneNumber { get; set; }
    }


    // ========================= Get Payment ========================= //
    public interface IPaymentDetailsResponseBody
    {
        public string ReferenceCode { get; set; }
        public string Amount { get; set; }
        public string? PaidVia { get; set; }
        public string? PaidAt { get; set; }
        public string CallbackUrl { get; set; }
        public PaymentStatuses Status { get; set; }
        public string? PayoutAmount { get; set; }
    }

    public interface IPaymentDetailsResponse : IHttpResponse<IPaymentDetailsResponseBody>
    {
    }

    public class PaymentDetailsResponseBody : IPaymentDetailsResponseBody
    {
        public required string ReferenceCode { get; set; }
        public required string Amount { get; set; }
        public string? PaidVia { get; set; }
        public string? PaidAt { get; set; }
        public  string? CallbackUrl { get; set; }
        public PaymentStatuses Status { get; set; }
        public string? PayoutAmount { get; set; }
    }


    public class PaymentDetailsResponse : IPaymentDetailsResponse
    {
        public int StatusCode { get; set; }
        public IPaymentDetailsResponseBody? Body { get; set; }
        public IDictionary<string, string[]> Headers { get; set; } = new Dictionary<string, string[]>();
    }


    // ======================== Cancel Payment ======================= //
    public interface ICancelPaymentResponseBody
    {
        public string ReferenceCode { get; set; }
        public string Amount { get; set; }
        public string? PaidVia { get; set; }
        public string? PaidAt { get; set; }
        public string CallbackUrl { get; set; }
        public PaymentStatuses Status { get; set; }
        public string? PayoutAmount { get; set; }
    }

    public interface ICancelPaymentResponse : IHttpResponse<ICancelPaymentResponseBody>
    {
    }

    public class CancelPaymentResponseBody : ICancelPaymentResponseBody
    {
        public required string ReferenceCode { get; set; }
        public required string Amount { get; set; }
        public string? PaidVia { get; set; }
        public string? PaidAt { get; set; }
        public string? CallbackUrl { get; set; }
        public PaymentStatuses Status { get; set; }
        public string? PayoutAmount { get; set; }
    }

    public class CancelPaymentResponse : ICancelPaymentResponse
    {
        public int StatusCode { get; set; }
        public ICancelPaymentResponseBody? Body { get; set; }
        public IDictionary<string, string[]> Headers { get; set; } = new Dictionary<string, string[]>();
    }


    // ========================= Get API Keys ======================== //
    public interface IPublicKeyResponseBody
    {
        public string Id { get; set; }
        public string Key { get; set; }
    }

    public interface IPublicKeysResponse : IHttpResponse<List<IPublicKeyResponseBody>>
    {
    }


    public class PublicKeyResponseBody : IPublicKeyResponseBody
    {
        public string Id { get; set; }
        public string Key { get; set; }
    }

    public class PublicKeyResponse : IPublicKeysResponse
    {
        public int StatusCode { get; set; }
        public List<IPublicKeyResponseBody>? Body { get; set; }
        public IDictionary<string, string[]> Headers { get; set; } = new Dictionary<string, string[]>();
    }


    // ============================ Verify =========================== //
    public interface IVerifyPayload
    {
        public string KeyId { get; set; }

        public string? Content { get; set; }
    }

    public interface IVerifyPaymentResponseBody
    {
        public string ReferenceCode { get; set; }
        public PaymentStatuses Status { get; set; }
        public string? PayoutAmount { get; set; }
    }

    public interface IVerifyPaymentResponse : IHttpResponse<IVerifyPaymentResponseBody>
    {
    }

    public class VerifyPayload : IVerifyPayload
    {
        public string KeyId { get; set; }

        public string? Content { get; set; }
    }

    public class VerifyPaymentResponseBody : IVerifyPaymentResponseBody
    {
        public string ReferenceCode { get; set; }
        public PaymentStatuses Status { get; set; }
        public string? PayoutAmount { get; set; }
    }
}
