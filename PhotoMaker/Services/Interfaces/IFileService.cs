namespace PhotoMaker.Client.Services.Interfaces
{
    public interface IFileService
    {
        /// <summary>  Открыть файл  </summary>
        /// <param name="filepath">  Путь к файлу  </param>
        /// <returns>  Возвращает содержимое файла либо null  </returns>
        object? Open(string filepath);

        /// <summary>  Сохраняет информацию в файл  </summary>
        /// <param name="filepath">  Путь к файлу  </param>
        /// <param name="data">  Информация для сохранения в файл  </param>
        void Save(string filepath, object data);
    }
}
