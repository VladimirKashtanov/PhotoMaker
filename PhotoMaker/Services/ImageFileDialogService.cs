using Microsoft.Win32;
using System.Windows;

namespace PhotoMaker.Client.Services
{
    public class ImageFileDialogService : Interfaces.IDialogService
    {
        public string[] FilePaths { get; set; } = Array.Empty<string>();

        public string[] FileNames { get; set; } = Array.Empty<string>();

        public bool OpenFileDialog()
        {
            var openFileDialog = new OpenFileDialog()
            {
                Filter = "Изображения (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                FilePaths = openFileDialog.FileNames;
                FileNames = openFileDialog.SafeFileNames;
                return true;
            }
            return false;
        }

        public bool SaveFileDialog()
        {
            var saveFileDialog = new SaveFileDialog()
            {
                Filter = "PNG изображение (*.png)|*.png|JPEG изображение (*.jpg;*.jpeg)|*.jpg;*.jpeg",
                DefaultExt = "png",
                AddExtension = true
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                FilePaths[0] = saveFileDialog.FileName;
                return true;
            }
            return false;
        }

        public MessageBoxResult ShowMessage(string message, string caption, MessageBoxButton button, MessageBoxImage icon) =>
            MessageBox.Show(message, caption, button, icon);


        public ImageFileDialogService() { }
    }
}
