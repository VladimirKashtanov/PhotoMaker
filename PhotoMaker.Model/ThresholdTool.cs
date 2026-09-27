using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhotoMaker.Model
{
    public class ThresholdTool : Base.NotifyModel
    {
        #region СВОЙСТВА

        #region Threshold : byte  -  Порог бинаризации

        private byte _threshold = 128;

        /// <summary>  Порог бинаризации  </summary>
        public byte Threshold
        {
            get => _threshold;
            private set => SetValue(ref _threshold, value);
        }

        #endregion // Threshold
        #region ColorBins : int[]  -  Цветовые бины гистограммы пикселей

        /// <summary>  Цветовые бины гистограммы пикселей  </summary>
        public int[] ColorBins { get; private set; } = new int[256];

        #endregion // ColorBins
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
        #region IsThresholdToolOn : bool  -  Состояние инструмента "Threshold"

        private bool _isThresholdToolOn = false;

        /// <summary>  Состояние инструмента "Threshold"  </summary>
        public bool IsThresholdToolOn
        {
            get => _isThresholdToolOn;
            set
            {
                if (SetValue(ref _isThresholdToolOn, value))
                    PixelHistogramPlotUpdate(ColorBins, threshold: 128);
            }
        }

        #endregion // IsThresholdToolOn

        #region PixelHistogramPlot : PlotModel  -  Гистограмма цветов пикселей

        private PlotModel _pixelHistogramPlot = new PlotModel();

        /// <summary>  Гистограмма цветов пикселей  </summary>
        public PlotModel PixelHistogramPlot
        {
            get => _pixelHistogramPlot;
            private set => SetValue(ref _pixelHistogramPlot, value);
        }

        #endregion // PixelHistogramPlot

        #endregion // СВОЙСТВА



        #region ПОЛЯ

        #region _imageBinarizingCts : CancellationTokenSource?  -  Токен отмены операции бинаризации изображения

        /// <summary>  Токен отмены операции бинаризации изображения  </summary>
        private CancellationTokenSource? _imageBinarizingCts;

        #endregion // _imageBinarizingCts
        #region _imageBinarizingLock : object?  -  Замок операции бинаризации изображения

        /// <summary>  Замок операции бинаризации изображения  </summary>
        private readonly object _imageBinarizingLock = new();

        #endregion // _imageBinarizingLock

        #endregion // ПОЛЯ



        #region МЕТОДЫ

        #region PixelHistogramPlotInit() : void  -  Инициализировать гистограмму пикселей

        /// <summary>  Инициализировать гистограмму пикселей  </summary>
        private void PixelHistogramPlotInit()
        {
            var plot = new PlotModel()
            {
                PlotMargins = new OxyThickness(0),
                Padding = new OxyThickness(0),
                PlotAreaBorderThickness = new OxyThickness(0),
                PlotAreaBackground = OxyColors.DimGray,
                Background = OxyColors.DimGray,
                IsLegendVisible = false
            };

            var axisX = new LinearAxis
            {
                Minimum = 0,
                Maximum = 255,
                Position = AxisPosition.Bottom,
                IsZoomEnabled = false,
                IsPanEnabled = false,
                AxislineStyle = LineStyle.None,
                TickStyle = TickStyle.None,
                MajorStep = double.NaN,
                MinorStep = double.NaN,
                MinorGridlineStyle = LineStyle.None,
                MajorTickSize = 0,
                MinorTickSize = 0,
                AxisDistance = 0
            };
            plot.Axes.Add(axisX);

            var axisY = new LinearAxis
            {
                Key = "Y",
                Minimum = 0,
                Maximum = ColorBins.Max(),
                Position = AxisPosition.Left,
                IsZoomEnabled = false,
                IsPanEnabled = false,
                AxislineStyle = LineStyle.None,
                TickStyle = TickStyle.None,
                MajorStep = double.NaN,
                MinorStep = double.NaN,
                MinorGridlineStyle = LineStyle.None,
                MajorTickSize = 0,
                MinorTickSize = 0,
                AxisDistance = 0
            };
            plot.Axes.Add(axisY);

            var histogramSeries = new RectangleBarSeries
            {
                FillColor = OxyColor.FromAColor(120, OxyColors.LightGray),
                StrokeThickness = 0
            };
            for (int i = 0; i < ColorBins.Length; i++)
            {

                histogramSeries.Items.Add(new RectangleBarItem
                {
                    X0 = i,
                    X1 = i + 1,
                    Y0 = 0,
                    Y1 = ColorBins[i]
                });
            }
            plot.Series.Add(histogramSeries);

            var visor = new VisorAnnotation
            {
                Color = OxyColors.Turquoise,
                Type = LineAnnotationType.Vertical,
                X = Threshold,
                LineStyle = LineStyle.Solid,
                StrokeThickness = 2
            };
            plot.Annotations.Add(visor);

            PixelHistogramPlot = plot;

            PixelHistogramPlot.InvalidatePlot(false);
        }

        #endregion // PixelHistogramPlotInit()
        #region PixelHistogramPlotUpdate() : void  -  Обновить гистограмму пикселей

        /// <summary>  Обновить гистограмму пикселей  </summary>
        /// <param name="colorBins">  Входные значения цветовых бинов  </param>
        /// <param name="threshold">  Новое значение порога бинаризации  </param>
        public void PixelHistogramPlotUpdate(int[]? colorBins, byte? threshold)
        {
            if (PixelHistogramPlot == null) return;

            if (colorBins is not null) ColorBins = colorBins;
            if (threshold is not null) Threshold = (byte)threshold;

            var histogramSeries = PixelHistogramPlot.Series.OfType<RectangleBarSeries>().FirstOrDefault();
            if (histogramSeries == null)
            {
                histogramSeries = new RectangleBarSeries
                {
                    FillColor = OxyColor.FromAColor(120, OxyColors.WhiteSmoke),
                    StrokeThickness = 0
                };
                PixelHistogramPlot.Series.Insert(0, histogramSeries);
            }

            histogramSeries.Items.Clear();
            int maxBin = Math.Max(1, ColorBins.Max());
            for (int i = 0; i < ColorBins.Length; i++)
            {
                histogramSeries.Items.Add(new RectangleBarItem
                {
                    X0 = i,
                    X1 = i + 1,
                    Y0 = 0,
                    Y1 = ColorBins[i]
                });
            }

            var histoAxis = PixelHistogramPlot.Axes.FirstOrDefault(a => a.Key == "Y") as LinearAxis;
            if (histoAxis != null)
            {
                histoAxis.Minimum = 0;
                histoAxis.Maximum = maxBin;
            }

            var visor = PixelHistogramPlot.Annotations.OfType<VisorAnnotation>().FirstOrDefault();
            if (visor == null)
            {
                visor = new VisorAnnotation
                {
                    Color = OxyColors.White,
                    Type = LineAnnotationType.Vertical,
                    X = Threshold,
                    LineStyle = LineStyle.Solid,
                    StrokeThickness = 3
                };
                PixelHistogramPlot.Annotations.Add(visor);
            }
            else visor.X = Threshold;

            PixelHistogramPlot.InvalidatePlot(false);
            OnPropertyChanged(nameof(PixelHistogramPlot));
        }

        #endregion // PixelHistogramPlot()

        #region ImageBinarizing() : void  -  Бинаризировать изображение

        /// <summary>  Бинаризировать изображение  </summary>
        /// <param name="bitmap">  Битмап исходного изображения  </param>
        private void ImageBinarizing(BitmapSource? bitmap)
        {
            if (bitmap != null) InnerBitmap = bitmap;

            int width = InnerBitmap.PixelWidth;
            int height = InnerBitmap.PixelHeight;
            int bytesPerPixel = 4; // Pbgra32
            int stride = (InnerBitmap.Format.BitsPerPixel * width + 7) / 8;

            var pixels = new byte[height * stride];
            InnerBitmap.CopyPixels(pixels, stride, 0);

            unsafe
            {
                fixed (byte* p = pixels)
                {
                    byte* pPixels = p;

                    Parallel.For(0, height, y =>
                    {
                        byte* row = pPixels + y * stride;
                        for (int x = 0; x < width; x++)
                        {
                            byte* pixel = row + x * bytesPerPixel;

                            var gray = 0.2125 * pixel[2] + 0.7154 * pixel[1] + 0.0721 * pixel[0];
                            pixel[0] = (byte)(gray > _threshold ? 255 : 0);
                            pixel[1] = pixel[0];
                            pixel[2] = pixel[0];
                            pixel[3] = 255;
                        }
                    });
                }
            }

            var result = BitmapSource.Create(width, height, 96, 96, PixelFormats.Pbgra32, null, pixels, stride);
            result.Freeze();

            ResultBitmap = result;
        }

        #endregion // ImageBinarizing()
        #region ImageBinarizingAsyncDebounced() : void  -  Выполнить асинхронную бинаризацию изображения с отменой

        /// <summary>  Выполнить асинхронную бинаризацию изображения с отменой  </summary>
        /// <param name="bitmap">  Битмап исходного изображения  </param>
        /// <param name="onCompleted">  Действия, производимые после выполнения метода  </param>
        public void ImageBinarizingAsyncDebounced(BitmapSource bitmap, Action? onCompleted = null)
        {
            if (!IsThresholdToolOn)
            {
                onCompleted?.Invoke();
                return;
            }

            lock (_imageBinarizingLock)
            {
                _imageBinarizingCts?.Cancel();
                _imageBinarizingCts = new CancellationTokenSource();
                var token = _imageBinarizingCts.Token;

                Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(50, token);
                        if (token.IsCancellationRequested) return;

                        var sw = Stopwatch.StartNew();

                        await Task.Run(() => ImageBinarizing(bitmap));

                        sw.Stop();
                        ImageProcessingTime = (int)sw.ElapsedMilliseconds;

                        if (token.IsCancellationRequested) return;

                        onCompleted?.Invoke();
                    }
                    catch (TaskCanceledException) { }
                }, token);
            }
        }

        #endregion // ImageBinarizingAsyncDebounced()

        #endregion // МЕТОДЫ



        public ThresholdTool(BitmapSource bitmap, int[] colorBins)
        {
            InnerBitmap = bitmap;
            ColorBins = colorBins;

            PixelHistogramPlotInit();
        }
    }



    #region VisorAnnotation : class  -  Аннотация визора для модели PlotModel

    public class VisorAnnotation : LineAnnotation
    {
        public int Max { get; set; }
        public override void Render(IRenderContext rc)
        {
            base.Render(rc);

            if (Type != LineAnnotationType.Vertical)
                return;

            var xScreen = Transform(new OxyPlot.DataPoint(X, 0)).X;
            var yTop = PlotModel?.PlotArea.Top ?? 0;

            var pts = new List<ScreenPoint>
                {
                    new ScreenPoint(xScreen - 6, yTop),
                    new ScreenPoint(xScreen + 6, yTop),
                    new ScreenPoint(xScreen, yTop + 12)
                };

            rc.DrawPolygon(pts, Color, Color, StrokeThickness, EdgeRenderingMode.Adaptive);
        }
    }

    #endregion // VisorAnnotation
}
