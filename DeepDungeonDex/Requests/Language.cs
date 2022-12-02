using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Logging;
using DeepDungeonDex.Storage;
using Newtonsoft.Json;

namespace DeepDungeonDex.Requests
{
    public class Language : IDisposable
    {
        private readonly CancellationTokenSource token = new();
        private readonly Thread loadThread;
        public static TimeSpan CacheTime = TimeSpan.FromHours(6);
        public StorageHandler Handler;

        public Language(StorageHandler handler)
        {
            Handler = handler;
            loadThread = new Thread(() => RefreshLang());
            loadThread.Start();
        }

        public async Task<LocaleKeys?> GetFileList()
        {
            try
            {
                var list = await Get("locales.json");
                return new LocaleKeys { LocaleDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(list)!};
            }
            catch (Exception e)
            {
                PluginLog.Error(e, e.Message);
                return null;
            }
        }

        public async Task RefreshLang(bool continuous = true)
        {
            var fileList = await GetFileList();
            if (fileList == null) 
                goto RefreshEnd;

            Handler.AddJsonStorage("locales.json", fileList);

            if(Handler.GetInstance("index.json") is not Dictionary<string, string> list)
                goto RefreshEnd;

            foreach (var (name, folders) in fileList.LocaleDictionary)
            {
                foreach (var (_, file) in list)
                {
                    try
                    {
                        var path = $"{name}/{file}";
                        var content = await Get(path);
                        Handler.AddYmlStorage(path, new Locale{ TranslationDictionary = StorageHandler.Deserializer.Deserialize<Dictionary<string, string>>(content)});
                    }
                    catch (Exception e)
                    {
                        PluginLog.Error(e, e.Message);
                    }
                }
            }

            Handler.Save();

            RefreshEnd:
            if (continuous)
            {
                await Task.Delay(CacheTime, token.Token);
                if (!token.IsCancellationRequested)
                    await RefreshLang();
            }
        }
        
        public static async Task<string> Get(string path)
        {
            return await Data.Get("Localization/" + path);
        }

        public void Dispose()
        {
            token.Cancel();
            loadThread.Join();
            token.Dispose();
        }
    }
}
