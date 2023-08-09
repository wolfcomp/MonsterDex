using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DeepDungeonDex;

public partial class Requests : IDisposable
{
    private readonly CancellationTokenSource _token = new();
    private readonly Thread _loadFileThread;
    private readonly Thread _loadLangThread;
    private readonly Regex _percentRegex = new("(%%)|(%)", RegexOptions.Compiled);
    public HttpClient Client = new();
    public const string BaseUrl = "https://raw.githubusercontent.com/wolfcomp/DeepDungeonDex/data";
    public TimeSpan CacheTime = TimeSpan.FromHours(6);
    public StorageHandler Handler;

    public Requests(StorageHandler handler)
    {
        Handler = handler;
        handler.AddJsonStorage("index.json", GetFileList().Result!);
#pragma warning disable CS4014
        _loadFileThread = new Thread(() => RefreshFileList());
        _loadLangThread = new Thread(() => RefreshLang());
#pragma warning restore CS4014
        _loadFileThread.Start();
        _loadLangThread.Start();
    }

    public async Task<Dictionary<string, string[]>?> GetFileList()
    {
        try
        {
            PluginLog.Verbose("Getting file list");
            var content = await Get("index.json");
            return JsonConvert.DeserializeObject<Dictionary<string, string[]>>(content);
        }
        catch (Exception e)
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

        PluginLog.Verbose("Loading Types");
        var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetInterfaces().Contains(typeof(ILoadableString))).ToArray();

        foreach (var (className, files) in list)
        {
            var type = types.FirstOrDefault(t => t.Name == className);
            if (type == null)
                continue;
            PluginLog.Verbose($"Loading Files: {files.Length} of Type: {className}");
            foreach (var file in files)
            {
                try
                {
                    PluginLog.Verbose($"Loading File: {file}");
                    var content = await Get(file);
                    if (string.IsNullOrWhiteSpace(content))
                        continue;
                    PluginLog.Verbose("Creating instance");
                    var instance = (ILoadableString)Activator.CreateInstance(type)!;
                    PluginLog.Verbose("Loading content");
                    Handler.AddYmlStorage(file, instance.Load(content, false));
                }
                catch (Exception e)
                {
                    PluginLog.Error(e, $"Failed processing File: {file}");
                }
            }
        }

        PluginLog.Verbose("Loading complete saving storage");
        Handler.Save();

        RefreshEnd:
        if (continuous)
        {
            PluginLog.Verbose($"Refreshing file list in {CacheTime:g}");
            await Task.Delay(CacheTime, _token.Token);
            if (!_token.IsCancellationRequested)
                await RefreshFileList();
        }
    }

    public async Task<string> Get(string url)
    {
        PluginLog.Verbose($"Requesting {BaseUrl}/{url}");
        var response = await Client.GetAsync($"{BaseUrl}/{url}");
        PluginLog.Verbose($"Response: {response.StatusCode}");
        if (response.IsSuccessStatusCode)
        {
            PluginLog.Verbose("Reading string");
            return await response.Content.ReadAsStringAsync();
        }
        throw new HttpRequestException($"Request: {url}\nFailed with status code {response.StatusCode}");
    }

    public void Dispose()
    {
        _token.Cancel();
        _loadFileThread.Join();
        _loadLangThread.Join();
        _token.Dispose();
    }
}