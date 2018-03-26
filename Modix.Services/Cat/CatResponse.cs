namespace Modix.Services.Cat
{
    public abstract class CatResponse
    {
        public bool Success { get; set; }
    }

    public class ByteCatResponse : CatResponse
    {
        public ByteCatResponse()
        {
            Success = false;
        }

        public ByteCatResponse(byte[] bytes)
        {
            Success = true;
            Bytes = bytes;
        }

        public byte[] Bytes { get; set; }
    }

    public class UrlCatResponse : CatResponse
    {
        public UrlCatResponse()
        {
            Success = false;
        }

        public UrlCatResponse(string catUrl)
        {
            Success = true;
            Url = catUrl;
        }

        public string Url { get; set; }
    }
}