namespace DeepDungeonDex.Models;

public interface ILoad : IDisposable
{
    public object Load(string str);
}

public interface ILoad<out T> : ILoad where T : class
{
    public new T Load(string str);
}

public class NamedType
{
    public string Name { get; set; } = "";
    public Type Type { get; set; } = typeof(object);
        
    public Tuple<Type, string?> GetTuple() => new(Type, Name);
}