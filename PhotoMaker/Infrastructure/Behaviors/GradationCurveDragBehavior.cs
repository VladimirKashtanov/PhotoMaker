using Microsoft.Xaml.Behaviors;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System.Windows;
using System.Windows.Input;

namespace PhotoMaker.Client.Infrastructure.Behaviors
{
    internal class GradationCurveDragBehavior : Behavior<PlotView>
    {
        #region StartDragGradationCurveCommandProperty

        public ICommand StartDragGradationCurveCommand
        {
            get => (ICommand)GetValue(StartDragGradationCurveCommandProperty);
            set => SetValue(StartDragGradationCurveCommandProperty, value);
        }

        public static readonly DependencyProperty StartDragGradationCurveCommandProperty =
            DependencyProperty.Register(nameof(StartDragGradationCurveCommand), typeof(ICommand), typeof(GradationCurveDragBehavior));

        #endregion // StartDragGradationCurveCommandProperty
        #region DragGradationCurveCommand

        public ICommand DragGradationCurveCommand
        {
            get => (ICommand)GetValue(DragGradationCurveCommandProperty);
            set => SetValue(DragGradationCurveCommandProperty, value);
        }

        public static readonly DependencyProperty DragGradationCurveCommandProperty =
            DependencyProperty.Register(nameof(DragGradationCurveCommand), typeof(ICommand), typeof(GradationCurveDragBehavior));

        #endregion // DragGradationCurveCommand
        #region EndDragGradationCurveCommandProperty

        public ICommand EndDragGradationCurveCommand
        {
            get => (ICommand)GetValue(EndDragGradationCurveCommandProperty);
            set => SetValue(EndDragGradationCurveCommandProperty, value);
        }

        public static readonly DependencyProperty EndDragGradationCurveCommandProperty =
            DependencyProperty.Register(nameof(EndDragGradationCurveCommand), typeof(ICommand), typeof(GradationCurveDragBehavior));

        #endregion // EndDragGradationCurveCommandProperty
        #region RemoveNodeCommandProperty

        public ICommand RemoveNodeCommand
        {
            get => (ICommand)GetValue(RemoveNodeCommandProperty);
            set => SetValue(RemoveNodeCommandProperty, value);
        }

        public static readonly DependencyProperty RemoveNodeCommandProperty =
            DependencyProperty.Register(nameof(RemoveNodeCommand), typeof(ICommand), typeof(GradationCurveDragBehavior));

        #endregion // RemoveNodeCommandProperty


        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            AssociatedObject.PreviewMouseMove += OnMouseMove;
            AssociatedObject.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
            AssociatedObject.PreviewMouseRightButtonDown += OnMouseRightButtonDown;

        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
            AssociatedObject.PreviewMouseMove -= OnMouseMove;
            AssociatedObject.PreviewMouseLeftButtonUp -= OnMouseLeftButtonUp;
            AssociatedObject.PreviewMouseRightButtonDown -= OnMouseRightButtonDown;

            base.OnDetaching();
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not PlotView) return;

            var gradationCurve = AssociatedObject.Model.Series.OfType<LineSeries>().FirstOrDefault();
            var nodes = AssociatedObject.Model.Series.OfType<ScatterSeries>().FirstOrDefault();

            if (nodes == null || gradationCurve == null) return;

            var screenPoint = new OxyPlot.ScreenPoint(e.GetPosition(AssociatedObject).X, e.GetPosition(AssociatedObject).Y);
            var gradationCurveHitResult = gradationCurve.HitTest(new OxyPlot.HitTestArguments(screenPoint, 5));
            var nodesHitResult = gradationCurve.HitTest(new OxyPlot.HitTestArguments(screenPoint, 2));

            if (gradationCurveHitResult == null && nodesHitResult == null) return;

            var pos = GetPlotCoordinates(e.GetPosition(AssociatedObject));
            if (pos != null && StartDragGradationCurveCommand.CanExecute(pos) == true)
            {
                AssociatedObject.CaptureMouse();
                StartDragGradationCurveCommand.Execute(pos);
                e.Handled = true;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is not PlotView) return;

            var pos = GetPlotCoordinates(e.GetPosition(AssociatedObject));
            if (pos != null && DragGradationCurveCommand.CanExecute(pos) == true)
            {
                DragGradationCurveCommand.Execute(pos);
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is not PlotView) return;

            var pos = GetPlotCoordinates(e.GetPosition(AssociatedObject));
            if (pos != null && EndDragGradationCurveCommand.CanExecute(pos) == true)
            {
                EndDragGradationCurveCommand.Execute(pos);
                AssociatedObject.ReleaseMouseCapture();
            }
        }

        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not PlotView) return;

            var pos = GetPlotCoordinates(e.GetPosition(AssociatedObject));
            if (pos != null && RemoveNodeCommand.CanExecute(pos) == true)
            {
                RemoveNodeCommand.Execute(pos);
            }
        }


        #region МЕТОДЫ

        #region GetPlotCoordinates() : Model.DataPoint  -  Преобразует экранные координаты в координаты графика

        /// <summary>  Преобразует экранные координаты в координаты графика  </summary>
        /// <param name="screenPoint">  Точка в экранных координатах  </param>
        /// <returns>  Точка в координатах графика  </returns>
        private Model.DataPoint? GetPlotCoordinates(Point screenPoint)
        {
            if (AssociatedObject is not PlotView plotView) return null;

            var model = plotView.ActualModel;
            if (model == null) return null;

            var xAxis = model.Axes.FirstOrDefault(a => a.IsHorizontal());
            var yAxis = model.Axes.FirstOrDefault(a => a.IsVertical());
            if (xAxis == null || yAxis == null) return null;

            var plotPoint = xAxis.InverseTransform(screenPoint.X, screenPoint.Y, yAxis);
            var x = (byte)Math.Clamp(plotPoint.X, 0, 255);
            var y = (byte)Math.Clamp(plotPoint.Y, 0, 255);

            return new Model.DataPoint(x, y);
        }

        #endregion // GetPlotCoordinates

        #endregion // МЕТОДЫ
    }
}
