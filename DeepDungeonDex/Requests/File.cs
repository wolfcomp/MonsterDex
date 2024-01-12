using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace DeepDungeonDex;

public partial class Requests
{
    private ZipArchive? _fileStream;

    public async Task<string?> GetFromFile(string path)
    {
        if (_fileStream == null)
            GetDat("DeepDungeonDex.data.dat");

        try
        {
            path = path.Replace("/", "\\");
            var entry = _fileStream!.Entries.FirstOrDefault(e => e.FullName == path);
            if (entry == null)
                return null;
            await using var stream = entry.Open();
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
        catch (Exception e)
        {
            _log.Error(e, $"Error while trying to read: {path}");
            return null;
        }
    }

    public async Task<Dictionary<string, string[]>?> GetFileListFromFile()
    {
        var content = await GetFromFile("index.json");
        return content == null ? null : JsonConvert.DeserializeObject<Dictionary<string, string[]>>(content);
    }

    public void GetDat(string path)
    {
        var compStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path)!;
        var stream = new ZipArchive(compStream, ZipArchiveMode.Read);
        _fileStream = stream;
    }
}