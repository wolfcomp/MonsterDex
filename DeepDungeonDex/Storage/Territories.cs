namespace DeepDungeonDex.Storage;

public class Territories : ILoad<Territories>
{
    public Dictionary<string, ushort[][]> TerritoryDictionary { get; set; } = new();

    public Dictionary<ushort, string> TerritoryNameDictionary { get; set; } = new();

    public Territories Load(string path)
    {
        TerritoryDictionary = StorageHandler.Deserializer.Deserialize<Dictionary<string, ushort[][]>>(path);
        return this;
    }

    public void Dispose()
    {
        TerritoryDictionary.Clear();
    }

    public string GetTerritoryName(ushort id, IPluginLog log)
    {
        log.Verbose($"Getting territory name for {id}");
        if (TerritoryNameDictionary.TryGetValue(id, out var nameCached))
            return nameCached;
        log.Verbose($"Searching for territory name for {id}");
        foreach (var (name, ids) in TerritoryDictionary)
        {
            foreach (var idPair in ids)
            {
                log.Verbose($"Checking {idPair[0]} - {idPair[1]} with name {name}");
                if (!(idPair[0] <= id && idPair[1] >= id))
                    continue;

                log.Verbose($"Found territory name {name} for {id}");
                TerritoryNameDictionary[id] = name;
                return name;
            }
        }
        log.Verbose($"No territory name found for {id}");
        return "";
    }

    object ILoad.Load(string str) => Load(str);
}
