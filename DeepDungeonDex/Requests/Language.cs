using System.Threading.Tasks;

namespace DeepDungeonDex;

public partial class Requests
{
    public void ChangeLanguage()
    {
        if (Handler.GetInstance<LocaleKeys>() is not { } locales || Handler.GetInstance("index.json") is not Dictionary<string, string[]> list)
            return;

        var loc = Handler.GetInstance<Configuration>()!.Locale;
        var name = locales.LocaleDictionary.Keys.ToArray()[loc];
        _log.Verbose($"Changed language to {name} processing MobData descriptions");
        foreach (var (_, files) in list)
        {
            foreach (var file in files)
            {
                if (file == "Job.yml")
                    continue;
                var path = file.Replace(".yml", ".dat");
                _log.Verbose($"Loading {path}");
                var mobData = Handler.GetInstance<MobData>(path);
                if (mobData == null)
                    continue;

                _log.Verbose($"Processing MobData descriptions for {file}");
                try
                {
                    _log.Verbose("Loading language file");
                    if (Handler.GetInstance($"{name}/{file}") is not Locale langData)
                        continue;
                    _log.Verbose("Looping through MobData");
                    foreach (var (id, _) in mobData.MobDictionary)
                    {
                        if (!langData.TranslationDictionary.TryGetValue(id.ToString(), out var description))
                            continue;

                        _log.Verbose($"Found description for {id}");
                        mobData.MobDictionary[id].Description = _percentRegex.Replace(description, "%").Replace("\\n", "\n").Split("\n").Select(t => t.Split(' ')).ToArray();
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e, "");
                }

            }
        }
    }

    public async Task<LocaleKeys?> GetLangFileList()
    {
        try
        {
            _log.Verbose("Getting file list");
            var list = await GetLocalization("locales.json");
            return new LocaleKeys { LocaleDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(list)! };
        }
        catch (Exception e)
        {
            _log.Error(e, e.Message);
            return null;
        }
    }

    public async Task RefreshLang(bool continuous = true)
    {
        RequestingLang = true;
        var fileList = await GetLangFileList();
        if (fileList == null)
            goto RefreshEnd;

        Handler.AddJsonStorage("locales.json", fileList);

        _log.Verbose("Getting file list");
        if (Handler.GetInstance("index.json") is not Dictionary<string, string[]> list)
            goto RefreshEnd;

        foreach (var (name, _) in fileList.LocaleDictionary)
        {
            var main = $"{name}/main.yml";
            _log.Verbose("Loading main language file");
            Handler.AddYmlStorage(main, new Locale { TranslationDictionary = StorageHandler.Deserializer.Deserialize<Dictionary<string, string>>(await GetLocalization(main)) });
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
                        _log.Verbose($"Loading {path}");
                        content = await GetLocalization(path);
                        _log.Verbose("Deserializing");
                        Handler.AddYmlStorage(path, new Locale { TranslationDictionary = StorageHandler.Deserializer.Deserialize<Dictionary<string, string>>(content) });
                    }
                    catch (Exception e)
                    {
                        _log.Error(e, "");
                        _log.Debug($"Message: {content}");
                    }
                }
            }
        }

        _log.Verbose("Loading complete saving storage");
        Handler.Save();
        ChangeLanguage();

        RefreshEnd:
        RequestingLang = false;
        if (continuous)
        {
            _log.Verbose($"Refreshing file list in {CacheTime:g}");
            await Task.Delay(CacheTime, _token.Token);
            if (!_token.IsCancellationRequested)
                await RefreshLang();
        }
    }

    public async Task<string> GetLocalization(string path)
    {
        return await Get("Localization/" + path);
    }
}