using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DeepDungeonDex.Storage;

namespace DeepDungeonDex.Requests
{
    public class Data
    {
        public static HttpClient Client = new();
        public const string BaseUrl = "https://raw.githubusercontent.com/wolfcomp/DeepDungeonDex/data";

        public Data(StorageHandler handler)
        {
        }

        public static async Task<string> Get(string url)
        {
            var response = await Client.GetAsync($"{BaseUrl}/{url}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            throw new HttpRequestException($"Request: {url}\nFailed with status code {response.StatusCode}");
        }
    }
}
