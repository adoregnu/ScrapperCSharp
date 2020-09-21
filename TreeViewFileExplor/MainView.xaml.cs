using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TreeViewFileExplor;
using TreeViewFileExplorer.ShellClasses;

namespace TreeViewFileExplorer
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        FileSystemObjectInfo _selectedNode;
        public string CurrentPath
        {
            get { return (string)GetValue(CurrentPathProperty); }
            set { SetValue(CurrentPathProperty, value); }
        }

        public static DependencyProperty CurrentPathProperty =
           DependencyProperty.Register("CurrentPath", typeof(string), typeof(MainView),
               new FrameworkPropertyMetadata(
                   null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                   OnCurrentPathChanged));

        readonly List<FileSystemWatcher> _fsWatchers
            = new List<FileSystemWatcher>();

        public MainView()
        {
            InitializeComponent();
            InitializeFileSystemObjects();
        }

        static void OnCurrentPathChanged(DependencyObject src,
            DependencyPropertyChangedEventArgs e)
        {
            MainView ctrl = src as MainView;
            ctrl.PreSelect((string)e.NewValue);
        }
        #region Events

        private void FileSystemObject_AfterExplore(object sender, EventArgs e)
        {
            Cursor = Cursors.Arrow;
        }

        private void FileSystemObject_BeforeExplore(object sender, EventArgs e)
        {
            Cursor = Cursors.Wait;
        }
#if false
        void FileSystemObject_Delete(object sender, TreeViewEvent e)
        {
            var fsNode = e.FsNode;
            var res = MessageBox.Show($"Delete {fsNode.FileSystemInfo.Name}?",
                "Warning", MessageBoxButton.OKCancel);
            if (res == MessageBoxResult.OK)
            {
                //treeView.Items.Remove(fsInfo);
                //treeView.Items.Remove(fsNode);
                //treeView.Items.Refresh();
                //treeView.UpdateLayout();
            }
        }
#endif
        private void OnTreeItemSelectedChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _selectedNode = (sender as TreeView).SelectedItem as FileSystemObjectInfo;
            CurrentPath = _selectedNode.FileSystemInfo.FullName;
        }

#endregion

#region Methods

        private void InitializeFileSystemObjects()
        {
            var drives = DriveInfo.GetDrives();
            DriveInfo
                .GetDrives()
                .ToList()
                .ForEach(drive =>
                {
                    var fileSystemObject = new FileSystemObjectInfo(drive);
                    fileSystemObject.BeforeExplore += FileSystemObject_BeforeExplore;
                    fileSystemObject.AfterExplore += FileSystemObject_AfterExplore;
                    //fileSystemObject.DeleteEvent += FileSystemObject_Delete;
                    treeView.Items.Add(fileSystemObject);
                });

            foreach (var drive in drives)
            {
                if (drive.DriveType != DriveType.Fixed) continue;
                FileSystemWatcher fsWatcher = new FileSystemWatcher
                {
                    Path = drive.RootDirectory.FullName,
                    Filter = "*.*",
                    NotifyFilter = NotifyFilters.FileName |
                                    NotifyFilters.DirectoryName,
                    IncludeSubdirectories = true
                };
                fsWatcher.Created += new FileSystemEventHandler(OnChanged);
                fsWatcher.Deleted += new FileSystemEventHandler(OnChanged);
                fsWatcher.EnableRaisingEvents = true;
                _fsWatchers.Add(fsWatcher);
            }
        }

        void OnChanged(object sender, FileSystemEventArgs e)
        {
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Deleted:
                case WatcherChangeTypes.Created:
                    break;
                default:
                    return;
            }
            Refresh(e.FullPath);
        }

        void Refresh(string pathComponents)
        {
        }

        private void PreSelect(string path)
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            var driveFileSystemObjectInfo = GetDriveFileSystemObjectInfo(path);
            driveFileSystemObjectInfo.IsExpanded = true;
            PreSelect(driveFileSystemObjectInfo, path);
        }

        private void PreSelect(FileSystemObjectInfo fileSystemObjectInfo,
            string path)
        {
            foreach (var childInfo in fileSystemObjectInfo.Children)
            {
                var isParentPath = IsParentPath(path, childInfo.FileSystemInfo.FullName);
                if (isParentPath)
                {
                    if (string.Equals(childInfo.FileSystemInfo.FullName, path,
                        StringComparison.CurrentCultureIgnoreCase))
                    {
                        /* We found the item for pre-selection */
                    }
                    else
                    {
                        childInfo.IsExpanded = true;
                        PreSelect(childInfo, path);
                    }
                }
            }
        }

#endregion

#region Helpers

        private FileSystemObjectInfo GetDriveFileSystemObjectInfo(string path)
        {
            var directory = new DirectoryInfo(path);
            var drives = DriveInfo.GetDrives();
            var drive = drives.Where(d => d.RootDirectory.FullName.Equals(
                    directory.Root.FullName, StringComparison.CurrentCultureIgnoreCase))
                .FirstOrDefault();
            return GetDriveFileSystemObjectInfo(drive);
        }

        private FileSystemObjectInfo GetDriveFileSystemObjectInfo(DriveInfo drive)
        {
            foreach (var fso in treeView.Items.OfType<FileSystemObjectInfo>())
            {
                if (fso.FileSystemInfo.FullName.Equals(drive.RootDirectory.FullName,
                    StringComparison.CurrentCultureIgnoreCase))
                {
                    return fso;
                }
            }
            return null;
        }

        private bool IsParentPath(string path,
            string targetPath)
        {
            return path.StartsWith(targetPath,
                StringComparison.CurrentCultureIgnoreCase);
        }

#endregion

    }
}
