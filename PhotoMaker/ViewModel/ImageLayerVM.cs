using PhotoMaker.Model;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PhotoMaker.Client.ViewModel
{
    public class ImageLayerVM : Base.BaseViewModel
    {
        #region МОДЕЛЬ

        public Model.ImageLayer Model { get; }

        private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not ImageLayer) return;

            if (Application.Current.Dispatcher.CheckAccess())
                OnPropertyChanged(e.PropertyName);
            else
                Application.Current.Dispatcher?.Invoke(() =>
                    OnPropertyChanged(e.PropertyName)
                );
        }

        #endregion // МОДЕЛЬ


        #region СВОЙСТВА

        #region Name : string  -  Имя слоя

        /// <summary>  Имя слоя  </summary>
        public string Name => Model.Name;

        #endregion // Name
        #region Bitmap : BitmapSource  -  Битмап изображения слоя

        /// <summary>  Битмап изображения слоя  </summary>
        public BitmapSource Bitmap => Model.Bitmap;

        #endregion // Bitmap
        #region ImageSizeString : string  -  Строка размеров изображения

        /// <summary>  Строка размеров изображения  </summary>
        public string ImageSizeString => Model.ImageSizeString;

        #endregion // ImageSizeString
        #region ImageDpiString : string  -  Строка DPI изображения

        /// <summary>  Строка DPI изображения  </summary>
        public string ImageDpiString => Model.ImageDpiString;

        #endregion // ImageDpiString
        #region ChannelB : bool  -  Синий канал изображения

        /// <summary>  Синий канал изображения  </summary>
        public bool ChannelB
        {
            get => Model.ChannelB;
            set => SetValue(Model.ChannelB, value, v => Model.ChannelB = value);
        }

        #endregion // ChannelB
        #region ChannelG : bool  -  Зеленый канал изображения

        /// <summary>  Зеленый канал изображения  </summary>
        public bool ChannelG
        {
            get => Model.ChannelG;
            set => SetValue(Model.ChannelG, value, v => Model.ChannelG = value);
        }

        #endregion // ChannelG
        #region ChannelR : bool  -  Красный канал изображения

        /// <summary>  Красный канал изображения  </summary>
        public bool ChannelR
        {
            get => Model.ChannelR;
            set => SetValue(Model.ChannelR, value, v => Model.ChannelR = value);
        }

        #endregion // ChannelR
        #region Opacity : byte  -  Прозрачность изображения

        /// <summary>  Прозрачность изображения  </summary>
        public byte Opacity
        {
            get => Model.Opacity;
            set => SetValue(Model.Opacity, value, v => Model.Opacity = v);
        }

        #endregion // Opacity
        #region OffsetX : int  -  Смещение изображения слоя по ширине

        /// <summary>  Смещение изображения слоя по ширине  </summary>
        public int OffsetX
        {
            get => Model.OffsetX;
            set => SetValue(Model.OffsetX, value, v => Model.OffsetX = v);
        }

        #endregion // OffsetX
        #region OffsetY : int  -  Смещение изображения слоя по высоте

        /// <summary>  Смещение изображения слоя по высоте  </summary>
        public int OffsetY
        {
            get => Model.OffsetY;
            set => SetValue(Model.OffsetY, value, v => Model.OffsetY = v);
        }

        #endregion // OffsetY
        #region ActiveBlendMode : ImageLayerBlendModes.IBlendMode  -  Активный режим наложения слоя

        /// <summary>  Активный режим наложения слоя  </summary>
        public ImageLayerBlendModes.IBlendMode ActiveBlendMode
        {
            get => Model.ActiveBlendMode;
            set => SetValue(Model.ActiveBlendMode, value, v => Model.ActiveBlendMode = v);
        }

        #endregion // ActiveBlendMode
        #region BlendModes : List  -  Доступные режими наложения слоя

        /// <summary>  Доступные режими наложения слоя  </summary>
        public List<ImageLayerBlendModes.IBlendMode> BlendModes => Model.BlendModes;

        #endregion // BlendModes

        #endregion // СВОЙСТВА


        public ImageLayerVM(ImageLayer model)
        {
            Model = model;
            Model.PropertyChanged += Model_PropertyChanged;
        }
    }
}
