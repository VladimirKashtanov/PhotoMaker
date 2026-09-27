using OxyPlot;
using PhotoMaker.Client.Infrastructure.Commands;
using PhotoMaker.Model;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace PhotoMaker.Client.ViewModel
{
    public class CurvesToolVM : Base.BaseViewModel
    {
        #region МОДЕЛЬ 

        public CurvesTool Model { get; }

        private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not CurvesTool) return;

            if (Application.Current.Dispatcher.CheckAccess())
                OnPropertyChanged(e.PropertyName);
            else
                Application.Current.Dispatcher?.Invoke(() =>
                    OnPropertyChanged(e.PropertyName)
                );
        }

        #endregion // МОДЕЛЬ



        #region СВОЙСТВА

        #region GradationCurvePlot : PlotModel  -  График градационной кривой

        /// <summary>  График градационной кривой  </summary>
        public PlotModel GradationCurvePlot => Model.GradationCurvePlot;

        #endregion // GradationCurvePlot
        #region PixelHistogramPlot : PlotModel  -  График градационной кривой

        /// <summary>  График градационной кривой  </summary>
        public PlotModel PixelHistogramPlot => Model.PixelHistogramPlot;

        #endregion // PixelHistogramPlot

        #endregion // СВОЙСТВА



        #region ПОЛЯ

        #region _draggingNode : Model.DataPoint  -  Перемещаемый узел градационной кривой

        /// <summary>  Перемещаемый узел градационной кривой  </summary>
        private Model.DataPoint? _draggingNode;

        #endregion // _draggingNode
        #region _isDragging : bool  -  Активность перемещения узла

        /// <summary>  Активность перемещения узла  </summary>
        private bool _isDragging = false;

        #endregion // _isDragging

        #endregion // ПОЛЯ



        #region КОМАНДЫ

        #region StartDragGradationCurveCommand : ICommand  -  Команда начала вытягивания градационной кривой

        /// <summary>  Команда начала вытягивания градационной кривой.  </summary>
        public ICommand StartDragGradationCurveCommand { get; }

        private void StartDragGradationCurveCommand_Execute(object? parameter)
        {
            if (parameter is not Model.DataPoint plotPoint) return;

            // Ищем ближайший узел
            var nearestNode = Model.Nodes.MinBy(p => Distance(p, plotPoint));
            if (nearestNode == null) return;

            var dist = Distance(nearestNode, plotPoint);
            if (dist <= CurvesTool.GRADATION_CURVE_NODE_SIZE)
            {
                StartDrag(nearestNode);
            }
            else if (dist > CurvesTool.GRADATION_CURVE_NODE_SIZE * 1.7)
            {
                // проверяем, находится ли точка в допустимой окрестности графика
                var nearestPointOnLine = Model.ColorPoints.MinBy(p => Math.Abs(p.X - plotPoint.X));
                if (nearestPointOnLine == null) return;

                const double tolerance = 5.0; // допустимая окрестность графика
                if (Math.Abs(nearestPointOnLine.Y - plotPoint.Y) > tolerance) return;

                // добавляем новый узел
                var newNode = new Model.DataPoint(nearestPointOnLine.X, nearestPointOnLine.Y);
                Model.InsertNode(newNode);
                StartDrag(newNode);
            }
            else return;

            Model.UpdateGradationCurvePlot();


            static double Distance(Model.DataPoint a, Model.DataPoint b) =>
                Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));

            void StartDrag(Model.DataPoint node)
            {
                _draggingNode = node;
                _isDragging = true;
            }
        }

        private bool StartDragGradationCurveCommand_CanExecute(object? parameter) => true;

        #endregion // StartDragGradationCurveCommand
        #region DragGradationCurveCommand : ICommand  -  Команда вытягивания градационной кривой

        /// <summary>  Команда вытягивания градационной кривой.  </summary>
        public ICommand DragGradationCurveCommand { get; }

        private void DragGradationCurveCommand_Execute(object? parameter)
        {
            if (parameter is not Model.DataPoint plotPoint) return;
            if (_draggingNode == null || !_isDragging) return;

            // проверяем уникальность узлов по x
            if (Model.Nodes.Any(n => n != _draggingNode && n.X == plotPoint.X))
                plotPoint.X = _draggingNode.X;

            // проверяем пересечение с ближайшей точкой
            var nearestNode = Model.Nodes
                .Where(node => node != _draggingNode)
                .MinBy(node => Distance(node, plotPoint));

            if (nearestNode != null)
            {
                double dist = Distance(nearestNode, plotPoint);
                if (dist <= CurvesTool.GRADATION_CURVE_NODE_SIZE * 1.7) return;
            }

            // блокируем изменение крайних точек по X
            if (_draggingNode.X == 0) plotPoint.X = 0;
            if (_draggingNode.X == 255) plotPoint.X = 255;

            _draggingNode.X = plotPoint.X;
            _draggingNode.Y = plotPoint.Y;

            Model.UpdateGradationCurvePlot();


            static double Distance(Model.DataPoint a, Model.DataPoint b) =>
                Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        private bool DragGradationCurveCommand_CanExecute(object? parameter) => true;

        #endregion // DragGradationCurveCommand
        #region EndDragGradationCurveCommand - Команда вытягивания градационной кривой

        /// <summary>  Команда начала вытягивания градационной кривой.  </summary>
        public ICommand EndDragGradationCurveCommand { get; }

        private void EndDragGradationCurveCommand_Execute(object? parameter)
        {
            _draggingNode = null;
            _isDragging = false;

            Model.UpdateGradationCurvePlot();
        }

        private bool EndDragGradationCurveCommand_CanExecute(object? parameter) => true;

        #endregion // EndDragGradationCurveCommand
        #region RemoveNodeCommand - Команда удаления узла с градационной кривой

        /// <summary>  Команда удаления узла с градационной кривой.  </summary>
        public ICommand RemoveNodeCommand { get; }

        private void RemoveNodeCommand_Execute(object? parameter)
        {
            if (parameter is not Model.DataPoint plotPoint) return;

            // ищем ближайший к месту клика узел
            var nearestNode = Model.Nodes.MinBy(point => Distance(point, plotPoint));
            if (nearestNode == null) return;

            // проверяем, попадает ли клик в окрестность ближайшей точки
            double dist = Distance(nearestNode, plotPoint);
            if (dist > CurvesTool.GRADATION_CURVE_NODE_SIZE) return;

            Model.RemoveNode(nearestNode);

            Model.UpdateGradationCurvePlot();


            static double Distance(Model.DataPoint a, Model.DataPoint b) =>
                Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        private bool RemoveNodeCommand_CanExecute(object? parameter) => true;

        #endregion // RemoveNodeCommand

        #endregion // КОМАНДЫ



        public CurvesToolVM(CurvesTool model)
        {
            Model = model;
            Model.PropertyChanged += Model_PropertyChanged;


            StartDragGradationCurveCommand = new LambdaCommand(StartDragGradationCurveCommand_Execute, StartDragGradationCurveCommand_CanExecute);
            DragGradationCurveCommand = new LambdaCommand(DragGradationCurveCommand_Execute, DragGradationCurveCommand_CanExecute);
            EndDragGradationCurveCommand = new LambdaCommand(EndDragGradationCurveCommand_Execute, EndDragGradationCurveCommand_CanExecute);
            RemoveNodeCommand = new LambdaCommand(RemoveNodeCommand_Execute, RemoveNodeCommand_CanExecute);
        }
    }
}
