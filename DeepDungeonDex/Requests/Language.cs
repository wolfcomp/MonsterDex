using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeepDungeonDex.Storage;

namespace DeepDungeonDex.Requests
{
    public class Language
    {
        public Language(StorageHandler handler)
        {
            Task.Factory.StartNew(() => RefreshLang(), TaskCreationOptions.LongRunning);
        }
        
        public async Task RefreshLang(bool continuous = true)
        {

        }
        
        public static async Task<string> Get(string path)
        {
            return await Data.Get("Localization/" + path);
        }
    }
}
