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
    private IPluginLog _log;
    public HttpClient Client = new();
    public const string BaseUrl = "https://raw.githubusercontent.com/wolfcomp/DeepDungeonDex/data";
    public TimeSpan CacheTime = TimeSpan.FromHours(6);
    public StorageHandler Handler;
    public bool RequestingData { get; private set; }
    public bool RequestingLang { get; private set; }
    public bool IsRequesting => RequestingData || RequestingLang;

    public Requests(StorageHandler handler, IPluginLog log)
    {
        Handler = handler;
        _log = log;
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
            _log.Verbose("Getting file list");
            var content = await Get("index.json");
            return JsonConvert.DeserializeObject<Dictionary<string, string[]>>(content);
        }
        catch (Exception e)
        {
            _log.Error(e, "");
            return null;
        }
    }

    public async Task RefreshFileList(bool continuous = true)
    {
        RequestingData = true;
        var list = await GetFileList();
        if (list == null)
            goto RefreshEnd;

        Handler.AddJsonStorage("index.json", list);

        _log.Verbose("Loading Types");
        var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetInterfaces().Contains(typeof(ILoadableString))).ToArray();

        foreach (var (className, files) in list)
        {
            var type = types.FirstOrDefault(t => t.Name == className);
            if (type == null)
                continue;
            _log.Verbose($"Loading Files: {files.Length} of Type: {className}");
            foreach (var file in files)
            {
                try
                {
                    _log.Verbose($"Loading File: {file}");
                    var content = await Get(file);
                    if (string.IsNullOrWhiteSpace(content))
                        continue;
                    _log.Verbose("Creating instance");
                    var instance = (ILoadableString)Activator.CreateInstance(type)!;
                    _log.Verbose("Loading content");
                    Handler.AddYmlStorage(file, instance.Load(content, false));
                }
                catch (Exception e)
                {
                    _log.Error(e, $"Failed processing File: {file}");
                }
            }
        }

        _log.Verbose("Loading complete saving storage");
        Handler.Save();

        RefreshEnd:
        RequestingData = false;
        if (continuous)
        {
            _log.Verbose($"Refreshing file list in {CacheTime:g}");
            await Task.Delay(CacheTime, _token.Token);
            if (!_token.IsCancellationRequested)
                await RefreshFileList();
        }
    }

    public async Task<string> Get(string url)
    {
        _log.Verbose($"Requesting {BaseUrl}/{url}");
        var response = await Client.GetAsync($"{BaseUrl}/{url}");
        _log.Verbose($"Response: {response.StatusCode}");
        if (response.IsSuccessStatusCode)
        {
            _log.Verbose("Reading string");
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
        _log = null!;
    }
}