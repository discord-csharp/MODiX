namespace Modix.Services.Animals
{
    public abstract class Response
    {
        public bool Success { get; set; }
    }

    public class ByteResponse : Response
    {
        public ByteResponse()
        {
            Success = false;
        }

        public ByteResponse(byte[] bytes)
        {
            Success = true;
            Bytes = bytes;
        }

        public byte[] Bytes { get; set; }
    }

    public class UrlResponse : Response
    {
        public UrlResponse()
        {
            Success = false;
        }

        public UrlResponse(string url)
        {
            Success = true;
            Url = url;
        }

        public string Url { get; set; }
    }
}