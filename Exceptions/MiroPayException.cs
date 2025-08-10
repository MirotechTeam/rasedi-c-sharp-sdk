using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

///<summary>
/// Exception-handling strategy for your MiroPaySDK, covering allmost all the exception cases
///</summary>
namespace MiroPaySDK.Exceptions
{
    public class MiroPayException : Exception
    {
        public MiroPayException(string message) : base(message) { }
        public MiroPayException(string message, Exception innerException) : base(message, innerException) { }
    }


    public class PublicKeyNotFoundException : MiroPayException
    {
        public PublicKeyNotFoundException(string keyId)
            : base($"Public key with ID '{keyId}' was not found.") { }
    }

    public class InvalidPayloadException : MiroPayException
    {
        public InvalidPayloadException(string message)
            : base(message) { }
    }

    public class JwtValidationException : MiroPayException
    {
        public JwtValidationException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class PayloadDeserializationException : MiroPayException
    {
        public PayloadDeserializationException(string message)
            : base(message) { }
    }

    public class PemFormatException : MiroPayException
    {
        public PemFormatException(string message) : base(message) { }
    }
}
