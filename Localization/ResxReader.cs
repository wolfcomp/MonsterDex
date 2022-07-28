using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace DeepDungeonDex.Localization
{
    internal class ResxReader
    {
        private readonly XmlTextReader _reader;
        private readonly Dictionary<string, string> _data = new();

        public ResxReader(string url)
        {
            _reader = new XmlTextReader(url);
        }

        internal void ReadResource()
        {
            while (_reader.Read())
            {
                if (_reader.NodeType != XmlNodeType.Element || _reader.Name != "data") continue;
                var name = _reader.GetAttribute("name")!;
                while (_reader.Read())
                    if (_reader.NodeType == XmlNodeType.Element && _reader.Name == "value")
                        break;
                _reader.Read();
                var value = _reader.Value;
                _data.Add(name, value);
            }
        }

        public string GetString(string name)
        {
            if(_data.Count == 0) ReadResource();
            return _data.TryGetValue(name, out var value) ? value : name;
        }
    }
}
