using System.Threading.Tasks;

namespace RasediSDK.Core.Interfaces
{
    public interface IAuthenticator
    {
        /// <summary>
        /// Generate a signature string synchronously.
        /// </summary>
        /// <param name="args">Arguments for signature generation (e.g. method, URL)</param>
        /// <returns>Signature string</returns>
        Task<string> MakeSignatureAsync(params object[] args);


        /// <summary>
        /// Generate a signature string synchronously.
        /// </summary>
        /// <param name="args">Arguments for signature generation (e.g. method, URL)</param>
        /// <returns>Signature string</returns>
        string MakeSignature(params object[] args);

        string KeyId { get; }
    }
}
