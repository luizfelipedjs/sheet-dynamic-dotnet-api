namespace Playground.Api.Models
{
    public class Endpoint
    {
        public string Method { get; set; }
        public string Url { get; set; }
        public int ResponseCode { get; set; }
        public string ResponseBody { get; set; }
    }
}
