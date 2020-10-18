using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrapper.ScrapItems
{
    static class NameMap
    {
        static Dictionary<string, string> _studioMap
            = new Dictionary<string, string>
            {
                { "scute", "S-Cute"},
                { "sodcreate", "SOD Create"},
            };
        public static string StudioName(string name)
        {
            if (_studioMap.ContainsKey(name))
                return _studioMap[name];
            else
                return name;
        }
    }
}
