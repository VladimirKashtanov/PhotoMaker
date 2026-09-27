using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhotoMaker.Model
{
    public class FiltersTool : Base.NotifyModel
    {
        #region СВОЙСТВА

        #region IsFiltersToolOn : bool  -  Состояние инструмента "Filters"

        private bool _isFiltersToolOn = false;

        /// <summary>  Состояние инструмента "Filters"  </summary>
        public bool IsFiltersToolOn
        {
            get => _isFiltersToolOn;
            set
            {
                if(SetValue(ref _isFiltersToolOn, value))
                    if (value == false) ResultBitmap = null;
            }
        }

        #endregion // IsFiltersToolOn
        #region ActiveFilter : ImageFilter.IFilter  -  Активный фильтр

        private ImageFilter.IFilter _activeFilter = ImageFilter.Modes[0];

        /// <summary>  Активный фильтр  </summary>
        public ImageFilter.IFilter ActiveFilter
        {
            get => _activeFilter;
            set => SetValue(ref _activeFilter, value);
        }

        #endregion // ActiveFilter
        #region FilterModes : List  -  Режимы фильтрации

        /// <summary>  Режимы фильтрации  </summary>
        public List<ImageFilter.IFilter> FilterModes { get; } = ImageFilter.Modes.ToList();

        #endregion // FilterModes

        #region Kernel : floa[,]?  -  Ядро фильтра

        private float[,]? _kernel;

        /// <summary>  Ядро фильтра  </summary>
        public float[,]? Kernel
        {
            get => _kernel;
            set 
            {
                if (SetValue(ref _kernel, value) && _kernel is not null)
                    Brightness = (float)Math.Round(_kernel.Cast<float>().Sum(), 5, MidpointRounding.AwayFromZero);
            }
        }

        #endregion // Kernel
        #region RadiusX : int  -  Радиус ядра фильтра по X

        private int _radiusX = 1;

        /// <summary>  Радиус ядра фильтра по X  </summary>
        public int RadiusX
        {
            get => _radiusX;
            set => SetValue(ref _radiusX, value);
        }

        #endregion // RadiusX
        #region RadiusY : int  -  Радиус ядра фильтра по Y

        private int _radiusY = 1;

        /// <summary>  Радиус ядра фильтра по Y  </summary>
        public int RadiusY
        {
            get => _radiusY;
            set => SetValue(ref _radiusY, value);
        }

        #endregion // RadiusY
        #region Brightness : float  -  Сумма элементов ядра фильтра (яркость изображения)

        private float _brightness = 0;

        /// <summary>  Сумма элементов ядра фильтра  </summary>
        public float Brightness
        {
            get => _brightness;
            set => SetValue(ref _brightness, value);
        }

        #endregion // Brightness

        #region Sigma : float  -  Степень рассеивания размытия Гаусса

        private float _sigma = 1;

        /// <summary>  Степень рассеивания размытия Гаусса  </summary>
        public float Sigma
        {
            get => _sigma;
            set => SetValue(ref _sigma, value);
        }

        #endregion // Sigma

        #region InnerBitmap : BitmapSource  -  Битмап входного изображения

        /// <summary>  Битмап входного изображения  </summary>
        public BitmapSource InnerBitmap { get; private set; }

        #endregion // InnerBitmap
        #region ResultBitmap : BitmapSource  -  Битмап обработанного изображения

        public BitmapSource? _resultBitmap = null;

        /// <summary>  Битмап обработанного изображения  </summary>
        public BitmapSource? ResultBitmap
        {
            get => _resultBitmap;
            private set => SetValue(ref _resultBitmap, value);
        }

        #endregion // ResultBitmap
        #region ImageProcessingTime : int  -  Время обработки изображения

        private int _imageProcessingTime;

        /// <summary>  Время обработки изображения  </summary>
        public int ImageProcessingTime
        {
            get => _imageProcessingTime;
            private set => SetValue(ref _imageProcessingTime, value);
        }

        #endregion // ImageProcessingTime
        
        #endregion // СВОЙСТВА



        #region ПОЛЯ

        #region _imageFiltrationCts : CancellationTokenSource?  -  Токен отмены операции фильтрации изображения

        /// <summary>  Токен отмены операции фильтрации изображения  </summary>
        private CancellationTokenSource? _imageFiltrationCts;

        #endregion // _imageFiltrationCts
        #region _imageFiltrationLock : object?  -  Замок операции фильтрации изображения

        /// <summary>  Замок операции фильтрации изображения  </summary>
        private readonly object _imageFiltrationLock = new();

        #endregion // _imageFiltrationLock

        #endregion // ПОЛЯ



        #region МЕТОДЫ

        #region ImageFiltration() : void  -  Фильтровать изображение

        /// <summary>  Фильтровать изображение  </summary>
        /// <param name="bitmap">  Битмап исходного изображения  </param>
        private void ImageFiltration(BitmapSource? bitmap)
        {
            if (bitmap != null) InnerBitmap = bitmap;
            if (Kernel == null) return;

            var result = ActiveFilter.FilterBitmap(InnerBitmap, Kernel);

            ResultBitmap = result;
        }

        #endregion // ImageFiltration()
        #region ImageFiltrationAsyncDebounced() : void  -  Выполнить асинхронную фильтрацию изображения с отменой

        /// <summary>  Выполнить асинхронную фильтрацию изображения с отменой  </summary>
        /// <param name="bitmap">  Битмап исходного изображения  </param>
        /// <param name="onCompleted">  Действия, производимые после выполнения метода  </param>
        public void ImageFiltrationAsyncDebounced(BitmapSource bitmap, Action? onCompleted = null)
        {
            if (!IsFiltersToolOn) 
            {
                onCompleted?.Invoke();
                return;
            }

            lock (_imageFiltrationLock)
            {
                _imageFiltrationCts?.Cancel();
                _imageFiltrationCts = new CancellationTokenSource();
                var token = _imageFiltrationCts.Token;

                Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(50, token);
                        if (token.IsCancellationRequested) return;

                        var sw = Stopwatch.StartNew();

                        await Task.Run(() => ImageFiltration(bitmap));

                        sw.Stop();
                        ImageProcessingTime = (int)sw.ElapsedMilliseconds;

                        if (token.IsCancellationRequested) return;

                        onCompleted?.Invoke();
                    }
                    catch (TaskCanceledException) { }
                }, token);
            }
        }

        #endregion // ImageFiltrationAsyncDebounced()

        #region GenerateGaussKernel() : void  -  Сгенерировать ядро размытия Гаусса

        /// <summary>  Сгенерировать ядро размытия Гаусса  </summary>
        public void GenerateGaussKernel()
        {
            if (Kernel is null) return;

            if (Kernel.GetLength(0) % 2 == 0 || Kernel.GetLength(1) % 2 == 0) return;
            if (!(Sigma > 0)) return;

            int n = RadiusX * 2 + 1;
            int m = RadiusY * 2 + 1;
            var kernel = new float[n, m];
            int centerX = n / 2;
            int centerY = m / 2;

            double sigmaSqr = Sigma * Sigma;
            double coefficient = 1.0 / (2.0 * Math.PI * sigmaSqr);

            Parallel.For(-RadiusX, RadiusX + 1, i =>
            {
                for (int j = -RadiusY; j < (RadiusY + 1); ++j)
                {
                    double value = coefficient * Math.Exp(-(i * i + j * j) / (2 * sigmaSqr));
                    kernel[i + RadiusX, j + RadiusY] = (float)Math.Round(value, 5, MidpointRounding.AwayFromZero); 
                }
            });

            Kernel = kernel;
        }

        #endregion // GenerateGaussKernel()

        #endregion // МЕТОДЫ



        public FiltersTool(BitmapSource bitmap)
        {
            InnerBitmap = bitmap;
            ResultBitmap = bitmap;
        }
    }
}
