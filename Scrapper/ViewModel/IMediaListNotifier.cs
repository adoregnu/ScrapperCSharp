using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Scrapper.Model;
namespace Scrapper.ViewModel
{
    interface IMediaListNotifier
    {
        void OnMediaItemMoved(string path);
    }
}
