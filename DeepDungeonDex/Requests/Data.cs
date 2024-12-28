using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DeepDungeonDex;

public partial class Requests : IDisposable
{
    private readonly CancellationTokenSource _token = new();
    private readonly Thread _loadFileThread;
    private Thread? _loadLangThread;
    private readonly Regex _percentRegex = new("(%%)|(%)", RegexOptions.Compiled);
    private IPluginLog _log;
    private bool _loadedOnce;
    public HttpClient Client = new();
    public const string BaseUrl = "https://raw.githubusercontent.com/wolfcomp/MonsterDex/data";
    public TimeSpan CacheTime = TimeSpan.FromHours(6);
    public StorageHandler Handler;
    public bool RequestingData { get; private set; }
    public bool RequestingLang { get; private set; }
    public bool IsRequesting => RequestingData || RequestingLang;
    public Action? IsRequestDone = null;
#if RELEASE
    private const bool Debug = false;
#else
    private const bool Debug = true;
#endif

    public Requests(StorageHandler handler, IPluginLog log)
    {
        Handler = handler;
        _log = log;
#pragma warning disable CS4014
        _loadFileThread = new Thread(() => RefreshFileList());
#pragma warning restore CS4014
        _loadFileThread.Start();
    }

    public async Task<Dictionary<string, string[]>?> GetFileList()
    {
        try
        {
            _log.Verbose("Getting file list");
            var content = await Get("index.json");
            return string.IsNullOrWhiteSpace(content) ? null : JsonConvert.DeserializeObject<Dictionary<string, string[]>>(content);
        }
        catch (Exception e)
        {
            _log.Error(e, "");
            return null;
        }
    }

    public async Task RefreshFileList(bool continuous = true)
    {
    StartRequest:
        RequestingData = true;
        var list = await GetFileList();
        if (list == null)
            goto RefreshEnd;

        Handler.AddStorage("index.json", list);

        _log.Verbose("Loading Types");
        var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetInterfaces().Contains(typeof(ILoad))).ToArray();

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
                    _log.Verbose($"Content length from file: {content?.Length}");
                    if (string.IsNullOrWhiteSpace(content))
                        continue;
                    _log.Verbose("Creating instance");
                    var instance = (ILoad)Activator.CreateInstance(type)!;
                    _log.Verbose("Loading content");
                    Handler.AddStorage(file, instance.Load(content));
                }
                catch (Exception e)
                {
                    _log.Error(e, $"Failed processing File: {file}");
                }
            }
        }

        _loadedOnce = true;
        IsRequestDone?.Invoke();

    RefreshEnd:

        if (_loadLangThread == null)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            _loadLangThread = new Thread(() => RefreshLang());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            _loadLangThread.Start();
        }
        RequestingData = false;
        if (continuous)
        {
            _log.Verbose($"Refreshing file list in {CacheTime:g}");
            await Task.Delay(CacheTime, _token.Token);
            if (!_token.IsCancellationRequested)
                goto StartRequest;
        }
    }

    public async Task<string?> Get(string url)
    {
#pragma warning disable CS0162 // Unreachable code detected
        if (Debug)
            return await GetFromFile(url);
        try
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
        catch (Exception e)
        {
            _log.Error(e, "Trying to get file from precompiled data");
            return !_loadedOnce ? await GetFromFile(url) : null;
        }
#pragma warning restore CS0162 // Unreachable code detected
    }

    public void Dispose()
    {
        _token.Cancel();
        _loadFileThread.Join();
        _loadLangThread?.Join();
        _token.Dispose();
        _fileStream?.Dispose();
        _log = null!;
    }
}