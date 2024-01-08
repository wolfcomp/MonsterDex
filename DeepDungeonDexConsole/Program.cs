using System.CommandLine;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using YamlDotNet.Core.Events;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

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
            var dataPath = @"D:\source\repos\DeepDungeonDexData";
            var path = @"D:\source\repos\DeepDungeonDex\DeepDungeonDex\data.dat";
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

    public static class Extensions
    {
        public static bool TryGetKey(this Dictionary<int, string> dict, string value, out int key)
        {
            var k = dict.Where(pair => string.Equals(value, pair.Value, StringComparison.InvariantCultureIgnoreCase)).ToArray();
            if (k.Any())
            {
                key = k.First().Key;
                return true;
            }

            key = default;
            return false;
        }
    }

    internal class YamlStringEnumConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => type.IsEnum;

        public object ReadYaml(IParser parser, Type type)
        {
            var items = new List<string>();
            if (type.GetCustomAttributes<FlagsAttribute>().Any())
            {
                parser.TryConsume<SequenceStart>(out _);
                while (parser.TryConsume<Scalar>(out var scalar))
                {
                    items.Add(scalar.Value);
                }
                parser.TryConsume<SequenceEnd>(out _);
            }
            else if (parser.TryConsume<Scalar>(out var scalar))
                items.Add(scalar.Value);
            return Enum.Parse(type, string.Join(", ", items));
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            if (value == null) return;
            if (type.GetCustomAttributes<FlagsAttribute>().Any())
            {
                var str = value.ToString()!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                emitter.Emit(new SequenceStart(default, default, false, SequenceStyle.Any));
                foreach (var s in str)
                {
                    emitter.Emit(new Scalar(s));
                }
                emitter.Emit(new SequenceEnd());
            }
            else
            {
                emitter.Emit(new Scalar(value.ToString()!));
            }
        }
    }

    [Flags]
    public enum Weakness
    {
        None = 0x00,
        Stun = 0x01,
        Heavy = 0x02,
        Slow = 0x04,
        Sleep = 0x08,
        Bind = 0x10,
        Undead = 0x20
    }
}