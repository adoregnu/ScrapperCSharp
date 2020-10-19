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
                { "Tameike Goro-", "Tameike Goro" }
            };
        public static string StudioName(string name)
        {
            if (_studioMap.ContainsKey(name))
                return _studioMap[name];
            else
                return name;
        }

        static Dictionary<string, string> _actorMap
            = new Dictionary<string, string>
            {
                {"Oohashi Miku", "Ohashi Miku" }
            };

        public static string ActorName(string name)
        {
            if (_actorMap.ContainsKey(name))
                return _actorMap[name];
            else
                return name;
        }
    }
}
