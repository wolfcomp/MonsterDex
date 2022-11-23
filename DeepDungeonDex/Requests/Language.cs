using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Logging;
using DeepDungeonDex.Storage;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;

namespace DeepDungeonDex.Requests
{
    public class Language
    {
        private Dictionary<string, string> _language = new();
        public static TimeSpan CacheTime = TimeSpan.FromHours(6);
        public StorageHandler Handler;

        public Language(StorageHandler handler)
        {
            Handler = handler;
            Task.Factory.StartNew(() => RefreshLang(), TaskCreationOptions.LongRunning);
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

            if(Handler.GetInstance("index.json") is not string[] list)
                goto RefreshEnd;

            foreach (var (name, folders) in fileList.LocaleDictionary)
            {
                foreach (var file in list)
                {
                    try
                    {
                        var content = await Get($"{folders}/{file}");
                        Handler.AddYmlStorage(file, new Locale().Load(content, name));
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
                await Task.Delay(CacheTime);
                await RefreshLang();
            }
        }
        
        public static async Task<string> Get(string path)
        {
            return await Data.Get("Localization/" + path);
        }
    }
}
