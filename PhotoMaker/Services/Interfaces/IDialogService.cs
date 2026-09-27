using System.Windows;

namespace PhotoMaker.Client.Services.Interfaces
{
    public interface IDialogService
    {
        /// <summary>  Пути к файлам  </summary>
        public string[] FilePaths { get; }

        /// <summary>  Имена файлов  </summary>
        public string[] FileNames { get; }

        /// <summary>  Открыть файловый диалог для загрузки файла/файлов  </summary>
        /// <returns>  Результат открытия диалога  </returns>
        public bool OpenFileDialog();

        /// <summary>  Открыть файловый диалог для сохранения файла/файлов  </summary>
        /// <returns>  Результат открытия диалога  </returns>
        public bool SaveFileDialog();

        /// <summary>  Открыть диалоговое окно  </summary>
        /// <param name="message">  Сообщение в окне  </param>
        /// <param name="caption">  Заголовок окна  </param>
        /// <param name="button">  Кнопки окна  </param>
        /// <param name="icon"> Иконка окна  </param>
        /// <returns>  Результат открытия диалога  </returns>
        public MessageBoxResult ShowMessage(string message, string caption, MessageBoxButton button, MessageBoxImage icon);
    }
}
