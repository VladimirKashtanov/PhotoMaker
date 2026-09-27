using PhotoMaker.Client.Infrastructure.Commands;
using PhotoMaker.Client.Services.Interfaces;
using PhotoMaker.Client.ViewModel.Base;
using PhotoMaker.Model;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace PhotoMaker.Client.ViewModel
{
    public class MainWindowVM : BaseViewModel
    {
        #region Model : ImageEditor  -  Модель

        private ImageEditor Model { get; }

        private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not ImageEditor) return;

            if (Application.Current.Dispatcher.CheckAccess())
                UpdateProperties(e);
            else
                Application.Current.Dispatcher?.Invoke(() =>
                    UpdateProperties(e)
                );
        }

        private void UpdateProperties(PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Model.CurvesTool):
                    if (Model.CurvesTool is not null && CurvesToolVM is null)
                        CurvesToolVM = new CurvesToolVM(Model.CurvesTool);
                    else
                        CurvesToolVM = null;
                    OnPropertyChanged(nameof(CurvesToolVM));
                    break;

                case nameof(Model.FiltersTool):
                    if (Model.FiltersTool is not null && FiltersToolVM is null)
                        FiltersToolVM = new FiltersToolVM(Model.FiltersTool);
                    else
                        FiltersToolVM = null;
                    OnPropertyChanged(nameof(FiltersToolVM));
                    break;

                case nameof(Model.ThresholdTool):
                    if (Model.ThresholdTool is not null && ThresholdToolVM is null)
                        ThresholdToolVM = new ThresholdToolVM(Model.ThresholdTool);
                    else
                        ThresholdToolVM = null;
                    OnPropertyChanged(nameof(ThresholdToolVM));
                    break;

                case nameof(Model.FourierTool):
                    if (Model.FourierTool is not null && FourierToolVM is null)
                        FourierToolVM = new FourierToolVM(Model.FourierTool);
                    else
                        FourierToolVM = null;
                    OnPropertyChanged(nameof(FourierToolVM));
                    break;

                default: 
                    OnPropertyChanged(e.PropertyName);
                    break;
            }
        }

        #endregion // Model



        #region СЕРВИСЫ

        private readonly IDialogService _imageFileDialogService;
        private readonly IFileService _imageFileService;

        #endregion // СЕРВИСЫ



        #region СВОЙСТВА

        #region WindowTitle : string  -  Заголовок окна
        private string _title = "Photo Maker";

        /// <summary>  Заголовок окна  </summary>
        public string WindowTitle
        {
            get => _title;
            set => SetValue(_title, value, v => _title = v);
        }

        #endregion // WindowTitle
        #region BitmapVisibility : Visibility  -  Видимость результирующего изображения в окне

        private Visibility _resultImageBitmapVisibility;

        /// <summary>  Видимость результирующего изображения в окне  </summary>
        public Visibility ResultImageBitmapVisibility
        {
            get => _resultImageBitmapVisibility;
            set => SetValue(_resultImageBitmapVisibility, value, v => _resultImageBitmapVisibility = v);
        }

        #endregion // BitmapVisibility
        #region DropImageTextVisibility : Visibility  -  Видимость текста дропа изображения в окне

        private Visibility _dropImageTextVisibility;

        /// <summary>  Видимость текста дропа изображения в окне  </summary>
        public Visibility DropImageTextVisibility
        {
            get => _dropImageTextVisibility;
            set => SetValue(_dropImageTextVisibility, value, v => _dropImageTextVisibility = v);
        }

        #endregion // DropImageTextVisibility
        #region LayersCount : int  -  Кол-во слоев на изображении

        /// <summary>  Кол-во слоев на изображении  </summary>
        public int LayersCount => Model.LayersCount;

        #endregion // LayersCount

        #region ResultImageBitmap : BitmapSource  -  Битмап результирующего изображения

        /// <summary>  Битмап результирующего изображения  </summary>
        public BitmapSource? ResultImageBitmap => Model.ResultImageBitmap;

        #endregion // ResultImageBitmap
        #region ImageSizeString : string  -  Строка размеров обрабатываемого изображения

        /// <summary>  Строка размеров обрабатываемого изображения  </summary>
        public string ImageSizeString => Model.ImageSizeString;

        #endregion // ImageSizeString
        #region ImageProcessingTime : int  -  Время обработки изображения

        /// <summary>  Время обработки изображения  </summary>
        public int ImageProcessingTime => Model.ImageProcessingTime;

        #endregion // ImageProcessingTime
        #region LayerVMs : ObservableCollection  -  Слои результирующего изображения

        /// <summary>  Слои результирующего изображения  </summary>
        public ObservableCollection<ImageLayerVM> LayerVMs { get; set; }

        private void LayerVMs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender is not ObservableCollection<ImageLayerVM>) return;

            if (e.Action == NotifyCollectionChangedAction.Remove)
                if (e.OldItems != null)
                    foreach (ImageLayerVM layerVM in e.OldItems)
                        Model.Layers.Remove(layerVM.Model);

            if (e.Action == NotifyCollectionChangedAction.Move)
                if (e.OldItems != null && e.OldItems.Count == 1)
                    Model.Layers.Move(e.OldStartingIndex, e.NewStartingIndex);

            if (e.Action == NotifyCollectionChangedAction.Reset)
                Model.Layers.Clear();

            ResultImageBitmapVisibility = LayersCount > 0 ? Visibility.Visible : Visibility.Collapsed;
            DropImageTextVisibility = LayersCount > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        #endregion // LayerVMs

        #region CurvesToolVM : CurvesToolVM  -  Инструмент "Curves"

        /// <summary>  Инструмент "Curves"  </summary>
        public CurvesToolVM? CurvesToolVM { get; private set; } = null;

        #endregion // CurvesToolVM
        #region FiltersToolVM : FiltersToolVM  -  Инструмент "Filters"

        /// <summary>  Инструмент "Filters"  </summary>
        public FiltersToolVM? FiltersToolVM { get; private set; } = null;

        #endregion // FiltersToolVM
        #region ThresholdToolVM : ThresholdToolVM  -  Инструмент "Threshold"

        /// <summary>  Инструмент "Threshold"  </summary>
        public ThresholdToolVM? ThresholdToolVM { get; private set; } = null;

        #endregion // ThresholdToolVM
        #region FourierToolVM : FourierToolVM  -  Инструмент "Fourier"

        /// <summary>  Инструмент "Fourier"  </summary>
        public FourierToolVM? FourierToolVM { get; private set; } = null;

        #endregion // FourierToolVM

        #endregion // СВОЙСТВА



        #region КОМАНДЫ

        #region CloseApplicationCommand : ICommand  -  Команда закрытия приложения на крестик

        /// <summary>  Команда закрытия приложения на крестик  </summary>
        public ICommand CloseApplicationCommand { get; }

        private void CloseApplicationCommand_Execute(object? parameter)
        {
            if (ResultImageBitmap is null)
            {
                Application.Current.Shutdown();
                return;
            }

            string message = "Это действие приведет к выходу из приложения.\nХотите сохранить изображение?";
            var result = _imageFileDialogService.ShowMessage(message, WindowTitle, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);            

            switch (result)
            {
                case MessageBoxResult.Yes:
                    if (SaveBitmapToFile(ResultImageBitmap))
                        Application.Current.Shutdown();
                    return;
                case MessageBoxResult.No:
                    Application.Current.Shutdown();
                    return;
                case MessageBoxResult.Cancel:
                    break;
            }
        }

        private bool CloseApplicationCommand_CanExecute(object? parameter) => true;

        #endregion // CloseApplicationCommand
        #region AddLayerCommand : ICommand  -  Команда добавления слоя

        /// <summary>  Команда добавления слоя  </summary>
        public ICommand AddLayerCommand { get; }

        private void AddLayerCommand_Execute(object? parameter)
        {
            try
            {
                if (_imageFileDialogService.OpenFileDialog())
                {
                    var filepaths = _imageFileDialogService.FilePaths;
                    List<string> empty = new();
                    List<ImageLayerVM> news = new();

                    for (int i = 0; i < filepaths.Length; ++i)
                    {
                        var result = _imageFileService.Open(filepaths[i]);

                        if (result is null) empty.Add(filepaths[i]);
                        if (result is not BitmapSource bitmap) return;

                        news.Add(new ImageLayerVM(new ImageLayer(bitmap, _imageFileDialogService.FileNames[i])));
                    }

                    if (news.Count > 0)
                    {
                        AddLayersRange(news);

                            string message = "Изображения загружены успешно!";
                        _imageFileDialogService.ShowMessage(message, WindowTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                    } 

                    if (empty.Count > 0)
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("Ошибка загрузки изображений!");
                        foreach (var str in empty)
                            sb.AppendLine(str);
                        throw new NullReferenceException(sb.ToString());
                    }
                }
            }
            catch (NullReferenceException ex)
            {
                _imageFileDialogService.ShowMessage(ex.Message, WindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                string message = "Что-то пошло не так!";
                _imageFileDialogService.ShowMessage(message, WindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool AddLayerCommand_CanExecute(object? parameter) => true;

        #endregion // AddLayerCommand
        #region DropLayerCommand : ICommand  -  Команда добавления слоя дропом

        /// <summary>  Команда добавления слоя дропом  </summary>
        public ICommand DropLayerCommand { get; }

        private void DropLayerCommand_Execute(object? parameter)
        {
            if (parameter is not DragEventArgs dea) return;

            var data = new object();
            if (dea.Data.GetDataPresent(DataFormats.FileDrop))
                data = dea.Data.GetData(DataFormats.FileDrop);
            if (data is not string[] filepaths) return;

            try
            {
                if (filepaths == null || filepaths.Length == 0) throw new Exception();

                List<string> empty = new();
                List<ImageLayerVM> news = new();

                for (int i = 0; i < filepaths.Length; ++i)
                {
                    var result = _imageFileService.Open(filepaths[i]);

                    if (result is null) empty.Add(filepaths[i]);
                    if (result is not BitmapSource bitmap) return;

                    news.Add(new ImageLayerVM(new ImageLayer(bitmap, Path.GetFileName(filepaths[i]))));
                }

                if (news.Count > 0)
                {
                    AddLayersRange(news);

                    string message = "Изображения загружены успешно!";
                    _imageFileDialogService.ShowMessage(message, WindowTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                }

                if (empty.Count > 0)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Ошибка загрузки изображений!");
                    foreach (var str in empty)
                        sb.AppendLine(str);
                    throw new NullReferenceException(sb.ToString());
                }
            }
            catch (NullReferenceException ex)
            {
                _imageFileDialogService.ShowMessage(ex.Message, WindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                string message = "Что-то пошло не так!";
                _imageFileDialogService.ShowMessage(message, WindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool DropLayerCommand_CanExecute(object? parameter) => true;

        #endregion // DropLayerCommand
        #region RemoveLayerCommand : ICommand  -  Команда удаления слоя

        /// <summary>  Команда удаления слоя  </summary>
        public ICommand RemoveLayerCommand { get; }

        private void RemoveLayerCommand_Execute(object? parameter)
        {
            if (parameter is not ImageLayerVM imageLayerVM) return;
            LayerVMs.Remove(imageLayerVM);
        }

        private bool RemoveLayerCommand_CanExecute(object? parameter) => true;

        #endregion // RemoveLayerCommand
        #region ClearLayersCommand : ICommand  -  Команда очистки слоев

        /// <summary>  Команда очистки слоев  </summary>
        public ICommand ClearLayersCommand { get; }

        private void ClearLayersCommand_Execute(object? parameter)
        {
            string message = "Это действие приведет к очистке слоев.\nВы уверены, что хотите продолжить?";
            var result = _imageFileDialogService.ShowMessage(message, WindowTitle, MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.OK) LayerVMs.Clear();
        }

        private bool ClearLayersCommand_CanExecute(object? parameter) => LayerVMs.Any();

        #endregion // ClearLayersCommand
        #region SaveImageCommand : ICommand  -  Команда сохранения изображения

        /// <summary>  Команда сохранения изображения  </summary>
        public ICommand SaveImageCommand { get; }

        private void SaveImageCommand_Execute(object? parameter)
        {
            if (parameter is not BitmapSource bitmap) return;
            SaveBitmapToFile(bitmap);
        }

        private bool SaveImageCommand_CanExecute(object? parameter) => ResultImageBitmap != null;

        #endregion // SaveImageCommand
        #region MoveLayerUpCommand : ICommand  -  Команда перемещения слоя вверх

        /// <summary>  Команда перемещения слоя вверх  </summary>
        public ICommand MoveLayerUpCommand { get; }

        private void MoveLayerUpCommand_Execute(object? parameter)
        {
            if (parameter is not ImageLayerVM imageLayerVM) return;

            var index = LayerVMs.IndexOf(imageLayerVM);
            LayerVMs.Move(index, index - 1);
        }

        private bool MoveLayerUpCommand_CanExecute(object? parameter)
        {
            if (parameter is not ImageLayerVM imageLayerVM) return false;

            var index = LayerVMs.IndexOf(imageLayerVM);
            if (index > 0) return true;
            return false;
        }

        #endregion // MoveLayerUpCommand
        #region MoveLayerDownCommand : ICommand  -  Команда перемещения слоя вниз

        /// <summary>  Команда перемещения слоя вниз  </summary>
        public ICommand MoveLayerDownCommand { get; }

        private void MoveLayerDownCommand_Execute(object? parameter)
        {
            if (parameter is not ImageLayerVM imageLayerVM) return;

            var index = LayerVMs.IndexOf(imageLayerVM);
            LayerVMs.Move(index, index + 1);
        }

        private bool MoveLayerDownCommand_CanExecute(object? parameter)
        {
            if (parameter is not ImageLayerVM imageLayerVM) return false;

            var index = LayerVMs.IndexOf(imageLayerVM);
            if (index < (LayerVMs.Count - 1)) return true;
            return false;
        }

        #endregion // MoveLayerDownCommand

        #endregion // КОМАНДЫ



        #region МЕТОДЫ

        #region SaveBitmapToFile() : void  -  Сохранить изображение в файл

        /// <summary>  Сохранить изображение в файл  </summary>
        /// <param name="bitmap">  Битмап изображения  </param>
        /// <returns>  Результат сохранения  </returns>
        private bool SaveBitmapToFile(BitmapSource bitmap)
        {
            try
            {
                if (_imageFileDialogService.SaveFileDialog())
                {
                    _imageFileService.Save(_imageFileDialogService.FilePaths[0], bitmap);

                    string message = "Изображение сохранено успешно!";
                    _imageFileDialogService.ShowMessage(message, WindowTitle, MessageBoxButton.OK, MessageBoxImage.Information);

                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                string message = "Что-то пошло не так!";
                _imageFileDialogService.ShowMessage(message, WindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        #endregion // SaveBitmapToFile()
        #region AddLayersVMRange() : void  -  Добавить группу слоев изображения

        /// <summary>  Добавить группу слоев  </summary>
        /// <param name="layerVMs">  Группа слоев  </param>
        public void AddLayersRange(List<ImageLayerVM> layerVMs)
        {
            if (!layerVMs.Any()) return;

            LayerVMs.CollectionChanged -= LayerVMs_CollectionChanged;

            foreach (var layerVM in layerVMs)
                LayerVMs.Add(layerVM);

            Model.AddLayersRange(layerVMs.Select(layerVM => layerVM.Model).ToList());

            LayerVMs.CollectionChanged += LayerVMs_CollectionChanged;

            ResultImageBitmapVisibility = LayerVMs.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            DropImageTextVisibility = LayerVMs.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        #endregion // AddLayersRange

        #endregion // МЕТОДЫ



        public MainWindowVM(IDialogService dialogService, IFileService imageFileService)
        {
            Model = new ImageEditor();
            Model.PropertyChanged += Model_PropertyChanged;



            _imageFileDialogService = dialogService;
            _imageFileService = imageFileService;



            LayerVMs = new ObservableCollection<ImageLayerVM>(
                Model.Layers.Select(layer => new ImageLayerVM(layer)).ToList()
            );
            LayerVMs.CollectionChanged += LayerVMs_CollectionChanged;

            _resultImageBitmapVisibility = ResultImageBitmap != null ? Visibility.Visible : Visibility.Collapsed;
            _dropImageTextVisibility = ResultImageBitmap != null ? Visibility.Collapsed : Visibility.Visible;



            CloseApplicationCommand = new LambdaCommand(CloseApplicationCommand_Execute, CloseApplicationCommand_CanExecute);
            AddLayerCommand = new LambdaCommand(AddLayerCommand_Execute, AddLayerCommand_CanExecute);
            DropLayerCommand = new LambdaCommand(DropLayerCommand_Execute, DropLayerCommand_CanExecute);
            RemoveLayerCommand = new LambdaCommand(RemoveLayerCommand_Execute, RemoveLayerCommand_CanExecute);
            ClearLayersCommand = new LambdaCommand(ClearLayersCommand_Execute, ClearLayersCommand_CanExecute);
            SaveImageCommand = new LambdaCommand(SaveImageCommand_Execute, SaveImageCommand_CanExecute);
            MoveLayerUpCommand = new LambdaCommand(MoveLayerUpCommand_Execute, MoveLayerUpCommand_CanExecute);
            MoveLayerDownCommand = new LambdaCommand(MoveLayerDownCommand_Execute, MoveLayerDownCommand_CanExecute);
        }
    }
}