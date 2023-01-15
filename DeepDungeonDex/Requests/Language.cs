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
            if (Handler.GetInstance<LocaleKeys>() is not { } locales || Handler.GetInstance("index.json") is not Dictionary<string, string[]> list)
                return;

            var loc = Handler.GetInstance<Configuration>()!.Locale;
            var name = locales.LocaleDictionary.Keys.ToArray()[loc];
            PluginLog.Verbose($"Changed language to {name} processing MobData descriptions");
            foreach (var (_, files) in list)
            {
                foreach (var file in files)
                {
                    if (file == "Job.yml")
                        continue;
                    PluginLog.Verbose($"Loading {file}");
                    var data = (Storage.Storage)Handler.GetInstance(file)!;
                    if (data.Value is not MobData mobData)
                        continue;

                    PluginLog.Verbose($"Processing MobData descriptions for {file}");
                    try
                    {
                        PluginLog.Verbose("Loading language file");
                        var langData = (Locale)Handler.GetInstance($"{name}/{file}")!;
                        PluginLog.Verbose("Looping through MobData");
                        foreach (var (id, _) in mobData.MobDictionary)
                        {
                            if (!langData.TranslationDictionary.TryGetValue(id.ToString(), out var description))
                                continue;

                            PluginLog.Verbose($"Found description for {id}");
                            mobData.MobDictionary[id].Description = percentRegex.Replace(description, "%%");
                        }
                    }
                    catch (Exception e)
                    {
                        PluginLog.Error(e, "");
                    }
                }
            }
        }

        public async Task<LocaleKeys?> GetFileList()
        {
            try
            {
                PluginLog.Verbose("Getting file list");
                var list = await Get("locales.json");
                return new LocaleKeys { LocaleDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(list)! };
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

            PluginLog.Verbose("Getting file list");
            if (Handler.GetInstance("index.json") is not Dictionary<string, string[]> list)
                goto RefreshEnd;

            foreach (var (name, _) in fileList.LocaleDictionary)
            {
                var main = $"{name}/main.yml";
                PluginLog.Verbose("Loading main language file");
                Handler.AddYmlStorage(main, new Locale { TranslationDictionary = StorageHandler.Deserializer.Deserialize<Dictionary<string, string>>(await Get(main)) });
                foreach (var (_, files) in list)
                {
                    foreach (var file in files)
                    {
                        if (file == "Job.yml")
                            continue;
                        var content = "";
                        try
                        {
                            var path = $"{name}/{file}";
                            PluginLog.Verbose($"Loading {path}");
                            content = await Get(path);
                            PluginLog.Verbose("Deserializing");
                            Handler.AddYmlStorage(path, new Locale { TranslationDictionary = StorageHandler.Deserializer.Deserialize<Dictionary<string, string>>(content) });
                        }
                        catch (Exception e)
                        {
                            PluginLog.Error(e, "");
                            PluginLog.Debug($"Message: {content}");
                        }
                    }
                }
            }

            PluginLog.Verbose("Loading complete saving storage");
            Handler.Save();
            ChangeLanguage();

        RefreshEnd:
            if (continuous)
            {
                PluginLog.Verbose($"Refreshing file list in {CacheTime:g}");
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
