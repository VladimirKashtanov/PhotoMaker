using System.Windows.Media.Imaging;

namespace PhotoMaker.Model
{
    public class ImageLayer : Base.NotifyModel
    {
        #region СВОЙСТВА

        #region Name : string  -  Имя слоя

        /// <summary>  Имя слоя  </summary>
        public string Name { get; private set; }

        #endregion // Name
        #region Bitmap : BitmapSource  -  Битмап изображения слоя

        private BitmapSource _bitmap;

        /// <summary>  Битмап изображения слоя  </summary>
        public BitmapSource Bitmap
        {
            get => _bitmap;
            private set => SetValue(ref _bitmap, value);
        }

        #endregion // Bitmap
        #region ImageSizeString : string  -  Строка размеров изображения

        /// <summary>  Строка размеров изображения  </summary>
        public string ImageSizeString { get; private set; }

        /// <summary>  Строит строку размеров изображения  </summary>
        /// <param name="width">  Ширина изображения  </param>
        /// <param name="height">  Высота изображения  </param>
        /// <returns>  Строка размеров изображения  </returns>
        private static string BuildImageSizeString(int width = 0, int height = 0) =>
            $"{width} x {height}";

        #endregion // ImageSizeString
        #region ImageDpiString : string  -  Строка DPI изображения

        /// <summary>  Строка DPI изображения  </summary>
        public string ImageDpiString { get; private set; }

        /// <summary>  Строит строку DPI изображения  </summary>
        /// <param name="xDpi">  DPI изображения по ширине  </param>
        /// <param name="yDpi">  DPI изображения по длине  </param>
        /// <returns>  Строка DPI изображения  </returns>
        private static string BuildImageDPIString(double xDpi = 0, double yDpi = 0) =>
            $"{(int)xDpi} x {(int)yDpi}";

        #endregion // ImageDPIString
        #region ChannelB : bool  -  Синий канал изображения

        private bool _channelB = true;

        /// <summary>  Синий канал изображения  </summary>
        public bool ChannelB
        {
            get => _channelB;
            set => SetValue(ref _channelB, value);
        }

        #endregion // ChannelB
        #region ChannelG : bool  -  Синий канал изображения

        private bool _channelG = true;

        /// <summary>  Зеленый канал изображения  </summary>
        public bool ChannelG
        {
            get => _channelG;
            set => SetValue(ref _channelG, value);
        }

        #endregion // ChannelG
        #region ChannelR : bool  -  Синий канал изображения

        private bool _channelR = true;

        /// <summary>  Красный канал изображения  </summary>
        public bool ChannelR
        {
            get => _channelR;
            set => SetValue(ref _channelR, value);
        }

        #endregion // ChannelR
        #region Opacity : byte  -  Прозрачность изображения

        private byte _opacity = 255;

        /// <summary>  Прозрачность изображения  </summary>
        public byte Opacity
        {
            get => _opacity;
            set => SetValue(ref _opacity, value);
        }

        #endregion // Opacity
        #region OffsetX : int  -  Смещение изображения слоя по ширине

        private int _offsetX = 0;

        /// <summary>  Смещение изображения слоя по ширине  </summary>
        public int OffsetX
        {
            get => _offsetX;
            set => SetValue(ref _offsetX, value);
        }

        #endregion // OffsetX
        #region OffsetY : int  -  Смещение изображения слоя по высоте

        private int _offsetY = 0;

        /// <summary>  Смещение изображения слоя по высоте  </summary>
        public int OffsetY
        {
            get => _offsetY;
            set => SetValue(ref _offsetY, value);
        }

        #endregion // OffsetY
        #region ActiveBlendMode : ImageLayerBlendModes.IBlendMode  -  Активный режим наложения слоя

        private ImageLayerBlendModes.IBlendMode _activeBlendMode = ImageLayerBlendModes.Modes[0];

        /// <summary>  Активный режим наложения слоя  </summary>
        public ImageLayerBlendModes.IBlendMode ActiveBlendMode
        {
            get => _activeBlendMode;
            set => SetValue(ref _activeBlendMode, value);
        }

        #endregion // ActiveBlendMode
        #region BlendModes : List  -  Доступные режими наложения слоя

        /// <summary>  Доступные режими наложения слоя  </summary>
        public List<ImageLayerBlendModes.IBlendMode> BlendModes => ImageLayerBlendModes.Modes.ToList();

        #endregion // BlendModes

        #endregion // СВОЙСТВА


        public ImageLayer(BitmapSource bitmap, string filename)
        {
            Name = filename;
            _bitmap = bitmap;
            ImageSizeString = BuildImageSizeString(_bitmap.PixelWidth, _bitmap.PixelHeight);
            ImageDpiString = BuildImageDPIString(_bitmap.DpiX, _bitmap.DpiY);
        }
    }
}
