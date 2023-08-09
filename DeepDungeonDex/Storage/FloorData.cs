namespace DeepDungeonDex.Storage;

internal class FloorData : ILoadableString
{
    public Dictionary<byte, byte> FloorDictionary { get; set; } = new();

    public NamedType? Save(string path)
    {
        StorageHandler.SerializeYamlFile(path, FloorDictionary);
        return null;
    }

    public Storage Load(string path)
    {
        FloorDictionary = StorageHandler.Deserializer.Deserialize<Dictionary<byte, byte>>(StorageHandler.ReadFile(path));
        return new Storage(this);
    }

    public Storage Load(string path, string name)
    {
        FloorDictionary = StorageHandler.Deserializer.Deserialize<Dictionary<byte, byte>>(StorageHandler.ReadFile(path));
        return new Storage(this)
        {
            Name = name
        };
    }

    public Storage Load(string str, bool fromFile)
    {
        if (fromFile)
            return Load(str);
        FloorDictionary = StorageHandler.Deserializer.Deserialize<Dictionary<byte, byte>>(str);
        return new Storage(this);
    }
}