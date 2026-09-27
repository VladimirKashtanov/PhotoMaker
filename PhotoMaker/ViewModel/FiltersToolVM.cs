using PhotoMaker.Client.Infrastructure.Commands;
using PhotoMaker.Model;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PhotoMaker.Client.ViewModel
{
    public class FiltersToolVM : Base.BaseViewModel
    {
        #region МОДЕЛЬ

        private FiltersTool Model { get; }

        private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not FiltersTool) return;

            if (Application.Current.Dispatcher.CheckAccess())
                OnPropertyChanged(e.PropertyName);
            else
                Application.Current.Dispatcher?.Invoke(() =>
                    OnPropertyChanged(e.PropertyName)
                );
        }

        #endregion // МОДЕЛЬ



        #region СВОЙСТВА

        #region IsFiltersToolOn : bool  -  Состояние инструмента "Filters"

        /// <summary>  Состояние инструмента "Threshold"  </summary>
        public bool IsFiltersToolOn
        {
            get => Model.IsFiltersToolOn;
            set
            {
                if (SetValue(Model.IsFiltersToolOn, value, v => Model.IsFiltersToolOn = v))
                    IsParametersEnabled = !IsFiltersToolOn;
            }
        }

        #endregion // IsFiltersToolOn
        #region ActiveFilter : ImageFilter.IFilter  -  Активный фильтр

        /// <summary>  Активный фильтр  </summary>
        public ImageFilter.IFilter ActiveFilter
        {
            get => Model.ActiveFilter;
            set => SetValue(Model.ActiveFilter, value, v => Model.ActiveFilter = v);
        }

        #endregion // ActiveFilter
        #region FilterModes : List  -  Режимы фильтрации

        /// <summary>  Режимы фильтрации  </summary>
        public List<ImageFilter.IFilter> FilterModes { get; }

        #endregion // FilterModes

        #region Kernel : DataTable?  -  Ядро фильтра

        /// <summary>  Ядро фильтра  </summary>
        public DataTable? Kernel
        {
            get => SyncDataTableFromKernel(Model.Kernel);
            set 
            {
                if (value is not null && Model?.Kernel != null)
                {
                    SyncKernelFromDataTable(value);

                    IsEnabled = true;
                    IsGaussFillingEnabled = true;
                }
                else
                {
                    OnPropertyChanged(nameof(Kernel));

                    IsEnabled = false;
                    IsGaussFillingEnabled = false;
                }
            }
        }

        #endregion // KernelVM
        #region RadiusX : int  -  Радиус ядра фильтра по X

        /// <summary>  Радиус ядра фильтра по X  </summary>
        public int RadiusX
        {
            get => Model.RadiusX;
            set => SetValue(Model.RadiusX, value, v => Model.RadiusX = v);
        }

        #endregion // RadiusX
        #region RadiusY : int  -  Радиус ядра фильтра по Y

        /// <summary>  Радиус ядра фильтра по Y  </summary>
        public int RadiusY
        {
            get => Model.RadiusY;
            set => SetValue(Model.RadiusY, value, v => Model.RadiusY = v);
        }

        #endregion // RadiusY
        #region Brightness : float  -  Сумма элементов ядра фильтра (яркость изображения)

        /// <summary>  Сумма элементов ядра фильтра  </summary>
        public float Brightness
        {
            get => Model.Brightness;
            set => SetValue(Model.Brightness, value, v => Model.Brightness = v);
        }

        #endregion // Brightness

        #region Sigma : float  -  Степень рассеивания размытия Гаусса

        /// <summary>  Степень рассеивания размытия Гаусса  </summary>
        public float Sigma
        {
            get => Model.Sigma;
            set => SetValue(Model.Sigma, value, v => Model.Sigma = v);
        }

        #endregion // Sigma

        #region IsEnabled : bool  -  Состояние доступности инструмента "Filters"

        private bool _isEnabled;

        /// <summary>  Состояние доступности инструмента "Filters"  </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetValue(_isEnabled, value, v => _isEnabled = v);
        }

        #endregion // IsEnabled
        #region IsParametersEnabled : bool  -  Состояние доступности параметров инструмента "Filters"

        private bool _isParametersEnabled;

        /// <summary>  Состояние доступности параметров инструмента "Filters"  </summary>
        public bool IsParametersEnabled
        {
            get => _isParametersEnabled;
            set => SetValue(_isParametersEnabled, value, v => _isParametersEnabled = v);
        }

        #endregion // IsParametersEnabled
        #region KernelVisibility : Visibility  -  Видимость матрицы ядра фильтра

        private Visibility _kernelVisibility;

        /// <summary>  Видимость матрицы ядра фильтра  </summary>
        public Visibility KernelVisibility
        {
            get => _kernelVisibility;
            set => SetValue(_kernelVisibility, value, v => _kernelVisibility = v);
        }

        #endregion // KernelVisibility
        #region IsGaussFillingEnabled : bool  -  Состояние доступности заполнения ядра фильтра Гауссианом
        private bool _isGaussFillingEnabled;

        /// <summary>  Состояние доступности заполнения ядра фильтра Гауссианом  </summary>
        public bool IsGaussFillingEnabled
        {
            get => _isGaussFillingEnabled;
            set => SetValue(_isGaussFillingEnabled, value, v => _isGaussFillingEnabled = v);
        }

        #endregion // IsGaussFillingEnabled
        #region IsRadiusEnabled : bool  -  Состояние текст-боксов для радиуса
        private bool _isRadiusEnabled;

        /// <summary>  Состояние текст-боксов для радиуса  </summary>
        public bool IsRadiusEnabled
        {
            get => _isRadiusEnabled;
            set => SetValue(_isRadiusEnabled, value, v => _isRadiusEnabled = v);
        }

        #endregion // IsRadiusEnabled

        #endregion // СВОЙСТВА



        #region КОМАНДЫ

        #region GenerateKernelCommand : ICommand  -  Команда генерации ядра фильтра

        /// <summary>  Команда генерации ядра фильтра  </summary>
        public ICommand GenerateKernelCommand { get; }

        private async Task GenerateKernelCommand_Execute(object? parameter)
        {
            if (RadiusX > 0 && RadiusY > 0 && RadiusX < 26 && RadiusY < 26)
            {
                Kernel = await Task.Run(() =>
                {
                    Model.Kernel = new float[RadiusX * 2 + 1, RadiusY * 2 + 1];
                    return SyncDataTableFromKernel(Model.Kernel);
                });

                KernelVisibility = Visibility.Visible;
                IsRadiusEnabled = false;
            }
        }

        private bool GenerateKernelCommand_CanExecute(object? parameter) => 
            Kernel is null && RadiusX > 0 && RadiusY > 0 && RadiusX < 26 && RadiusY < 26;

        #endregion // GenerateKernelCommand
        #region ClearKernelCommand : ICommand  -  Команда генерации ядра фильтра

        /// <summary>  Команда генерации ядра фильтра  </summary>
        public ICommand ClearKernelCommand { get; }

        private void ClearKernelCommand_Execute(object? parameter)
        {
            KernelVisibility = Visibility.Collapsed;
            IsRadiusEnabled = true;

            Kernel = null;
            Model.Kernel = null;
        }

        private bool ClearKernelCommand_CanExecute(object? parameter) => Kernel is not null;

        #endregion // ClearKernelCommand
        #region FillGaussianCommand : ICommand  -  Команда заполнения ядра фильтра Гауссианом

        /// <summary>  Команда заполнения ядра фильтра Гауссианом  </summary>
        public ICommand FillGaussianCommand { get; }

        private async Task FillGaussianCommand_Execute(object? parameter) =>
            await Task.Run(new Action(() => Model.GenerateGaussKernel()));

        private bool FillGaussianCommand_CanExecute(object? parameter) => 
            Kernel is not null && Sigma > 0 & Sigma < 10;

        #endregion // FillGaussianCommand
        #region UpdateKernelElementCommand : ICommand  -  Команда изменения элемента ядра фильтра

        /// <summary>  Команда изменения элемента ядра фильтра  </summary>
        public ICommand UpdateKernelElementCommand { get; }

        private void UpdateKernelElementCommand_Execute(object? parameter)
        {
            if (Kernel is null) return;
            if (parameter is not DataGridCellEditEndingEventArgs args) return;

            var rowIndex = args.Row.GetIndex();
            var colIndex = args.Column.DisplayIndex;

            if (args.EditingElement is TextBox textBox)
            {
                if (float.TryParse(textBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float val))
                {
                    if (Model.Kernel is null) return;

                    Model.Kernel[rowIndex, colIndex] = val;

                    double sum = 0;
                    foreach (DataRow row in Kernel.Rows)
                        foreach (var cell in row.ItemArray)
                            if (double.TryParse(cell?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double d))
                                sum += d;

                    Brightness = (float)Math.Round(sum, 5, MidpointRounding.AwayFromZero);
                }
            }
        }

        private bool UpdateKernelElementCommand_CanExecute(object? parameter) =>
            Kernel is not null && Sigma > 0 & Sigma < 10;

        #endregion // UpdateKernelElementCommand

        #endregion // КОМАНДЫ



        #region МЕТОДЫ

        #region SyncDataTableFromKernel() : DataTable  -  Преобразование матрицы в DataTable

        /// <summary>  Преобразование матрицы в DataTable  </summary>
        /// <param name="matrix">  Входная матрица  </param>
        /// <returns>  Преобразованный объект DataTable  </returns>
        public DataTable? SyncDataTableFromKernel(float[,]? matrix)
        {
            if (matrix == null) return null;
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            var table = new DataTable();

            for (int c = 0; c < cols; c++)
                table.Columns.Add($"C{c}", typeof(string));

            for (int r = 0; r < rows; r++)
            {
                var row = table.NewRow();
                for (int c = 0; c < cols; c++)
                    row[c] = matrix[r, c].ToString("F5", System.Globalization.CultureInfo.InvariantCulture);
                table.Rows.Add(row);
            }

            return table;
        }

        #endregion // SyncDataTableFromKernel()
        #region SyncKernelFromDataTable() : void  -  Синхронизация ядра с UI

        /// <summary>  Синхронизация ядра с UI  </summary>
        /// <param name="table">  Входная таблица матрицы  </param>
        private void SyncKernelFromDataTable(DataTable table)
        {
            var temp = new float[RadiusX * 2 + 1, RadiusY * 2 + 1];
            for (int r = 0; r < table.Rows.Count; r++)
            {
                for (int c = 0; c < table.Columns.Count; c++)
                {
                    object? cellValue = table.Rows[r][c];
                    if (!float.TryParse(cellValue?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out float val))
                        val = 0.0f;
                    temp[r, c] = val;
                }
            }

            Brightness = (float)Math.Round(temp.Cast<float>().Sum(), 5, MidpointRounding.AwayFromZero);
            Model.Kernel = temp;
        }

        #endregion // SyncKernelFromDataTable()

        #endregion // МЕТОДЫ



        public FiltersToolVM(FiltersTool model)
        {
            Model = model;
            Model.PropertyChanged += Model_PropertyChanged;



            FilterModes = model.FilterModes;

            _isEnabled = Kernel is not null ? true : false;
            _isParametersEnabled = !IsFiltersToolOn;
            _kernelVisibility = Kernel is not null ? Visibility.Visible : Visibility.Collapsed;
            _isGaussFillingEnabled = Kernel is not null ? true : false;
            _isRadiusEnabled = Kernel is null ? true : false;



            GenerateKernelCommand = new AsyncLambdaCommand(GenerateKernelCommand_Execute, GenerateKernelCommand_CanExecute);
            ClearKernelCommand = new LambdaCommand(ClearKernelCommand_Execute, ClearKernelCommand_CanExecute);
            FillGaussianCommand = new AsyncLambdaCommand(FillGaussianCommand_Execute, FillGaussianCommand_CanExecute);
            UpdateKernelElementCommand = new LambdaCommand(UpdateKernelElementCommand_Execute, UpdateKernelElementCommand_CanExecute);
        }
    }
}
