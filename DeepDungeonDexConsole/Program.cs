using System.IO.Compression;
using System.Text;

namespace DeepDungeonDexConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var program = new Program();
            program.CompressDataFiles();
        }

        public void CompressDataFiles()
        {
            var runDir = AppContext.BaseDirectory;
            while (runDir.Substring(runDir.LastIndexOf(Path.DirectorySeparatorChar) + 1) != "DeepDungeonDexConsole")
            {
                runDir = runDir.Substring(0, runDir.LastIndexOf(Path.DirectorySeparatorChar));
            }
            var dataPath = Path.Combine(runDir, "data");
            var path = Path.Combine(runDir, "..", "DeepDungeonDex", "data.dat");
            if(File.Exists(path))
                File.Delete(path);
            var stream = new FileStream(path, FileMode.Create);
            var archive = new ZipArchive(stream, ZipArchiveMode.Create, true, Encoding.UTF8);
            var files = Directory.GetFiles(dataPath, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var entryName = file.Replace(dataPath, "")[1..];
                if(entryName == "crowdin.yml" || entryName.StartsWith('.'))
                    continue;
                var entry = archive.CreateEntryFromFile(file, entryName, CompressionLevel.Optimal);
                Console.WriteLine($"Compressed {file} to data.dat({entry.FullName})");
            }
            archive.Dispose();
            stream.Dispose();
        }
    }
}