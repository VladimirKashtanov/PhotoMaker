using MathNet.Numerics;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.LinearAlgebra.Complex32;
using System.Diagnostics;
using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhotoMaker.Model
{
    public class FourierTool : Base.NotifyModel
    {
        #region СВОЙСТВА

        #region IsEnabled : bool  -  Состояние инструмента

        private bool _isEnabled = false;

        /// <summary>  Состояние инструмента  </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (SetValue(ref _isEnabled, value))
                {
                    if (value == true) CreateFourierTransform();
                    else FourierTransformImageBitmap = null;
                }
            }
        }

        #endregion // IsEnabled
        #region ImageBitmap : BitmapSource  -  Битмап изображения

        private BitmapSource _imageBitmap;

        /// <summary>  Битмап изображения  </summary>
        public BitmapSource ImageBitmap
        {
            get => _imageBitmap;
            set => SetValue(ref _imageBitmap, value);
        }

        #endregion // ImageBitmap
        #region FourierTransformImageBitmap : BitmapSource?  -  Битмап Фурье-образа изображения

        private BitmapSource? _fourierTransformImageBitmap;

        /// <summary>  Битмап Фурье-образа изображения  </summary>
        public BitmapSource? FourierTransformImageBitmap
        {
            get => _fourierTransformImageBitmap;
            set => SetValue(ref _fourierTransformImageBitmap, value);
        }

        #endregion // FourierTransformImageBitmap
        #region ImageProcessingTime : int  -  Время обработки изображения

        /// <summary>  Время обработки изображения  </summary>
        public int ImageProcessingTime { get; private set; }

        #endregion // ImageProcessingTime

        #endregion // СВОЙСТВА



        #region ПОЛЯ

        #region _imageFourierCts : CancellationTokenSource?  -  Токен отмены операции преобразований Фурье над изображением

        /// <summary>  Токен отмены операции преобразований Фурье над изображением  </summary>
        private CancellationTokenSource? _imageFourierCts;

        #endregion // _imageFourierCts
        #region _imageFourierLock : object?  -  Замок операции преобразований Фурье над изображением

        /// <summary>  Замок операции преобразований Фурье над изображением  </summary>
        private readonly object _imageFourierLock = new();

        #endregion // _imageFourierLock

        #endregion // ПОЛЯ



        #region МЕТОДЫ

        #region CreateFourierTransform() : void  -  Выполнить преобразования Фурье над изображением

        /// <summary>  Выполнить преобразования Фурье над изображением  </summary>
        /// <param name="imageBitmap">  Битмап исходного изображения  </param>
        public void CreateFourierTransform(BitmapSource? imageBitmap = null)
        {
            if (imageBitmap is not null) ImageBitmap = imageBitmap;

            int width = ImageBitmap.PixelWidth;
            int height = ImageBitmap.PixelHeight;
            int stride = (ImageBitmap.Format.BitsPerPixel * width + 7) / 8;

            var pixels = new byte[height * stride];
            ImageBitmap.CopyPixels(pixels, stride, 0);

            FourierTransfotm(pixels, width, height, stride);

            var result = BitmapSource.Create(width, height, 96, 96, PixelFormats.Pbgra32, null, pixels, stride);
            result.Freeze();

            FourierTransformImageBitmap = result;
        }

        #endregion // CreateFourierTransform()
        #region ImageFourierAsyncDebounced() : void  -  Выполнить асинхронные преобразования Фурье с отменой над изображением

        /// <summary>  Выполнить асинхронные преобразования Фурье с отменой над изображением  </summary>
        /// <param name="bitmap">  Битмап исходного изображения  </param>
        /// <param name="onCompleted">  Действия, производимые после выполнения метода  </param>
        public void ImageFourierAsyncDebounced(BitmapSource bitmap, Action? onCompleted = null)
        {
            lock (_imageFourierLock)
            {
                _imageFourierCts?.Cancel();
                _imageFourierCts = new CancellationTokenSource();
                var token = _imageFourierCts.Token;

                Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(100, token);
                        if (token.IsCancellationRequested) return;

                        var sw = Stopwatch.StartNew();

                        await Task.Run(() => CreateFourierTransform(bitmap));

                        sw.Stop();
                        ImageProcessingTime = (int)sw.ElapsedMilliseconds;

                        if (token.IsCancellationRequested) return;

                        onCompleted?.Invoke();
                    }
                    catch (TaskCanceledException) { }
                }, token);
            }
        }

        #endregion // ImageFourierAsyncDebounced()

        #region FourierTransfotm() : void  -  Фурье-преобразование (оркестратор)

        /// <summary>  Фурье-преобразование (оркестратор)  </summary>
        /// <param name="pixels">  Массив пикселей  </param>
        /// <param name="width">  Ширина изображения  </param>
        /// <param name="height">  Высота изображения  </param>
        /// <param name="stride">  Шаг пикселя  </param>
        private void FourierTransfotm(byte[] pixels, int width, int height, int stride)
        {
            if (pixels is null) return;

            ExtractColorChannels(pixels, width, height, stride, out double[,] red, out double[,] green, out double[,] blue);

            var redSpec = FFT2D(red, width, height);
            var greenSpec = FFT2D(green, width, height);
            var blueSpec = FFT2D(blue, width, height);

            double[,] redMag = ComputeMagnitude(redSpec, width, height);
            double[,] greenMag = ComputeMagnitude(greenSpec, width, height);
            double[,] blueMag = ComputeMagnitude(blueSpec, width, height);

            ShiftQuadrantsInPlace(redMag);
            ShiftQuadrantsInPlace(greenMag);
            ShiftQuadrantsInPlace(blueMag);

            LogScaleInPlace(redMag);
            LogScaleInPlace(greenMag);
            LogScaleInPlace(blueMag);

            NormalizeAndWrite(redMag, greenMag, blueMag, pixels, width, height, stride);
        }

        #endregion // FourierTransfotm()
        #region ExtractColorChannels() : void  -  Извлечь цветовые каналы

        /// <summary>  Извлечь цветовые каналы  </summary>
        /// <param name="pixels">  Взодной массив пикселей  </param>
        /// <param name="width">  Ширина изображения  </param>
        /// <param name="height">  Высота изображения  </param>
        /// <param name="stride">  Шаг пикселя  </param>
        /// <param name="red">  Выходно массив красного канала  </param>
        /// <param name="green">  Выходно массив зеленого канала  </param>
        /// <param name="blue">  Выходно массив синего канала  </param>
        private void ExtractColorChannels(byte[] pixels, int width, int height, int stride, 
            out double[,] red, out double[,] green, out double[,] blue)
        {
            red = new double[width, height];
            green = new double[width, height];
            blue = new double[width, height];

            for (int y = 0; y < height; ++y)
            {
                int rowOffset = y * stride;
                for (int x = 0; x < width; ++x)
                {
                    int idx = rowOffset + x * 4;
                    blue[x, y] = pixels[idx + 0];
                    green[x, y] = pixels[idx + 1];
                    red[x, y] = pixels[idx + 2];
                }
            }
        }

        #endregion // ExtractColorChannels()
        #region FFT2D() : Complex[,]  -  Применить двумерное Фурье-преобразование

        /// <summary>  Применить двумерное Фурье-преобразование  </summary>
        /// <returns>  Выходные гармоники  </returns>
        private Complex32[,] FFT2D(double[,] channel, int width, int height)
        {
            var spectrum = new Complex32[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    spectrum[x, y] = new Complex32((float)channel[x, y], 0.0f);

            // По оси X
            var row = new Complex32[width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    row[x] = spectrum[x, y];

                Fourier.Forward(row, FourierOptions.Matlab);

                for (int x = 0; x < width; x++)
                    spectrum[x, y] = row[x];
            }

            // По оси Y
            var col = new Complex32[height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                    col[y] = spectrum[x, y];

                Fourier.Forward(col, FourierOptions.Matlab);

                for (int y = 0; y < height; y++)
                    spectrum[x, y] = col[y];
            }

            return spectrum;
        }

        #endregion // FFT2D()
        #region ComputeMagnitude() : double[,]  -  Расчитать амплитуду

        /// <summary>  Расчитать амплитуду  </summary>
        /// <param name="spec">  Входные спектры  </param>
        /// <param name="width">  Ширина изображения  </param>
        /// <param name="height">  Высота изображения  </param>
        /// <returns>  Выходные амплитуды  </returns>
        private double[,] ComputeMagnitude(Complex32[,] spec, int width, int height)
        {
            var mag = new double[width, height];
            for (int x = 0; x <width; ++x)
                for (int y = 0; y < height; ++y)
                    mag[x, y] = spec[x, y].Magnitude;
            return mag;
        }

        #endregion // ComputeMagnitude()
        #region ShiftQuadrantsInPlace() : void  -  Центрировать квадрант Фурье-образа

        /// <summary>  Центрировать квадрант Фурье-образа  </summary>
        /// <param name="data">  Входные данные  </param>
        private void ShiftQuadrantsInPlace(double[,] data)
        {
            int w = data.GetLength(0), h = data.GetLength(1);
            var tmp = new double[w, h];
            int halfW = w / 2, halfH = h / 2;

            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    int newX = (x + halfW) % w;
                    int newY = (y + halfH) % h;
                    tmp[newX, newY] = data[x, y];
                }

            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    data[x, y] = tmp[x, y];
        }

        #endregion // ShiftQuadrantsInPlace()
        #region LogScaleInPlace() : void  -  Логарифмировать масштаб Фурье-образа

        /// <summary>  Логарифмировать масштаб Фурье-образа  </summary>
        /// <param name="data">  Входные данные  </param>
        /// <param name="gamma">  Степень затемнения  </param>
        private static void LogScaleInPlace(double[,] data, double gamma = 3.8)
        {
            int w = data.GetLength(0), h = data.GetLength(1);

            // Логарифм с масштабом
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    data[x, y] = Math.Log(1.0 * data[x, y]);

            // Нормализация в [0..1]
            double min = double.MaxValue, max = double.MinValue;
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    if (data[x, y] < min) min = data[x, y];
                    if (data[x, y] > max) max = data[x, y];
                }

            double range = max - min;
            if (range < 1e-12) range = 1.0;

            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    data[x, y] = (data[x, y] - min) / range;

            // Гамма-коррекция
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    data[x, y] = Math.Pow(data[x, y], gamma);
        }

        #endregion // LogScaleInPlace()
        #region NormalizeAndWrite() : void  -  Нормализовать амплитуды

        /// <summary>  Нормализовать амплитуды  </summary>
        /// <param name="red">  Амплитуды красного канала  </param>
        /// <param name="green">  Амплитуды зеленого канала  </param>
        /// <param name="blue">  Амплитуды синего канала  </param>
        /// <param name="pixels">  Массив пикселей изображения  </param>
        /// <param name="width">  Ширина изображения  </param>
        /// <param name="height">  Высота изображения  </param>
        /// <param name="stride">  Шаг пикселя  </param>
        private static void NormalizeAndWrite(double[,] red, double[,] green, double[,] blue,
                                          byte[] pixels, int width, int height, int stride)
        {
            double maxR = FindMax(red), maxG = FindMax(green), maxB = FindMax(blue);
            if (maxR <= 0) maxR = 1.0;
            if (maxG <= 0) maxG = 1.0;
            if (maxB <= 0) maxB = 1.0;

            for (int y = 0; y < height; y++)
            {
                int rowOffset = y * stride;
                for (int x = 0; x < width; x++)
                {
                    int idx = rowOffset + x * 4;
                    pixels[idx + 0] = ToByte255(blue[x, y] / maxB);
                    pixels[idx + 1] = ToByte255(green[x, y] / maxG);
                    pixels[idx + 2] = ToByte255(red[x, y] / maxR);
                    pixels[idx + 3] = 255;
                }
            }
        }

        #endregion // NormalizeAndWrite()

        #region FindMax() : double  -  Поиск максимума

        /// <summary>  Поиск максимума  </summary>
        /// <param name="a">  Входной массив  </param>
        /// <returns> Найденный максимум  </returns>
        private static double FindMax(double[,] a)
        {
            double m = double.MinValue;
            int w = a.GetLength(0), h = a.GetLength(1);
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    if (a[x, y] > m) m = a[x, y];
            return m;
        }

        #endregion // FindMax()
        #region ToByte255() : byte  -  Конвертация в byte255

        /// <summary>  Конвертация в byte255  </summary>
        /// <param name="v">  Входное значение  </param>
        /// <returns>  Сконвертированное значение  </returns>
        private static byte ToByte255(double v)
        {
            double val = Math.Round(v * 255.0);
            if (val < 0) val = 0;
            if (val > 255) val = 255;
            return (byte)val;
        }

        #endregion // ToByte255()

        #endregion // МЕТОДЫ



        public FourierTool(BitmapSource bitmap)
        {
            _imageBitmap = bitmap;
        }
    }
}
