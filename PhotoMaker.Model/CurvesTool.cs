using MathNet.Numerics.Interpolation;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhotoMaker.Model
{
    public class CurvesTool : Base.NotifyModel
    {
        #region СВОЙСТВА

        #region Nodes : List  -  Узлы на графике градационной кривой

        /// <summary>  Узлы на графике градационной кривой  </summary>
        public List<DataPoint> Nodes { get; private set; } = new List<DataPoint>();

        #endregion // Nodes
        #region ColorPoints : DataPoint[]  -  Цветовые точки графика градационной кривой

        /// <summary>  Цветовые точки графика градационной кривой  </summary>
        public DataPoint[] ColorPoints { get; private set; } = new DataPoint[256];

        #endregion // ColorPoints
        #region ColorBins : int[]  -  Цветовые бины гистограммы пикселей

        /// <summary>  Цветовые бины гистограммы пикселей  </summary>
        public int[] ColorBins { get; private set; } = new int[256];

        #endregion // ColorBins
        #region InnerBitmap : BitmapSource  -  Битмап входного изображения

        /// <summary>  Битмап входного изображения  </summary>
        public BitmapSource InnerBitmap { get; private set; }

        #endregion // InnerBitmap
        #region ResultBitmap : BitmapSource  -  Битмап обработанного изображения

        public BitmapSource _resultBitmap;

        /// <summary>  Битмап обработанного изображения  </summary>
        public BitmapSource ResultBitmap
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

        #region GradationCurvePlot : PlotModel  -  График градационной кривой

        private PlotModel _gradationCurvePlot = new PlotModel();

        /// <summary>  График градационной кривой  </summary>
        public PlotModel GradationCurvePlot
        {
            get => _gradationCurvePlot;
            private set => SetValue(ref _gradationCurvePlot, value);
        }

        #endregion // GradationCurvePlot
        #region PixelHistogramPlot : PlotModel  -  График градационной кривой

        private PlotModel _pixelHistogramPlot = new PlotModel();

        /// <summary>  График градационной кривой  </summary>
        public PlotModel PixelHistogramPlot
        {
            get => _pixelHistogramPlot;
            private set => SetValue(ref _pixelHistogramPlot, value);
        }

        #endregion // PixelHistogramPlot

        #endregion // СВОЙСТВА


        #region ПОЛЯ

        #region _imageGradationCts : CancellationTokenSource?  -  Токен отмены операции градации изображения

        /// <summary>  Токен отмены операции градации изображения  </summary>
        private CancellationTokenSource? _imageGradationCts;

        #endregion // _imageGradationCts
        #region _imageGradationLock : object?  -  Замок операции градации изображения

        /// <summary>  Замок операции градации изображения  </summary>
        private readonly object _imageGradationLock = new();

        #endregion // _imageGradationLock

        #endregion // ПОЛЯ


        #region КОСТАНТЫ

        #region GRADATION_CURVE_NODE_SIZE : double  -  Размер узла градационной кривой

        /// <summary>  Размер узла градационной кривой  </summary>
        public static double GRADATION_CURVE_NODE_SIZE => 5.0;

        #endregion // GRADATION_CURVE_NODE_SIZE

        #endregion // КОНСТАНТЫ


        #region МЕТОДЫ

        #region InsertNode - Добавить узел в график градационной кривой

        /// <summary>  Добавить узел в график градационной кривой  </summary>
        /// <param name="node">  Новый узел  </param>
        public void InsertNode(DataPoint node)
        {
            // Не добавляем узел, если уже есть узел с таким же X
            if (Nodes.Any(n => n.X == node.X)) return;

            // не добавляем узел, если узлов больше 8
            if (Nodes.Count > 8) return;

            // Вставляем узел так, чтобы на графике не было пересечений
            int index = 0;
            while (index < Nodes.Count && Nodes[index].X < node.X)
                ++index;

            Nodes.Insert(index, node);
        }

        #endregion // InsertNode
        #region RemoveNode - Удалить узел из графика градационной кривой

        /// <summary>  Удалить узел из графика градационной кривой  </summary>
        /// <param name="node">  Узел на удаление  </param>
        public void RemoveNode(DataPoint node)
        {
            // оставляем краевые узлы
            if (node.X == 0 || node.X == 255) return;

            Nodes.Remove(node);
        }

        #endregion // RemoveNode
        #region CalculateColorPoints - Посчитать цветовые точки градационной кривой

        /// <summary>  Посчитать цветовые точки градационной кривой  </summary>
        public void CalculateColorPoints()
        {
            if (Nodes == null || Nodes.Count < 2) return;

            var xs = Nodes.Select(n => (double)n.X).ToArray();
            var ys = Nodes.Select(n => (double)n.Y).ToArray();
            var spline = CubicSpline.InterpolateNatural(xs, ys);

            for (int x = 0; x < 256; ++x)
            {
                double y = Math.Clamp(spline.Interpolate(x), 0, 255);
                ColorPoints[x] = new DataPoint((byte)x, (byte)y);
            }
        }

        #endregion // CalculateColorPoints
        #region CalculateColorBins - Расчитать цветовые бины гистограммы

        /// <summary>  Расчитать цветовые бины гистограммы  </summary>
        public void CalculateColorBins()
        {
            if (ResultBitmap == null) return;

            int width = ResultBitmap.PixelWidth;
            int height = ResultBitmap.PixelHeight;
            int bytesPerPixel = (ResultBitmap.Format.BitsPerPixel + 7) / 8;
            int stride = (ResultBitmap.Format.BitsPerPixel * width + 7) / 8;

            var pixels = new byte[height * stride];
            ResultBitmap.CopyPixels(pixels, stride, 0);

            var bins = new int[256];

            Parallel.For(0, height, () => new int[256], (y, state, local) =>
            {
                int rowStart = y * stride;
                for (int x = 0; x < width; x++)
                {
                    int i = rowStart + x * bytesPerPixel;
                    if (i + 2 >= pixels.Length) continue;

                    byte b = pixels[i + 0];
                    byte g = pixels[i + 1];
                    byte r = pixels[i + 2];

                    var luminance = (byte)Math.Clamp(0.299 * r + 0.587 * g + 0.114 * b, 0, 255);
                    local[luminance]++;
                }
                return local;
            },
            local =>
            {
                for (int i = 0; i < 256; i++)
                    bins[i] += local[i];
            });

            ColorBins = bins;
        }

        #endregion // CalculateColorBins

        #region GradationCurvePlotInit() : void  -  Инициализировать градационную кривую

        /// <summary>  Инициализировать градационную кривую  </summary>
        private void GradationCurvePlotInit()
        {
            var plot = new PlotModel()
            {
                PlotMargins = new OxyThickness(0),
                Padding = new OxyThickness(0),
                PlotAreaBorderThickness = new OxyThickness(0),
                PlotAreaBackground = OxyColors.AliceBlue,
                Background = OxyColors.AliceBlue,
                IsLegendVisible = false
            };

            // Ось Y
            var axisY = new LinearAxis
            {
                Minimum = 0,
                Maximum = 255,
                Position = AxisPosition.Left,
                IsZoomEnabled = false,
                IsPanEnabled = false,
                AxislineStyle = LineStyle.None,
                TickStyle = TickStyle.None,
                MajorStep = double.NaN,
                MinorStep = 51,
                MinorGridlineStyle = LineStyle.Dash,
                MajorTickSize = 0,
                MinorTickSize = 0,
                AxisDistance = 0
            };
            plot.Axes.Add(axisY);

            // Ось X
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
                MinorStep = 51,
                MinorGridlineStyle = LineStyle.Dash,
                MajorTickSize = 0,
                MinorTickSize = 0,
                AxisDistance = 0
            };
            plot.Axes.Add(axisX);

            // Пропорциональная прямая
            var defautLine = new LineAnnotation
            {
                Slope = 1,
                Color = OxyColors.LightSteelBlue,
                StrokeThickness = 1,
                LineStyle = LineStyle.Solid,
                Layer = AnnotationLayer.BelowSeries
            };
            plot.Annotations.Add(defautLine);

            // Градационная кривая
            var gradationCurve = new LineSeries
            {
                Color = OxyColors.SlateGray,
                StrokeThickness = 1
            };

            CalculateColorPoints();
            foreach (var point in ColorPoints)
                gradationCurve.Points.Add(new OxyPlot.DataPoint(point.X, point.Y));
            plot.Series.Add(gradationCurve);

            // конфигурация узлов интерполяции градационной кривой
            var nodes = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = GRADATION_CURVE_NODE_SIZE,
                MarkerFill = OxyColors.SkyBlue,
                MarkerStroke = OxyColors.DarkSlateBlue
            };
            foreach (var node in Nodes)
                nodes.Points.Add(new ScatterPoint(node.X, node.Y));
            plot.Series.Add(nodes);

            GradationCurvePlot = plot;

            GradationCurvePlot.InvalidatePlot(false);
            OnPropertyChanged(nameof(GradationCurvePlot));
        }

        #endregion // GradationCurvePlotInit()
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

            // Ось X
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

            // Ось Y гистограммы
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

            // Цветовые бины
            var colorBins = new RectangleBarSeries
            {
                FillColor = OxyColor.FromAColor(120, OxyColors.LightGray),
                StrokeThickness = 0
            };

            for (int i = 0; i < ColorBins.Length; i++)
            {

                colorBins.Items.Add(new RectangleBarItem
                {
                    X0 = i,
                    X1 = i + 1,
                    Y0 = 0,
                    Y1 = ColorBins[i]
                });
            }
            plot.Series.Add(colorBins);

            PixelHistogramPlot = plot;
            PixelHistogramPlot.InvalidatePlot(false);
        }

        #endregion // PixelHistogramPlotInit()
        #region UpdateGradationCurvePlot() : void  -  Обновить график градационной кривой

        /// <summary>  Обновить график градационной кривой  </summary>
        public void UpdateGradationCurvePlot()
        {
            if (GradationCurvePlot == null) return;

            CalculateColorPoints();

            // Обновляем градационную кривую 
            var gradationCurve = GradationCurvePlot.Series.OfType<LineSeries>().FirstOrDefault();
            if (gradationCurve != null)
            {
                gradationCurve.Points.Clear();
                foreach (var point in ColorPoints)
                    gradationCurve.Points.Add(new OxyPlot.DataPoint(point.X, point.Y));
            }

            // Обновляем узлы сплайна
            var scatter = GradationCurvePlot.Series.OfType<ScatterSeries>().FirstOrDefault();
            if (scatter != null)
            {
                scatter.Points.Clear();
                foreach (var node in Nodes)
                    scatter.Points.Add(new ScatterPoint(node.X, node.Y));
            }

            GradationCurvePlot.InvalidatePlot(false);
            OnPropertyChanged(nameof(GradationCurvePlot));
        }

        #endregion // UpdateGradationCurvePlot()
        #region UpdatePixelHistogramPlot() : void  -  Обновить гистограмму пикселей

        /// <summary>  Обновить гистограмму пикселей  </summary>
        public void UpdatePixelHistogramPlot()
        {
            if (PixelHistogramPlot == null) return;

            CalculateColorBins();

            // Находим или создаем серию гистограммы
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

            // Заполняем серию гистограммы
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

            // Обновляем ось Y гистограммы
            var histoAxis = PixelHistogramPlot.Axes.FirstOrDefault(a => a.Key == "Y") as LinearAxis;
            if (histoAxis != null)
            {
                histoAxis.Minimum = 0;
                histoAxis.Maximum = maxBin;
            }

            PixelHistogramPlot.InvalidatePlot(false);
        }

        #endregion // UpdatePixelHistogramPlot()

        #region ImageGradation() : void  -  Выполнить градационное преобразование изображения

        /// <summary>  Выполнить градационное преобразование изображения  </summary>
        /// <param name="bitmap">  Битмап исходного изображения  </param>
        private void ImageGradation(BitmapSource? bitmap)
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

                            byte b = pixel[0];
                            byte g = pixel[1];
                            byte r = pixel[2];

                            pixel[0] = ColorPoints[b].Y;
                            pixel[1] = ColorPoints[g].Y;
                            pixel[2] = ColorPoints[r].Y;
                        }
                    });
                }
            }

            var result = BitmapSource.Create(width, height, 96, 96, PixelFormats.Pbgra32, null, pixels, stride);
            result.Freeze();

            ResultBitmap = result;
        }

        #endregion // ImageGradation
        #region ImageGradationAsyncDebounced() : void  -  Выполнить асинхронное градационное преобразование изображения с отменой

        /// <summary>  Выполнить асинхронное градационное преобразование изображения с отменой  </summary>
        /// <param name="bitmap">  Битмап исходного изображения  </param>
        /// <param name="onCompleted">  Действия, производимые после выполнения метода  </param>
        public void ImageGradationAsyncDebounced(BitmapSource? bitmap, Action? onCompleted = null)
        {
            lock (_imageGradationLock)
            {
                _imageGradationCts?.Cancel();
                _imageGradationCts = new CancellationTokenSource();
                var token = _imageGradationCts.Token;

                Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(100, token);
                        if (token.IsCancellationRequested) return;

                        var sw = Stopwatch.StartNew();

                        await Task.Run(() => ImageGradation(bitmap));

                        sw.Stop();
                        ImageProcessingTime = (int)sw.ElapsedMilliseconds;

                        if (token.IsCancellationRequested) return;

                        onCompleted?.Invoke();
                    }
                    catch (TaskCanceledException) { }
                }, token);
            }
        }

        #endregion // ImageGradationAsyncDebounced()

        #endregion // МЕТОДЫ


        public CurvesTool(BitmapSource bitmap)
        {
            InsertNode(new DataPoint(0, 0));
            InsertNode(new DataPoint(255, 255));

            InnerBitmap = bitmap;
            _resultBitmap = bitmap;

            GradationCurvePlotInit();

            CalculateColorBins();
            PixelHistogramPlotInit();
        }
    }


    public class DataPoint
    {
        public byte X { get; set; }
        public byte Y { get; set; }

        public DataPoint(byte x, byte y)
        {
            X = x;
            Y = y;
        }
    }
}