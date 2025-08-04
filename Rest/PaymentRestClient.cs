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

namespace MiroPaySDK.Rest;

public class PaymentRestClient
{
    private readonly int _upstreamVersion = 1;

    private readonly HttpClient _httpClient;

    private readonly PrivateKeyAuthenticator _authenticator;

    private readonly string _baseUrl = MiroPay.Rest.Constants.Constants.ApiBaseUrl;

    private readonly bool _isTest = true;

    private List<IPublicKeyResponseBody> _publicKeys = new ();


    // ======================== Constructor =========================== //
    public PaymentRestClient(string key, string secret)
    {
        this._httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10.0)
        };
        this._authenticator = new PrivateKeyAuthenticator(key, secret);
        this._isTest = CheckIsTest(secret);
    }


    // Check the secret if it is test or not
    private bool CheckIsTest(string secret)
    {
        return secret.Contains("test", StringComparison.OrdinalIgnoreCase);
    }

    //A method to trim baseUrl if needed
    private string TrimBaseUrl(string? hostName)
    {
        if (string.IsNullOrWhiteSpace(hostName))
        {
            return _baseUrl;
        }

        if (!hostName.StartsWith("https://"))
        {
            hostName = ((!hostName.StartsWith("http://")) ? ("https://" + hostName) : hostName.Replace("http://", "https://"));
        }

        return hostName.TrimEnd('/');
    }


    // ================================= Basic Method ======================================= //
    //** Calling API
    private async Task<IHttpResponse<T>> CallAsync<T>(string path, HttpMethod method, object? requestBody)
    {
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
            //var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            //{
            //    Converters = { new EnumMemberJsonConverter<GATEWAY>() },
            //    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            //});

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
            throw new Exception($"Failed to parse JSON: {ex.Message}", ex);
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

        //return await CallAsync<List<IPublicKeyResponseBody>>("/get-public-keys", HttpMethod.Get, null);
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

        //return (ICancelPaymentResponse)(await CallAsync<ICancelPaymentResponseBody>($"/cancel/{referenceCode}", HttpMethod.Patch, null)).Body;
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
                throw new Exception("Internal server error");
        }

        if (string.IsNullOrWhiteSpace(payload.Content))
            throw new Exception("Internal server error");

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
                throw new Exception("Invalid JWT token");

            var result = JsonSerializer.Deserialize<IVerifyPaymentResponseBody>(jwtToken.RawPayload);
            if (result == null)
                throw new Exception("Failed to deserialize JWT payload");

            return (IVerifyPaymentResponse)result;
        }
        catch (SecurityTokenException ex)
        {
            throw new Exception("JWT validation failed", ex);
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
    private byte[] ParsePemToBytes(string pem)
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

        throw new ArgumentException("Invalid PEM format");
    }
}



/*This is a helper class to handle enum serialization and deserialization
 If you use this:     PropertyNamingPolicy = JsonNamingPolicy.CamelCase
while serializing enums, you can use this converter to handle EnumMember attributes properly. */

public class EnumMemberJsonConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        foreach (var field in typeof(T).GetFields())
        {
            if (Attribute.GetCustomAttribute(field, typeof(EnumMemberAttribute)) is EnumMemberAttribute attr)
            {
                if (attr.Value == value)
                    return (T)field.GetValue(null)!;
            }
            else if (field.Name == value)
            {
                return (T)field.GetValue(null)!;
            }
        }
        throw new JsonException($"Unknown value '{value}' for enum '{typeof(T)}'");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var field = typeof(T).GetField(value.ToString());
        if (field != null && Attribute.GetCustomAttribute(field, typeof(EnumMemberAttribute)) is EnumMemberAttribute attr)
        {
            writer.WriteStringValue(attr.Value);
        }
        else
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}