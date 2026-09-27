using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhotoMaker.Model
{
    public class ImageEditor : Base.NotifyModel
    {
        #region СВОЙСТВА

        #region BlendedImageBitmap : BitmapSource?  -  Битмап наслоенного изображения

        private BitmapSource? _blendedImageBitmap = null;

        /// <summary>  Битмап наслоенного изображения  </summary>
        public BitmapSource? BlendedImageBitmap
        {
            get => _blendedImageBitmap;
            private set => SetValue(ref _blendedImageBitmap, value);
        }

        #endregion // BlendedImageBitmap
        #region GraduatedImageBitmap : BitmapSource?  -  Битмап градуированного изображения

        private BitmapSource? _graduatedImageBitmap = null;

        /// <summary>  Битмап градуированного изображения  </summary>
        public BitmapSource? GraduatedImageBitmap
        {
            get => _graduatedImageBitmap;
            private set => SetValue(ref _graduatedImageBitmap, value);
        }

        #endregion // BlendedImageBitmap
        #region FilteredImageBitmap : BitmapSource?  -  Битмап фильтрованного изображения

        private BitmapSource? _filteredImageBitmap = null;

        /// <summary>  Битмап фильтрованного изображения  </summary>
        public BitmapSource? FilteredImageBitmap
        {
            get => _filteredImageBitmap;
            private set => SetValue(ref _filteredImageBitmap, value);
        }

        #endregion // FilteredImageBitmap
        #region ResultImageBitmap : BitmapSource?  -  Битмап результирующего изображения

        private BitmapSource? _resultImageBitmap = null;

        /// <summary>  Битмап результирующего изображения  </summary>
        public BitmapSource? ResultImageBitmap
        {
            get => _resultImageBitmap;
            private set => SetValue(ref _resultImageBitmap, value);
        }

        public void UpdateResultImageBitmap()
        {
            if (CurvesTool == null) return;
            ResultImageBitmap = CurvesTool.ResultBitmap;
        }

        #endregion // ResultImageBitmap
        #region ImageSizeString : string  -  Строка размеров результирующего изображения

        private string _imageSizeString = BuildImageSizeString();

        /// <summary>  Строка размеров результирующего изображения  </summary>
        public string ImageSizeString
        {
            get => _imageSizeString;
            private set => SetValue(ref _imageSizeString, value);
        }

        /// <summary>  Строит строку размеров изображения  </summary>
        /// <param name="width">  Ширина изображения  </param>
        /// <param name="height">  Высота изображения  </param>
        /// <returns>  Строка размеров изображения  </returns>
        private static string BuildImageSizeString(int width = 0, int height = 0) =>
            $"{width} x {height}";

        #endregion // ImageSizeString
        #region ImageProcessingTime : int  -  Время обработки изображения

        private int _imageProcessingTime = IMAGE_PROCESSING_TIME_DEFAULT;

        /// <summary>  Время обработки изображения  </summary>
        public int ImageProcessingTime
        {
            get => _imageProcessingTime;
            private set => SetValue(ref _imageProcessingTime, value);
        }

        #endregion // ImageProcessingTime
        #region Layers : ObservableCollection  -  Слои результирующего изображения

        /// <summary>  Слои результирующего изображения  </summary>
        public ObservableCollection<ImageLayer> Layers { get; set; } = new();

        private void Layers_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender is not ObservableCollection<ImageLayer>) return;

            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems is not null)
                foreach (ImageLayer imageLayer in e.OldItems)
                    imageLayer.PropertyChanged -= ImageLayer_PropertyChanged;

            _prevLayersCount = LayersCount;
            LayersCount = Layers.Count;

            if (_prevLayersCount == 0 && LayersCount > 0) ImageProcessingInit();
            if (_prevLayersCount > 0 && LayersCount > 0) ImageBlendingPipeline();
            if (LayersCount == 0) ResetToDefault();
        }

        private void ImageLayer_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not ImageLayer) return;
            ImageBlendingPipeline();
        }

        #endregion // Layers
        #region LayersCount : int  -  Кол-во слоев на изображении

        private int _layersCount;

        /// <summary>  Кол-во слоев на изображении  </summary>
        public int LayersCount
        {
            get => _layersCount;
            private set => SetValue(ref _layersCount, value);
        }

        #endregion // LayersCount

        #region CurvesTool : CurvesTool?  -  Инструмент "Curves"

        private CurvesTool? _CurvesTool;

        /// <summary>  Инструмент "Curves"  </summary>
        public CurvesTool? CurvesTool
        {
            get => _CurvesTool;
            private set => SetValue(ref _CurvesTool, value);
        }

        private void CurvesTool_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not CurvesTool CurvesTool) return;

            if (e.PropertyName == nameof(CurvesTool.GradationCurvePlot))
                ImageGradationPipeline();
        }

        #endregion // CurvesTool
        #region FiltersTool : FiltersTool?  -  Инструмент "FiltersTool"

        private FiltersTool? _filtersTool;

        /// <summary>  Инструмент "FiltersTool"  </summary>
        public FiltersTool? FiltersTool
        {
            get => _filtersTool;
            private set => SetValue(ref _filtersTool, value);
        }

        private void FiltersTool_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not FiltersTool filtersTool) return;

            if (e.PropertyName == nameof(filtersTool.IsFiltersToolOn))
            {
                if (filtersTool.IsFiltersToolOn) ImageFiltrationPipeline();
                else
                {
                    ImageGradationPipeline();
                    FilteredImageBitmap = GraduatedImageBitmap;
                }
            }
        }

        #endregion // FiltersTool
        #region ThresholdTool : ThresholdTool?  -  Инструмент "Threshold"

        private ThresholdTool? _thresholdTool;

        /// <summary>  Инструмент "Threshold"  </summary>
        public ThresholdTool? ThresholdTool
        {
            get => _thresholdTool;
            private set => SetValue(ref _thresholdTool, value);
        }

        private void ThresholdTool_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not ThresholdTool thresholdTool) return;

            if (e.PropertyName == nameof(thresholdTool.IsThresholdToolOn))
            {
                if (thresholdTool.IsThresholdToolOn) ImageBinarizingPipeline();
                else if (FiltersTool?.IsFiltersToolOn == true) ImageFiltrationPipeline();
                else ImageGradationPipeline();
            }

            if (e.PropertyName == nameof(thresholdTool.Threshold))
                ImageBinarizingPipeline();
        }

        #endregion // ThresholdTool
        #region FourierTool : FourierTool?  -  Инструмент "Fourier"

        private FourierTool? _fourierTool;

        /// <summary>  Инструмент "Fourier"  </summary>
        public FourierTool? FourierTool
        {
            get => _fourierTool;
            private set => SetValue(ref _fourierTool, value);
        }

        private void FourierTool_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not FourierTool fourierTool) return;

            if (e.PropertyName == nameof(fourierTool.IsEnabled))
            {
                if (fourierTool.IsEnabled) ImageFourierPipeline();
                else if (ThresholdTool?.IsThresholdToolOn == true) ImageBinarizingPipeline();
                else if (FiltersTool?.IsFiltersToolOn == true) ImageFiltrationPipeline();
                else ImageGradationPipeline();
            }
        }

        #endregion // FourierTool

        #endregion // СВОЙСТВА



        #region ПОЛЯ

        #region _imageBlendingCts : CancellationTokenSource?  -  Токен отмены операции наслоения изображения

        /// <summary>  Токен отмены операции наслоения изображения  </summary>
        private CancellationTokenSource? _imageBlendingCts;

        #endregion // _imageBlendingCts
        #region _imageBlendingLock : object?  -  Замок операции наслоения изображения

        /// <summary>  Замок операции наслоения изображения  </summary>
        private readonly object _imageBlendingLock = new();

        #endregion // _imageBlendingLock

        #region _sumProcessingTime : int  -  Суммарное время обработки изображения

        /// <summary>  Суммарное время обработки изображения  </summary>
        private int _sumProcessingTime = 0;

        #endregion // ImageProcessingTime
        #region _prevLayersCount : int  -  Предыдущее кол-во слоев

        /// <summary>  Предыдущее кол-во слоев  </summary>
        private int _prevLayersCount = 0;

        #endregion // Предыдущее кол-во слоев

        #endregion // ПОЛЯ



        #region КОНСТАНТЫ

        #region IMAGE_PROCESSING_TIME_DEFAULT : int  -  Время обработки изображения по умолчанию

        /// <summary>  Время обработки изображения по умолчанию  </summary>
        public static int IMAGE_PROCESSING_TIME_DEFAULT => 0;

        #endregion // IMAGE_PROCESSING_TIME_DEFAULT

        #endregion // КОНСТАНТЫ



        #region МЕТОДЫ

        #region ResetToDefault() : void  -  Сбросить модель

        /// <summary>  Сбросить модель  </summary>
        public void ResetToDefault()
        {
            ResultImageBitmap = null;
            ImageSizeString = BuildImageSizeString();
            ImageProcessingTime = IMAGE_PROCESSING_TIME_DEFAULT;

            BlendedImageBitmap = null;
            GraduatedImageBitmap = null;
            FilteredImageBitmap = null;

            if (CurvesTool is not null) CurvesTool.PropertyChanged -= CurvesTool_PropertyChanged;
            CurvesTool = null;

            if (FiltersTool is not null) FiltersTool.PropertyChanged -= ThresholdTool_PropertyChanged;
            FiltersTool = null;

            if (ThresholdTool is not null) ThresholdTool.PropertyChanged -= ThresholdTool_PropertyChanged;
            ThresholdTool = null;
            
            if (FourierTool is not null) FourierTool.PropertyChanged -= FourierTool_PropertyChanged;
            FourierTool = null;
        }

        #endregion // ResetToDefault()

        #region ImageBlendingPipeline() : void  -  Пайплайн наложения слоев (при изменении слоев перезапускаются следующие операции при наличии)

        /// <summary>  Пайплайн наложения слоев (при изменении слоев перезапускаются следующие операции при наличии)  </summary>
        public void ImageBlendingPipeline()
        {
            _sumProcessingTime = 0;

            ImageBlendingAsyncDebounced(new Action(() =>
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    ImageGradationPipeline(isInnerPipeline: true)
                ))
            ));
        }

        #endregion // ImageBlendingPipeline()
        #region ImageGradationPipeline() : void  -  Пайплайн градации изображения (при градации перезапускаются следующие операции при наличии)

        /// <summary>  Пайплайн градации изображения (при градации перезапускаются следующие операции при наличии)  </summary>
        public void ImageGradationPipeline(bool isInnerPipeline = false)
        {
            if (BlendedImageBitmap is null) return;

            if (!isInnerPipeline) _sumProcessingTime = 0;

            CurvesTool?.ImageGradationAsyncDebounced(BlendedImageBitmap, new Action(() =>
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    CurvesTool.UpdatePixelHistogramPlot();
                    ThresholdTool?.PixelHistogramPlotUpdate(CurvesTool.ColorBins, null);

                    GraduatedImageBitmap = CurvesTool.ResultBitmap;
                    _sumProcessingTime += CurvesTool.ImageProcessingTime;

                    if (FiltersTool?.IsFiltersToolOn == false &&
                        ThresholdTool?.IsThresholdToolOn == false)
                    {
                        ResultImageBitmap = CurvesTool.ResultBitmap;
                        ImageProcessingTime = _sumProcessingTime;
                        return;
                    }

                    ImageFiltrationPipeline(isInnerPipeline: true);
                }))
            ));
        }

        #endregion // ImageGradationPipeline()
        #region ImageFiltrationPipeline() : void  -  Пайплайн фильтрации изображения (при фильтрации перезапускаются следующие операции при наличии)

        /// <summary>  Пайплайн фильтрации изображения (при фильтрации перезапускаются следующие операции при наличии)  </summary>
        public void ImageFiltrationPipeline(bool isInnerPipeline = false)
        {
            if (GraduatedImageBitmap is null) return;
            FilteredImageBitmap = GraduatedImageBitmap;

            if (!isInnerPipeline) _sumProcessingTime = 0;
            

            FiltersTool?.ImageFiltrationAsyncDebounced(GraduatedImageBitmap, new Action(() =>
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (FiltersTool.IsFiltersToolOn == true)
                    {
                        FilteredImageBitmap = FiltersTool.ResultBitmap;
                        _sumProcessingTime += FiltersTool.ImageProcessingTime;
                    }

                    if (ThresholdTool?.IsThresholdToolOn == false)
                    {
                        ResultImageBitmap = FilteredImageBitmap;
                        ImageProcessingTime = _sumProcessingTime;
                        return;
                    }                   

                    ImageBinarizingPipeline(isInnerPipeline: true);
                }))
            ));
        }

        #endregion // ImageFiltrationPipeline()
        #region ImageBinarizingPipeline() : void  -  Пайплайн бинаризации изображения (при бинаризации перезапускаются следующие операции при наличии)

        /// <summary>  Пайплайн бинаризации изображения (при бинаризации перезапускаются следующие операции при наличии)  </summary>
        public void ImageBinarizingPipeline(bool isInnerPipeline = false)
        {
            if (FilteredImageBitmap is null) return;

            if (!isInnerPipeline) _sumProcessingTime = 0;

            ThresholdTool?.ImageBinarizingAsyncDebounced(FilteredImageBitmap, new Action(() =>
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (ThresholdTool.IsThresholdToolOn == true)
                        ResultImageBitmap = ThresholdTool.ResultBitmap;
                    else
                        ResultImageBitmap = FilteredImageBitmap;

                    if (FourierTool?.IsEnabled == false)
                    {
                        ImageProcessingTime = _sumProcessingTime + ThresholdTool.ImageProcessingTime;
                        return;
                    }

                    ImageFourierPipeline(isInnerPipeline: true);
                }))
            ));
        }

        #endregion // ImageGradationPipeline()
        #region ImageFourierPipeline() : void  -  Пайплайн преобразования Фурье над изображением

        /// <summary>  Пайплайн преобразования Фурье над изображением  </summary>
        public void ImageFourierPipeline(bool isInnerPipeline = false)
        {
            if (ResultImageBitmap is null) return;

            if (!isInnerPipeline) _sumProcessingTime = 0;

            FourierTool?.ImageFourierAsyncDebounced(ResultImageBitmap, new Action(() =>
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    ImageProcessingTime = _sumProcessingTime + FourierTool.ImageProcessingTime
                ))
            ));
        }

        #endregion // ImageFourierPipeline()

        #region ImageProcessingInit() : void  -  Инициализировать операции обработки изображение

        /// <summary>  Инициализировать операции обработки изображение  </summary>
        public void ImageProcessingInit()
        {
            ImageBlendingAsyncDebounced(new Action(() =>
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (BlendedImageBitmap is null) return;

                    // Создание, при отсутствии, инструмента "Curves"
                    if (CurvesTool is null)
                    {
                        CurvesTool = new CurvesTool(BlendedImageBitmap);
                        CurvesTool.PropertyChanged += CurvesTool_PropertyChanged;

                        GraduatedImageBitmap = BlendedImageBitmap;
                    }

                    // Создание, при отсутствии, инструмента "Filters"
                    if (FiltersTool is null)
                    {
                        FiltersTool = new FiltersTool(BlendedImageBitmap);
                        FiltersTool.PropertyChanged += FiltersTool_PropertyChanged;

                        FilteredImageBitmap = BlendedImageBitmap;
                    }

                    // Создание, при отсутствии, инструмента "Threshold"
                    if (ThresholdTool is null)
                    {
                        ThresholdTool = new ThresholdTool(BlendedImageBitmap, CurvesTool.ColorBins);
                        ThresholdTool.PropertyChanged += ThresholdTool_PropertyChanged;
                    }

                    // Создание, при отсутствии, инструмента "Fourier"
                    if (FourierTool is null)
                    {
                        FourierTool = new FourierTool(BlendedImageBitmap);
                        FourierTool.PropertyChanged += FourierTool_PropertyChanged;
                    }

                    // Инициализация UI
                    ResultImageBitmap = BlendedImageBitmap;
                    ImageProcessingTime = _sumProcessingTime;
                }))
            ));
        }

        #endregion // ImageProccessingInit()
        #region AddLayersRange() : void  -  Добавить группу слоев

        /// <summary>  Добавить группу слоев  </summary>
        /// <param name="layers">  Группа слоев  </param>
        public void AddLayersRange(List<ImageLayer> layers)
        {
            if (!layers.Any()) return;

            Layers.CollectionChanged -= Layers_CollectionChanged;

            foreach (var layer in layers)
            {
                Layers.Add(layer);
                layer.PropertyChanged += ImageLayer_PropertyChanged;
            }
            
            Layers.CollectionChanged += Layers_CollectionChanged;

            _prevLayersCount = LayersCount;
            LayersCount = Layers.Count;

            if (_prevLayersCount == 0 && LayersCount > 0) ImageProcessingInit();
            if (_prevLayersCount > 0 && LayersCount > 0) ImageBlendingPipeline();
        }

        #endregion // AddLayersRange

        #region ImageBlending() : void  -  Наложить слои на изображение

        /// <summary>  Наложить слои на изображение  </summary>
        /// <returns>  Битмап результирующего изображения или null  </returns>
        private unsafe BitmapSource? ImageBlending()
        {
            if (!Layers.Any()) return null;

            int canvasWidth = Layers.Max(layer => layer.Bitmap.PixelWidth);
            int canvasHeight = Layers.Max(layer => layer.Bitmap.PixelHeight);

            int bytesPerPixel = (32 / 8); // Pbgra32

            // Массив пикселей холста
            int canvasStride = canvasWidth * bytesPerPixel;
            var canvasPixels = new byte[canvasHeight * canvasStride];

            fixed (byte* pCanvasPixels = canvasPixels)
            {
                foreach (var layer in Layers)
                {
                    bool isFirstLayer = false;
                    if (Layers.IndexOf(layer) == 0) isFirstLayer = true;

                    var layerBitmap = layer.Bitmap;
                    int layerWidth = layerBitmap.PixelWidth;
                    int layerHeight = layerBitmap.PixelHeight;
                    int layerStride = layerWidth * bytesPerPixel;

                    var layerPixels = new byte[layerHeight * layerStride];
                    layerBitmap.CopyPixels(layerPixels, layerStride, 0);

                    fixed (byte* pLayerPixels = layerPixels)
                    {
                        int offsetX = layer.OffsetX;
                        int offsetY = layer.OffsetY;

                        // Вычисление границ области наложения слоя
                        int startX = Math.Max(0, offsetX);
                        int startY = Math.Max(0, offsetY);
                        int endX = Math.Min(canvasWidth, offsetX + layerWidth);
                        int endY = Math.Min(canvasHeight, offsetY + layerHeight);

                        var blendMode = layer.ActiveBlendMode;

                        // Локальные копии указателей для использования в лямбде
                        byte* pCanvas = pCanvasPixels;
                        byte* pLayer = pLayerPixels;

                        Parallel.For(startY, endY, y =>
                        {
                            int layerY = y - offsetY;
                            byte* pCanvasRow = pCanvas + y * canvasStride;
                            byte* pLayerRow = pLayer + layerY * layerStride;

                            for (int x = startX; x < endX; ++x)
                            {
                                int layerX = x - offsetX;
                                byte* pCanvasPixel = pCanvasRow + x * bytesPerPixel;
                                byte* pLayerPixel = pLayerRow + layerX * bytesPerPixel;

                                byte layerB = *(pLayerPixel + 0);
                                byte layerG = *(pLayerPixel + 1);
                                byte layerR = *(pLayerPixel + 2);
                                byte layerA = (byte)(*(pLayerPixel + 3) * layer.Opacity / 255);

                                if (layerA == 0) continue;

                                byte canvasB = *(pCanvasPixel + 0);
                                byte canvasG = *(pCanvasPixel + 1);
                                byte canvasR = *(pCanvasPixel + 2);
                                byte canvasA = *(pCanvasPixel + 3);

                                byte newB, newG, newR, newA;
                                if (isFirstLayer)
                                {
                                    newB = layer.ChannelB ? layerB : (byte)0;
                                    newG = layer.ChannelG ? layerG : (byte)0;
                                    newR = layer.ChannelR ? layerR : (byte)0;
                                    newA = layerA;
                                }
                                else
                                {
                                    // Наложение каналов
                                    if (canvasA != 0)
                                    {
                                        newB = layer.ChannelB ? blendMode.BlendPixelPbgra32(canvasB, layerB, layerA) : (byte)0;
                                        newG = layer.ChannelG ? blendMode.BlendPixelPbgra32(canvasG, layerG, layerA) : (byte)0;
                                        newR = layer.ChannelR ? blendMode.BlendPixelPbgra32(canvasR, layerR, layerA) : (byte)0;
                                        newA = blendMode.CalcOpacityPbgra32(canvasA, layerA);
                                    }
                                    else
                                    {
                                        newB = layer.ChannelB ? layerB : (byte)0;
                                        newG = layer.ChannelG ? layerG : (byte)0;
                                        newR = layer.ChannelR ? layerR : (byte)0;
                                        newA = layerA;
                                    }
                                }

                                *(pCanvasPixel + 0) = newB;
                                *(pCanvasPixel + 1) = newG;
                                *(pCanvasPixel + 2) = newR;
                                *(pCanvasPixel + 3) = newA;
                            }
                        });
                    }
                }
            }

            var result = BitmapSource.Create(canvasWidth, canvasHeight, 96, 96, PixelFormats.Pbgra32, null, canvasPixels, canvasStride);
            result.Freeze();

            return result;
        }

        #endregion // ImageBlending()
        #region ImageBlendingAsyncDebounced() : void  -  Выполнить асинхронное градационное преобразование изображения с отменой

        /// <summary>  Выполнить асинхронное градационное преобразование изображения с отменой  </summary>
        /// <param name="onCompleted">  Действия, производимые после выполнения метода  </param>
        public void ImageBlendingAsyncDebounced(Action? onCompleted = null)
        {
            lock (_imageBlendingLock)
            {
                _imageBlendingCts?.Cancel();
                _imageBlendingCts = new CancellationTokenSource();
                var token = _imageBlendingCts.Token;

                Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(100, token);
                        if (token.IsCancellationRequested) return;

                        var sw = Stopwatch.StartNew();

                        BlendedImageBitmap = ImageBlending();

                        sw.Stop();
                        _sumProcessingTime = (int)sw.ElapsedMilliseconds;

                        if (BlendedImageBitmap is not null)
                            ImageSizeString = BuildImageSizeString(BlendedImageBitmap.PixelWidth, BlendedImageBitmap.PixelHeight);

                        onCompleted?.Invoke();
                    }
                    catch (TaskCanceledException) { }
                }, token);
            }
        }

        #endregion // ImageGradationAsyncDebounced()

        #endregion // IMAGE_SIZE_DEFAULT



        public ImageEditor()
        {
            Layers.CollectionChanged += Layers_CollectionChanged;
        }
    }
}
