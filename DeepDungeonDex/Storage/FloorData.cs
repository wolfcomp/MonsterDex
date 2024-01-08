namespace DeepDungeonDex.Storage;

internal class FloorData : ILoad<FloorData>
{
    public Dictionary<byte, byte> FloorDictionary { get; set; } = new();

    public FloorData Load(string path)
    {
        FloorDictionary = StorageHandler.Deserializer.Deserialize<Dictionary<byte, byte>>(path);
        return this;
    }

    public void Dispose()
    {
        FloorDictionary.Clear();
    }
    object ILoad.Load(string str) => Load(str);
}