namespace DeepDungeonDex.Models;

public interface ISaveable : IDisposable
{
    public NamedType? Save(string path);
}

public interface ILoadable : ISaveable
{
    public Storage.Storage Load(string path);
    public Storage.Storage Load(string path, string name);
}

public interface ILoadableString : ILoadable
{
    public Storage.Storage Load(string str, bool fromFile);
}

public class NamedType
{
    public string Name { get; set; } = "";
    public Type Type { get; set; } = typeof(object);
        
    public Tuple<Type, string?> GetTuple() => new(Type, Name);
}