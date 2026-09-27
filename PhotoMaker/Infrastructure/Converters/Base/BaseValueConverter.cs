using System.Globalization;
using System.Windows.Data;

namespace PhotoMaker.Client.Infrastructure.Converters.Base
{
    internal abstract class BaseValueConverter : IValueConverter
    {
        /// <summary>  Конвертирует значение для отображения  </summary>
        /// <param name="value">  Входное значение  </param>
        /// <param name="targetType">  Целевой тип конвертации  </param>
        /// <param name="parameter">  Параметр конвертера  </param>
        /// <param name="culture">  Специфическая информация о культуре  </param>
        /// <returns>  Сконвертированное значение для отображения  </returns>
        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        /// <summary>  Конвертирует значение для работы с данными  </summary>
        /// <param name="value">  Входное значение  </param>
        /// <param name="targetType">  Целевой тип конвертации  </param>
        /// <param name="parameter">  Параметр конвертера  </param>
        /// <param name="culture">  Специфическая информация о культуре  </param>
        /// <returns> Сконвертированное значение для работы с данными  </returns>
        public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);
    }
}
