using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepDungeonDex
{
    public interface ISaveable
    {
        public void Save(string path);

        public Action<DateTime> Updated { get; set; }
    }

    public interface ILoadable : ISaveable
    {
        public object Load(string path);
    }

    public interface ILoadable<out T> : ILoadable
    {
        public T Load(string path);
    }
}
