using PhotoMaker.Model;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PhotoMaker.Client.ViewModel
{
    public class FourierToolVM : Base.BaseViewModel
    {
        #region МОДЕЛЬ

        private FourierTool Model { get; }

        private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not FourierTool) return;

            if (Application.Current.Dispatcher.CheckAccess())
                OnPropertyChanged(e.PropertyName);
            else
                Application.Current.Dispatcher?.Invoke(() =>
                    OnPropertyChanged(e.PropertyName)
                );
        }

        #endregion // МОДЕЛЬ



        #region СВОЙСТА

        #region FourierTransformVisibility : Visibility  -  Видимость Фурье-образа изображения в UI

        private Visibility _fourierTransformVisibility;

        /// <summary>  Видимость Фурье-образа изображения в UI  </summary>
        public Visibility FourierTransformVisibility
        {
            get => _fourierTransformVisibility;
            set => SetValue(_fourierTransformVisibility, value, v => _fourierTransformVisibility = value);
        }

        #endregion // FourierTransformVisibility
        #region IsEnabled : bool  -  Состояние инструмента "Threshold"

        /// <summary>  Состояние инструмента "Threshold"  </summary>
        public bool IsEnabled
        {
            get => Model.IsEnabled;
            set
            {
                if (SetValue(Model.IsEnabled, value, v => Model.IsEnabled = v))
                {
                    if (value == true) FourierTransformVisibility = Visibility.Visible;
                    else FourierTransformVisibility = Visibility.Collapsed;
                }
            }
        }

        #endregion // IsEnabled
        #region FourierTransformImageBitmap : BitmapSource?  -  Битмап Фурье-образа изображения

        /// <summary>  Битмап Фурье-образа изображения  </summary>
        public BitmapSource? FourierTransformImageBitmap
        {
            get => Model.FourierTransformImageBitmap;
            set => SetValue(Model.FourierTransformImageBitmap, value, v => Model.FourierTransformImageBitmap = v);
        }

        #endregion // FourierTransformImageBitmap

        #endregion // СВОЙСТВА



        public FourierToolVM(FourierTool model)
        {
            Model = model;
            Model.PropertyChanged += Model_PropertyChanged;



            FourierTransformVisibility = Model.IsEnabled == true ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
