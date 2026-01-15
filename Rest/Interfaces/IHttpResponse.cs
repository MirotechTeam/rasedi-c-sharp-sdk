using System.Collections.Generic;

namespace RasediSDK.Rest.Interfaces
{

    // ======================== Http ======================== //
    public interface IHttpResponse<T>
    {
        T Body { get; set; }

        // Multi-value headers to match IncomingHttpHeaders
        IDictionary<string, string[]> Headers { get; set; }
        int StatusCode { get; set; }
    }



    public class HttpResponse<T> : IHttpResponse<T>
    {
        public int StatusCode { get; set; }
        public T Body { get; set; }
        public IDictionary<string, string[]> Headers { get; set; }
        public string? Message { get; set; }
        public bool Success { get; set; }

        public HttpResponse()
        {
            Headers = new Dictionary<string, string[]>();
        }
    }
}
