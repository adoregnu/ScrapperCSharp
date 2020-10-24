using FFmpeg.AutoGen;
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
                { "Emmanuelle", "Emmanuelle"},
                { "h.m.p", "h.m.p"},
                { "Daydream", "Mousouzoku"},
                { "Daydreamers", "Mousouzoku"},
                { "Mousouzoku", "Mousouzoku"},
                { "scute", "S-Cute"},
                { "sodcreate", "SOD Create"},
                { "Tameike Goro-", "Tameike Goro" },
                { "Das !", "Das"},
                { "Bi", "Chijo Heaven"},
            };
        public static string StudioName(string name)
        {
            if (_studioMap.Any(i => name.Contains(i.Key)))
            {
                var found = _studioMap.First(i => name.Contains(i.Key));
                return found.Value;
            }
            else
                return name;
        }

        static Dictionary<string, string> _actorMap
            = new Dictionary<string, string>
            {
                { "Oohashi Miku", "Ohashi Miku" },
                { "Mariya Nagai", "Maria Nagai" },
                { "Yui Ooba", "Yui Oba" },
                { "Yuu Shinoda", "Yu Shinoda" },
                { "Hibiki Ootsuki", "Hibiki Otsuki"}
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
