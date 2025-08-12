using MiroPaySDK.Core.Interfaces;
using Org.BouncyCastle.Asn1.Crmf;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using System.Diagnostics;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace MiroPaySDK.Core
{
    public class PrivateKeyAuthenticator : IAuthenticator
    {
        private readonly string _encryptedPrivateKey;
        private readonly string _secret;


        // ============================== Constructor ============================== //
        public PrivateKeyAuthenticator(string encryptedPrivateKey, string secret)
        {
            this._encryptedPrivateKey = encryptedPrivateKey;
            this._secret = secret;
        }


        // ============================== Properties ============================== //
        public string Secret { get; set; }

        public string EncryptedPrivateKey { get; set; }

        public string KeyId => this._secret;


        // ============================== Methods ============================== //
        public string MakeSignature(params object[] args)
        {
            string method = args[0].ToString();
            string relativeUrl = args[1].ToString();
            try
            {
                return GenerateSignature(method, relativeUrl);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error while generating ED25519 signature.", ex);
            }
        }

        public async Task<string> MakeSignatureAsync(params object[] args)
        {
            try
            {
                return await Task.Run(() => MakeSignature(args));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error while generating ED25519 signature (async).", ex);
            }
        }

        private string GenerateSignature(string method, string relativeUrl)
        {
            if (string.IsNullOrWhiteSpace(method)) throw new ArgumentException("Method is required.");
            if (string.IsNullOrWhiteSpace(relativeUrl)) throw new ArgumentException("Relative URL is required.");

            // Step 1: Build the raw signature string
            string rawSign = $"{method} || {this._secret} || {relativeUrl}";

            // Step 2: Base64-encode the raw string
            string base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawSign));
            byte[] dataToSign = Convert.FromBase64String(base64String);

            // Step 3: Load the ED25519 private key
            var privateKey = LoadPrivateKey(this._encryptedPrivateKey, this._secret);

            // Step 4: Sign the base64-encoded data
            var signer = new Ed25519Signer();
            signer.Init(true, privateKey);
            signer.BlockUpdate(dataToSign, 0, dataToSign.Length);
            byte[] signatureBytes = signer.GenerateSignature();

            // Step 5: Return Base64 string
            return Convert.ToBase64String(signatureBytes);
        }


        // ============================== Helper ============================== //
        private static Ed25519PrivateKeyParameters LoadPrivateKey(string pem, string password)
        {
            try
            {
                using var reader = new StringReader(pem);
                PemReader pemReader;

                if (pem.Contains("ENCRYPTED PRIVATE KEY"))
                {
                    if (string.IsNullOrEmpty(password)) throw new InvalidOperationException("Password is required for encrypted private key.");

                    pemReader = new PemReader(reader, new Password(password));
                }
                else
                {
                    pemReader = new PemReader(reader);
                }

                object obj = pemReader.ReadObject();

                //When PEM contains a full key pair (public + private keys).
                //Check if the obj is an instance of AsymmetricCipherKeyPair and private key is stored inside keyPair.Private.
                if (obj is AsymmetricCipherKeyPair keyPair)
                    return (Ed25519PrivateKeyParameters)keyPair.Private;

                //Checks if obj directly is an instance of Ed25519PrivateKeyParameters
                //It means the PEM contains only privateKey not keyPair, so it returns that key directly without casting it.
                if (obj is Ed25519PrivateKeyParameters privateKey)
                    return privateKey;

                throw new InvalidOperationException("Unsupported or unreadable ED25519 private key.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to load ED25519 private key.", ex);
            }
        }

        // ============================== Password Helper ============================== //
        // This class is sealed because it does not extend to the outside.
        private sealed class Password : IPasswordFinder
        {
            private readonly string _pwd;
            public Password(string pwd) => _pwd = pwd;
            public char[] GetPassword() => _pwd.ToCharArray();
        }
    }
}