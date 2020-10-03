using FileListView.Interfaces;

namespace Scrapper.ViewModel
{
    interface IFileListNotifier
    {
        void OnDirectoryChanged(string path);
        void OnFileSelected(ILVItemViewModel fsItem);
        void OnCheckboxChanged(ILVItemViewModel fsItem);
        void OnFileDeleted(ILVItemViewModel fsItem);
    }
}
