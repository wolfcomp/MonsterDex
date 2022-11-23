using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Logging;
using DeepDungeonDex.Storage;
using Newtonsoft.Json;

namespace DeepDungeonDex.Requests
{
    public class Data
    {
        public static HttpClient Client = new();
        public const string BaseUrl = "https://raw.githubusercontent.com/wolfcomp/DeepDungeonDex/data";
        public static TimeSpan CacheTime = TimeSpan.FromHours(6);
        public StorageHandler Handler;

        public Data(StorageHandler handler)
        {
            Handler = handler;
            handler.AddJsonStorage("index.json", GetFileList().Result!);
            Task.Factory.StartNew(() => RefreshFileList(), TaskCreationOptions.LongRunning);
        }

        public async Task<string[]?> GetFileList()
        {
            try
            {
                var content = await Get("index.json");
                return JsonConvert.DeserializeObject<string[]>(content);
            }
            catch(Exception e)
            {
                PluginLog.Error(e, e.Message);
                return null;
            }
        }

        public async Task RefreshFileList(bool continuous = true)
        {
            var list = await GetFileList();
            if (list == null)
                goto RefreshEnd;

            Handler.AddJsonStorage("index.json", list);

            foreach (var file in list)
            {
                try
                {
                    var content = await Get(file);
                    Handler.AddYmlStorage(file, new MobData().Load(content));
                }
                catch(Exception e)
                {
                    PluginLog.Error(e, e.Message);
                }
            }

            Handler.Save();

            RefreshEnd:
            if (continuous)
            {
                await Task.Delay(CacheTime);
                await RefreshFileList();
            }
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
