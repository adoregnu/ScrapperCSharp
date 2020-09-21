using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeViewFileExplorer.ShellClasses;

namespace TreeViewFileExplor
{
    public class TreeViewEvent : EventArgs
    {
        public FileSystemObjectInfo FsNode { get; set; }
    }
}
