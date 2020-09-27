using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileListView.Interfaces;

namespace Scrapper.ViewModel
{
    interface IFileListNotifier
    {
        void OnDirectoryChanged(string path);
        void OnFileSelected(string path);
        void OnCheckboxChanged(ILVItemViewModel fsItem);
        void OnFileDeleted(ILVItemViewModel fsItem);
    }
}
