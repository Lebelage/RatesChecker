using Microsoft.Win32;
using System;

namespace RatesChecker.Services.FileDialog
{
    internal class FileDialogService : IFileDialogService
    {
        #region Singleton - обход DI
        private static readonly Lazy<IFileDialogService> _instance =
            new Lazy<IFileDialogService>(() => new FileDialogService());

        public static IFileDialogService Instance => _instance.Value;

        private FileDialogService() { }
        #endregion

        public string OpenFile(string filter = "All files (*.*)|*.*", string defaultExt = "")
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = filter,
                DefaultExt = defaultExt,
                CheckFileExists = true 
            };

            
            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }

            return null; 
        }

        public string SaveFile(string filter = "All files (*.*)|*.*", string defaultExt = "")
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = filter,
                DefaultExt = defaultExt,
                OverwritePrompt = true 
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                return saveFileDialog.FileName;
            }

            return null;
        }

    }
}
