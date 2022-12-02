using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Logging;
using DeepDungeonDex.Models;
using DeepDungeonDex.Storage;
using Newtonsoft.Json;

namespace DeepDungeonDex.Requests
{
    public class Data : IDisposable
    {
        private readonly CancellationTokenSource token = new();
        private readonly Thread loadThread;
        public static HttpClient Client = new();
        public const string BaseUrl = "https://raw.githubusercontent.com/wolfcomp/DeepDungeonDex/data";
        public static TimeSpan CacheTime = TimeSpan.FromHours(6);
        public StorageHandler Handler;

        public Data(StorageHandler handler)
        {
            Handler = handler;
            handler.AddJsonStorage("index.json", GetFileList().Result!);
            loadThread = new Thread(() => RefreshFileList());
            loadThread.Start();
        }

        public async Task<Dictionary<string, string[]>?> GetFileList()
        {
            try
            {
                var content = await Get("index.json");
                return JsonConvert.DeserializeObject<Dictionary<string, string[]>>(content);
            }
            catch(Exception e)
            {
                PluginLog.Error(e, "");
                return null;
            }
        }

        public async Task RefreshFileList(bool continuous = true)
        {
            var list = await GetFileList();
            if (list == null)
                goto RefreshEnd;

            Handler.AddJsonStorage("index.json", list);

            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetInterfaces().Contains(typeof(ILoadableString))).ToArray();

            foreach (var (className, files) in list)
            {
                var type = types.FirstOrDefault(t => t.Name == className);
                if(type == null)
                    continue;
                foreach (var file in files)
                {
                    try
                    {
                        var content = await Get(file);
                        var instance = (ILoadableString)Activator.CreateInstance(type)!;
                        Handler.AddYmlStorage(file, instance.Load(content, false));
                    }
                    catch(Exception e)
                    {
                        PluginLog.Error(e, "");
                    }
                }
            }

            Handler.Save();

            RefreshEnd:
            if (continuous)
            {
                await Task.Delay(CacheTime, token.Token);
                if (!token.IsCancellationRequested)
                    await RefreshFileList();
            }
        }

        public static async Task<string> Get(string url)
        {
            PluginLog.Debug($"Requesting {BaseUrl}/{url}");
            var response = await Client.GetAsync($"{BaseUrl}/{url}");
            PluginLog.Debug($"Response: {response.StatusCode}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            throw new HttpRequestException($"Request: {url}\nFailed with status code {response.StatusCode}");
        }

        public void Dispose()
        {
            token.Cancel();
            loadThread.Join();
            token.Dispose();
        }
    }
}
