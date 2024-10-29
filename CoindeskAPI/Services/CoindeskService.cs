using System.Net.Http;
using System.Threading.Tasks;
using CoindeskAPI.Middleware;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace CoindeskAPI.Services
{
    public class CoindeskService
    {
        private readonly HttpClient m_httpClient;

        public CoindeskService(HttpClient httpClient)
        {
            m_httpClient = httpClient;
        }

        public async Task<JObject> GetBitcoinPriceIndexAsync()
        {
            var _response = await m_httpClient.GetStringAsync("https://api.coindesk.com/v1/bpi/currentprice.json");
            Debug.WriteLine(_response);
            Console.WriteLine(_response);
            return JObject.Parse(_response);
        }
        public async Task<string> GetBitcoinPriceIndexAsyncRaw()
        {
            var _response = await m_httpClient.GetStringAsync("https://api.coindesk.com/v1/bpi/currentprice.json");
            Debug.WriteLine(_response);
            Console.WriteLine(_response);
            return _response;
        }
    }
}
