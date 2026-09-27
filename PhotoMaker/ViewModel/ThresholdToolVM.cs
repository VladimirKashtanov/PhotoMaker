using OxyPlot;
using PhotoMaker.Client.Infrastructure.Commands;
using PhotoMaker.Model;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace PhotoMaker.Client.ViewModel
{
    public class ThresholdToolVM : Base.BaseViewModel
    {
        #region МОДЕЛЬ

        private ThresholdTool Model { get; }

        private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not ThresholdTool) return;

            if (Application.Current.Dispatcher.CheckAccess())
                OnPropertyChanged(e.PropertyName);
            else
                Application.Current.Dispatcher?.Invoke(() =>
                    OnPropertyChanged(e.PropertyName)
                );
        }

        #endregion // МОДЕЛЬ


        
        #region СВОЙСТВА

        #region Threshold : byte  -  Порог бинаризации

        /// <summary>  Порог бинаризации  </summary>
        public byte Threshold => Model.Threshold;

        #endregion // Threshold
        #region IsThresholdToolOn : bool  -  Состояние инструмента "Threshold"

        /// <summary>  Состояние инструмента "Threshold"  </summary>
        public bool IsThresholdToolOn
        {
            get => Model.IsThresholdToolOn;
            set
            {
                if (SetValue(Model.IsThresholdToolOn, value, v => Model.IsThresholdToolOn = v))
                    ThresholdToolVisibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        #endregion // IsThresholdToolOn
        #region ThresholdToolVisibility : Visibility  -  Видимость инструмента "Threshold"

        private Visibility _thresholdToolVisibility;

        /// <summary>  Видимость инструмента "Threshold"  </summary>
        public Visibility ThresholdToolVisibility
        {
            get => _thresholdToolVisibility;
            set => SetValue(_thresholdToolVisibility, value, v => _thresholdToolVisibility = v);
        }

        #endregion // ThresholdToolVisibility

        #region PixelHistogramPlot : PlotModel  -  Гистограмма цветов пикселей

        /// <summary>  Гистограмма цветов пикселей  </summary>
        public PlotModel PixelHistogramPlot => Model.PixelHistogramPlot;

        #endregion // PixelHistogramPlot

        #endregion // СВОЙСТВА



        #region ПОЛЯ

        #region _isVisorDragging : bool  -  Визор в состоянии драга

        /// <summary>  Визор в состоянии драга  </summary>
        private bool _isVisorDragging = false;

        #endregion // _isVisorDragging

        #endregion // ПОЛЯ



        #region КОМАНДЫ

        #region StartDragVisorCommand : ICommand  -  Команда начала перетягивания визора

        /// <summary>  Команда начала перетягивания визора  </summary>
        public ICommand StartDragVisorCommand { get; }

        private void StartDragVisorCommand_Execute(object? parameter)
        {
            if (parameter is not Model.DataPoint plotPoint) return;

            _isVisorDragging = true;
        }

        private bool StartDragVisorCommand_CanExecute(object? parameter) => true;

        #endregion // StartDragVisorCommand
        #region DragVisorCommand : ICommand  -  Команда перетягивания визора

        /// <summary>  Команда перетягивания визора  </summary>
        public ICommand DragVisorCommand { get; }

        private void DragVisorCommand_Execute(object? parameter)
        {
            if (parameter is not Model.DataPoint plotPoint) return;
            if (!_isVisorDragging) return;

            Model.PixelHistogramPlotUpdate(colorBins: null, threshold: plotPoint.X);
        }

        private bool DragVisorCommand_CanExecute(object? parameter) => true;

        #endregion // DragVisorCommand
        #region EndDragVisorCommand : ICommand  -  Команда конца перетягивания визора

        /// <summary>  Команда конца перетягивания визора  </summary>
        public ICommand EndDragVisorCommand { get; }

        private void EndDragVisorCommand_Execute(object? parameter)
        {
            if (parameter is not Model.DataPoint plotPoint) return;

            _isVisorDragging = false;
        }

        private bool EndDragVisorCommand_CanExecute(object? parameter) => true;

        #endregion // EndDragVisorCommand

        #endregion // КОМАНДЫ



        public ThresholdToolVM(ThresholdTool model)
        {
            Model = model;
            Model.PropertyChanged += Model_PropertyChanged;


            _thresholdToolVisibility = IsThresholdToolOn ? Visibility.Visible : Visibility.Collapsed;



            StartDragVisorCommand = new LambdaCommand(StartDragVisorCommand_Execute, StartDragVisorCommand_CanExecute);
            DragVisorCommand = new LambdaCommand(DragVisorCommand_Execute, DragVisorCommand_CanExecute);
            EndDragVisorCommand = new LambdaCommand(EndDragVisorCommand_Execute, EndDragVisorCommand_CanExecute);
        }
    }
}
