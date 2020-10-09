﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using MvvmDialogs;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;

using Scrapper.ViewModel.Base;
using System.Linq.Expressions;
using System.IO;
using System.Text.RegularExpressions;

namespace Scrapper.ViewModel
{
    class FileItem : ViewModelBase
    {
        public string SourceName { get; set; }
        public string TargetPath { get; set; }
        public string MediaPath;
    }

    class FileToFolderViewModel : Pane, IModalDialogViewModel
    {
        bool? _dialogResult;
        public bool? DialogResult
        {
            get => _dialogResult;
            private set => Set(nameof(DialogResult), ref _dialogResult, value);
        }

        public ObservableCollection<FileItem> Files { get; set; }

        public ICommand CmdPreview { get; set; }
        public ICommand CmdRename { get; set; }

        public string MediaPath { get; set; }
        public string Pattern { get; set; }
        public FileToFolderViewModel()
        {
            Files = new ObservableCollection<FileItem>();
            /// query current path to MediaViewModel
            MessengerInstance.Send(new NotificationMessageAction<string>(
                "queryCurrentPath", p => { MediaPath = p; }));

            CmdPreview = new RelayCommand(() => OnPreview());
            CmdRename = new RelayCommand(() => OnRename());

            Pattern = @"([a-zA-Z0-9]+)(?:-|00| )?" +
                @"([0-9]{3,8})(?:\D)?([0-9A-Ca-c])?";

            OnPreview();
        }

        string[] _exts = new string[] {
            "mp4", "avi", "mkv", "ts", "wmv", "srt",
            "smi", "sup", "sub", "idx", "m4v"
        };
        void OnPreview()
        {
            Files.Clear();
            var files = Directory.EnumerateFiles(MediaPath);
            Regex reg = new Regex(Pattern);
            foreach (var path in files)
            {
                var file = Path.GetFileName(path);
                if (_exts.Any(s => file.EndsWith(s,
                    StringComparison.CurrentCultureIgnoreCase)))
                {
                    var m = reg.Match(file);
                    if (!m.Success) continue;

                    string folder = $"{m.Groups[1].Value}-{m.Groups[2].Value}";
                    string pid = folder;
                    if (!string.IsNullOrEmpty(m.Groups[3].Value))
                    {
                        pid += $"-{m.Groups[3].Value}";
                    }
                    var ext = Path.GetExtension(file);
                    Files.Add(new FileItem
                    {
                        SourceName = file,
                        TargetPath = $"{folder}\\{pid}".ToUpper() + ext,
                        MediaPath = Path.GetDirectoryName(path)
                    }); ;
                }
            }
        }

        void OnRename()
        {
            List<FileItem> tmp = new List<FileItem>();
            foreach (var item in Files)
            {
                var src = item.MediaPath + "\\" + item.SourceName;
                var dest = item.MediaPath + "\\" + item.TargetPath;
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(dest));
                    File.Move(src, dest);
                    tmp.Add(item);
                    //Log.Print($"move {src} => {dest}");
                }
                catch (Exception ex)
                {
                    Log.Print(ex.Message, ex);
                }
            }
            tmp.ForEach(x => { Files.Remove(x); });
            DialogResult = true;
        }
    }
}
