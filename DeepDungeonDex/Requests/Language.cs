using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Logging;
using DeepDungeonDex.Models;
using DeepDungeonDex.Storage;
using Newtonsoft.Json;

namespace DeepDungeonDex.Requests
{
    public class Language : IDisposable
    {
        private readonly CancellationTokenSource token = new();
        private readonly Thread loadThread;
        private readonly Regex percentRegex = new("(%%)|(%)", RegexOptions.Compiled);
        public static TimeSpan CacheTime = TimeSpan.FromHours(6);
        public StorageHandler Handler;

        public Language(StorageHandler handler)
        {
            Handler = handler;
            loadThread = new Thread(() => RefreshLang());
            loadThread.Start();
        }

        public void ChangeLanguage()
        {
            var loc = Handler.GetInstance<Configuration>()!.Locale;
            if (Handler.GetInstance<LocaleKeys>() is not { } locales || Handler.GetInstance("index.json") is not Dictionary<string, string[]> list)
                return;

            var name = locales.LocaleDictionary.Keys.ToArray()[loc];
            foreach (var (_, files) in list)
            {
                foreach (var file in files)
                {
                    if(file == "Job.yml")
                        continue;
                    var data = (Storage.Storage)Handler.GetInstance(file)!;
                    var langData = (Locale)Handler.GetInstance($"{name}/{file}")!;
                    foreach (var (id, _) in (data.Value as MobData).MobDictionary)
                    {
                        if (langData.TranslationDictionary.TryGetValue(id.ToString(), out var description))
                        {
                            (data.Value as MobData).MobDictionary[id].Description = percentRegex.Replace(description, "%%");
                        }
                    }
                }
            }
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

            if(Handler.GetInstance("index.json") is not Dictionary<string, string[]> list)
                goto RefreshEnd;

            foreach (var (name, folders) in fileList.LocaleDictionary)
            {
                var main = $"{name}/main.yml";
                Handler.AddYmlStorage(main, new Locale { TranslationDictionary = StorageHandler.Deserializer.Deserialize<Dictionary<string, string>>(await Get(main))});
                foreach (var (_, files) in list)
                {
                    foreach(var file in files)
                    {
                        if(file == "Job.yml")
                            continue;
                        try
                        {
                            var path = $"{name}/{file}";
                            var content = await Get(path);
                            Handler.AddYmlStorage(path, new Locale { TranslationDictionary = StorageHandler.Deserializer.Deserialize<Dictionary<string, string>>(content) });
                        }
                        catch (Exception e)
                        {
                            PluginLog.Error(e, "");
                        }
                    }
                }
            }

            Handler.Save();
            ChangeLanguage();

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
