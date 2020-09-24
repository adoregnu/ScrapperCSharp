using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrapper.ViewModel
{
    interface IFileListNotifier
    {
        void OnDirectoryChanged(string path);
    }
}
