using Microsoft.Xaml.Behaviors;
using OxyPlot.Wpf;
using PhotoMaker.Model;
using System.Windows;
using System.Windows.Input;

namespace PhotoMaker.Client.Infrastructure.Behaviors
{
    internal class ThresholdToolVisorBehavior : Behavior<PlotView>
    {
        #region StartDragVisorCommandProperty

        public ICommand StartDragVisorCommand
        {
            get => (ICommand)GetValue(StartDragVisorCommandProperty);
            set => SetValue(StartDragVisorCommandProperty, value);
        }

        public static readonly DependencyProperty StartDragVisorCommandProperty =
            DependencyProperty.Register(nameof(StartDragVisorCommand), typeof(ICommand), typeof(ThresholdToolVisorBehavior));

        #endregion // StartDragVisorCommandProperty
        #region DragVisorCommandProperty

        public ICommand DragVisorCommand
        {
            get => (ICommand)GetValue(DragVisorCommandProperty);
            set => SetValue(DragVisorCommandProperty, value);
        }

        public static readonly DependencyProperty DragVisorCommandProperty =
            DependencyProperty.Register(nameof(DragVisorCommand), typeof(ICommand), typeof(ThresholdToolVisorBehavior));

        #endregion // DragVisorCommandProperty
        #region EndDragVisorCommandProperty

        public ICommand EndDragVisorCommand
        {
            get => (ICommand)GetValue(EndDragVisorCommandProperty);
            set => SetValue(EndDragVisorCommandProperty, value);
        }

        public static readonly DependencyProperty EndDragVisorCommandProperty =
            DependencyProperty.Register(nameof(EndDragVisorCommand), typeof(ICommand), typeof(ThresholdToolVisorBehavior));

        #endregion // EndDragVisorCommandProperty

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            AssociatedObject.PreviewMouseMove += OnMouseMove;
            AssociatedObject.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
            AssociatedObject.PreviewMouseMove -= OnMouseMove;
            AssociatedObject.PreviewMouseLeftButtonUp -= OnMouseLeftButtonUp;

            base.OnDetaching();
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var screenPoint = new OxyPlot.ScreenPoint(e.GetPosition(AssociatedObject).X, e.GetPosition(AssociatedObject).Y);
            var visor = AssociatedObject.Model.Annotations.OfType<VisorAnnotation>().FirstOrDefault();

            if (visor != null)
            {
                var hit = visor.HitTest(new OxyPlot.HitTestArguments(screenPoint, 5));
                if (hit != null)
                {
                    var pos = GetPlotCoordinates(e.GetPosition(AssociatedObject));
                    if (pos != null && StartDragVisorCommand?.CanExecute(hit) == true)
                    {
                        AssociatedObject.CaptureMouse();
                        StartDragVisorCommand.Execute(pos);
                        e.Handled = true;
                    }
                }
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var pos = GetPlotCoordinates(e.GetPosition(AssociatedObject));
            if (pos != null && DragVisorCommand.CanExecute(pos) == true)
            {
                DragVisorCommand.Execute(pos);
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var pos = GetPlotCoordinates(e.GetPosition(AssociatedObject));
            if (pos != null && EndDragVisorCommand.CanExecute(pos) == true)
            {
                EndDragVisorCommand.Execute(pos);
                AssociatedObject.ReleaseMouseCapture();
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
