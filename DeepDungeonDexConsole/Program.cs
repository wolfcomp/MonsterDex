using System.CommandLine;
using System.Reflection;
using YamlDotNet.Core.Events;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace DeepDungeonDexConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var str = new HttpClient().GetStringAsync("https://raw.githubusercontent.com/wolfcomp/DeepDungeonDex/data/Job.yml").Result;
            var dict = new DeserializerBuilder().WithTypeConverter(new YamlStringEnumConverter()).Build().Deserialize<Dictionary<uint, Weakness>>(str);

            foreach (var (_key, _weakness) in dict)
            {
                Console.WriteLine($"{_key} - {_weakness}");
            }
            
            //var input = new Option<FileInfo?>(new[] { "--input", "-i" }, "The file to process.");
            //var names = new Option<FileInfo?>(new[] { "--names", "-n" }, "The names file to use.");
            //var type = new Option<string?>(new[] { "--type", "-t" }, "The type operation to use.");
            //var verbose = new Option<bool>(new[] { "--verbose", "-v" }, "Enable verbose logging.");
            //var rootCommand = new RootCommand("Deep Dungeon Dex Console Tool");
            //rootCommand.AddOption(input);
            //rootCommand.AddOption(names);
            //rootCommand.AddOption(type);
            //rootCommand.AddOption(verbose);
            //rootCommand.SetHandler(RunCommand, input, names, type, verbose);
            //rootCommand.Invoke(args);
        }

        private static void RunCommand(FileInfo? input, FileInfo? names, string? type, bool verbose)
        {
            if (input == null || names == null || type == null || type.Length == 0 || type is not ("id" or "name"))
            {
                Console.WriteLine("You must specify both an input and names file.");
                return;
            }
            Console.WriteLine($"Input: {input.FullName}");
            Console.WriteLine($"Names: {names.FullName}");
            var bnpcsv = File.ReadAllLines(names.FullName);
            var bnpc = bnpcsv
                .Skip(3)
                .Select(line => line.Split(','))
                .ToDictionary(split => int.Parse(split[0]), split => split[1]);
            var lines = File.ReadAllLines(input.FullName);
            switch (type)
            {
                case "id":
                    for (var i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].StartsWith("#")) continue;
                        var line = lines[i];
                        var start = line.Split(':')[0];
                        if (verbose && !start.StartsWith(" ")) Console.WriteLine($"{i}:\n{line}\n{start}");
                        if (start.StartsWith(" ") || !int.TryParse(start.Trim(), out var j)) continue;
                        var name = bnpc[j][1..^1];
                        start += "-" + name + ":" + line.Split(':')[1];
                        if (verbose) Console.WriteLine(start);
                        lines[i] = start;
                    }

                    break;
                case "name":
                    for (var i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].StartsWith("#")) continue;
                        var line = lines[i];
                        var start = line.Split(':')[0];
                        if (verbose && !start.StartsWith(" ")) Console.WriteLine($"{i}:\n{line}\n{start}");
                        if (start.StartsWith(" ") || bnpc.TryGetKey(start.Trim(), out var k) || k == 0) continue;
                        start = k + "-" + start.Trim() + ":";
                        if (verbose) Console.WriteLine(start);
                        lines[i] = start;
                    }
                    break;
            }

            File.WriteAllLines(input.FullName, lines);
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