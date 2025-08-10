using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using MiroPaySDK.Contracts;
using MiroPaySDK.Rest.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MiroPaySDK.Core;
using MiroPay.Rest.Constants;
using MiroPaySDK.Exceptions;

namespace MiroPaySDK.Rest;

public class PaymentRestClient
{
    private readonly int _upstreamVersion = 1;

    private readonly HttpClient _httpClient;

    private readonly PrivateKeyAuthenticator _authenticator;

    private readonly string _baseUrl;

    private readonly bool _isTest;

    private List<IPublicKeyResponseBody> _publicKeys = new ();


    // ===================================== Constructor ================================== //
    /// <summary>
    /// Creating new client instance for MiroPay Payment API,
    /// Args: key = privateKey,
    /// secret = secretKey,
    /// isTest = true = test mode, false = live mode.
    /// </summary>
    public PaymentRestClient(string key, string secret, bool isTest = true)
    {
        this._httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10.0)
        };

        this._authenticator = new PrivateKeyAuthenticator(key, secret);
        this._isTest = isTest;

        _baseUrl = MiroPay.Rest.Constants.Constants.ApiBaseUrl;
    }


    // ===================================== Basic Method ======================================= //
    //** Calling API
    private async Task<IHttpResponse<T>> CallAsync<T>(string path, HttpMethod method, object? requestBody)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));

        if (method == null)
            throw new ArgumentNullException(nameof(method), "HTTP method cannot be null");


        // Step 1: Compose relative and full URL
        string v = $"/v{_upstreamVersion}";
        string relativeUrl = $"{v}/payment/rest/{(_isTest ? "test" : "live")}{path}";
        string versionedUrl = $"{_baseUrl}{relativeUrl}";

        // Step 2: Generate signature using authenticator
        string signature = await _authenticator.MakeSignatureAsync(method.Method, relativeUrl);

        // Step 3: Create HTTP request
        using var request = new HttpRequestMessage(method, versionedUrl);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("x-signature", signature);
        request.Headers.Add("x-id", _authenticator.KeyId);

        if (requestBody != null)
        {
            var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
            });
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        // Step 4: Send request and get response
        using var response = await _httpClient.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        // Step 5: Parse and return typed response
        try
        {
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(content);

            T? parsed = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            });

            return new HttpResponse<T>
            {
                StatusCode = (int)response.StatusCode,
                Body = parsed!,
                Headers = response.Headers.ToDictionary(h => h.Key, h => h.Value.ToArray())
            };
        }
        catch (JsonException ex)
        {
            throw new JsonException($"Failed to parse JSON: ", ex);
        }
    }

    //** Get public keys
    private async Task<IHttpResponse<List<IPublicKeyResponseBody>>> GetPublicKeysAsync()
    {
        IHttpResponse<List<PublicKeyResponseBody>> response = await CallAsync<List<PublicKeyResponseBody>>("/get-public-keys", HttpMethod.Get, null);

        return new PublicKeyResponse
        {
            StatusCode = response.StatusCode,
            Body = response.Body.Cast<IPublicKeyResponseBody>().ToList(),
            Headers = response.Headers
        };

    }


    //** Get Payment by ID
    public async Task<IPaymentDetailsResponse> GetPaymentByIdAsync(string referenceCode)
    {
        if (string.IsNullOrWhiteSpace(referenceCode))
            throw new ArgumentException("Reference code cannot be null or empty", nameof(referenceCode));

        IHttpResponse<PaymentDetailsResponseBody> response = await CallAsync<PaymentDetailsResponseBody>($"/status/{referenceCode}", HttpMethod.Get, null);

        return new PaymentDetailsResponse
        {
            StatusCode = 200,
            Body = response.Body,
            Headers = { }
        };
    }


    //** Create new payment
    public async Task<ICreatePaymentResponse> CreatePaymentAsync(ICreatePayment payload)
    {

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
    }


    //** Cancel payment
    public async Task<ICancelPaymentResponse> CancelPaymentAsync(string referenceCode)
    {
        if (string.IsNullOrWhiteSpace(referenceCode))
            throw new ArgumentException("Reference code cannot be null or empty", nameof(referenceCode));

        IHttpResponse<CancelPaymentResponseBody> response = await CallAsync<CancelPaymentResponseBody>($"/cancel/{referenceCode}", HttpMethod.Patch, null);

        return new CancelPaymentResponse
        {
            StatusCode = response.StatusCode,
            Body = response.Body,
            Headers = response.Headers
        };
    }

    //** Verify
    public async Task<IVerifyPaymentResponse> VerifyAsync(IVerifyPayload payload)
    {
        if (_publicKeys.Count == 0)
        {
            _publicKeys = (await GetPublicKeysAsync()).Body;
        }

        var targetKey = _publicKeys.Find(k => k.Id == payload.KeyId);
        if (targetKey == null)
        {
            _publicKeys = (await GetPublicKeysAsync()).Body;
            targetKey = _publicKeys.Find(k => k.Id == payload.KeyId);
            if (targetKey == null)
                throw new PublicKeyNotFoundException(payload.KeyId);
        }

        if (payload.Content is null)
            throw new ArgumentNullException(nameof(payload.Content), "Payload content is required.");

        if (string.IsNullOrWhiteSpace(payload.Content))
            throw new InvalidPayloadException("Payload content cannot be null, empty, or whitespace.");

        using var ecdsa = LoadECDsaPublicKey(targetKey.Key);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new ECDsaSecurityKey(ecdsa),
            ValidateIssuer = false,
            ValidateAudience = false,
            RequireExpirationTime = false,
            ValidateLifetime = false,
            ValidAlgorithms = new[] { SecurityAlgorithms.EcdsaSha512 }
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            tokenHandler.ValidateToken(payload.Content, validationParameters, out SecurityToken validatedToken);

            var jwtToken = validatedToken as JwtSecurityToken;
            if (jwtToken == null)
                throw new InvalidPayloadException("The validated token is not a valid JWT.");


            var result = JsonSerializer.Deserialize<IVerifyPaymentResponseBody>(jwtToken.RawPayload);
            if (result == null)
                if (result == null)
                    throw new PayloadDeserializationException("Failed to deserialize JWT payload into IVerifyPaymentResponseBody.");


            return (IVerifyPaymentResponse)result;
        }
        catch (SecurityTokenException ex)
        {
            throw new JwtValidationException("JWT validation failed.", ex);
        }
    }

    /// <summary>
    /// Load an ECDsa public key from PEM-formatted string.
    /// </summary>
    private ECDsa LoadECDsaPublicKey(string pem)
    {
        var publicKeyBytes = ParsePemToBytes(pem);
        var ecdsa = ECDsa.Create();
        ecdsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
        return ecdsa;
    }

    /// <summary>
    /// Parses PEM string to raw byte array.
    /// Assumes standard "-----BEGIN PUBLIC KEY-----" PEM format.
    /// </summary>
    private static byte[] ParsePemToBytes(string pem)
    {
        const string header = "-----BEGIN PUBLIC KEY-----";
        const string footer = "-----END PUBLIC KEY-----";

        int start = pem.IndexOf(header, StringComparison.Ordinal);
        if (start >= 0)
        {
            start += header.Length;
            int end = pem.IndexOf(footer, start, StringComparison.Ordinal);
            if (end >= 0)
            {
                string base64 = pem.Substring(start, end - start);
                base64 = base64.Replace("\r", "").Replace("\n", "").Trim();
                return Convert.FromBase64String(base64);
            }
        }

        throw new PemFormatException("Invalid PEM format.");
    }
}