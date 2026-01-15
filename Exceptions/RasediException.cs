using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

///<summary>
/// Exception-handling strategy for your RasediSDK, covering allmost all the exception cases
///</summary>
namespace RasediSDK.Exceptions
{
    public class RasediException : Exception
    {
        public RasediException(string message) : base(message) { }
        public RasediException(string message, Exception innerException) : base(message, innerException) { }
    }


    public class PublicKeyNotFoundException : RasediException
    {
        public PublicKeyNotFoundException(string keyId)
            : base($"Public key with ID '{keyId}' was not found.") { }
    }

    public class InvalidPayloadException : RasediException
    {
        public InvalidPayloadException(string message)
            : base(message) { }
    }

    public class JwtValidationException : RasediException
    {
        public JwtValidationException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class PayloadDeserializationException : RasediException
    {
        public PayloadDeserializationException(string message)
            : base(message) { }
    }

    public class PemFormatException : RasediException
    {
        public PemFormatException(string message) : base(message) { }
    }
}
