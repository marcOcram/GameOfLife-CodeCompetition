using GameOfLife;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GameOfLifeWPF.Controls
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:GameOfLifeWPF.Controls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:GameOfLifeWPF.Controls;assembly=GameOfLifeWPF.Controls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:LifeBoard/>
    ///
    /// </summary>
    public class LifeBoard : Control
    {
        #region Public Fields

        // Using a DependencyProperty as the backing store for brush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AliveBrushProperty =
            DependencyProperty.Register(nameof(AliveBrush), typeof(Brush), typeof(LifeBoard), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0, 255, 0))));

        // Using a DependencyProperty as the backing store for DeadBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DeadBrushProperty =
            DependencyProperty.Register(nameof(DeadBrush), typeof(Brush), typeof(LifeBoard), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(255, 0, 0))));

        // Using a DependencyProperty as the backing store for FieldSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FieldSizeProperty =
            DependencyProperty.Register(nameof(FieldSize), typeof(int), typeof(LifeBoard), new PropertyMetadata(5));

        // Using a DependencyProperty as the backing store for LifeState.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LifeStatesProperty =
            DependencyProperty.Register(nameof(LifeStates), typeof(LifeState[,]), typeof(LifeBoard), new PropertyMetadata(null, OnLifeStatesChanged));

        // Using a DependencyProperty as the backing store for NoLifeBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NoLifeBrushProperty =
            DependencyProperty.Register(nameof(NoLifeBrush), typeof(Brush), typeof(LifeBoard), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0, 0, 0))));

        #endregion Public Fields

        #region Private Fields

        private Canvas _canvas;

        private uint _columns;

        private Rectangle[,] _fields;

        private uint _rows;

        #endregion Private Fields

        #region Public Constructors

        static LifeBoard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LifeBoard), new FrameworkPropertyMetadata(typeof(LifeBoard)));
        }

        #endregion Public Constructors

        #region Public Properties

        public Brush AliveBrush {
            get { return (Brush)GetValue(AliveBrushProperty); }
            set { SetValue(AliveBrushProperty, value); }
        }

        public Brush DeadBrush {
            get { return (Brush)GetValue(DeadBrushProperty); }
            set { SetValue(DeadBrushProperty, value); }
        }

        public int FieldSize {
            get { return (int)GetValue(FieldSizeProperty); }
            set { SetValue(FieldSizeProperty, value); }
        }

        public LifeState[,] LifeStates {
            get { return (LifeState[,])GetValue(LifeStatesProperty); }
            set { SetValue(LifeStatesProperty, value); }
        }

        public Brush NoLifeBrush {
            get { return (Brush)GetValue(NoLifeBrushProperty); }
            set { SetValue(NoLifeBrushProperty, value); }
        }



        public double ZoomLevel {
            get { return (double)GetValue(ZoomLevelProperty); }
            set { SetValue(ZoomLevelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ZoomLevel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ZoomLevelProperty =
            DependencyProperty.Register(nameof(ZoomLevel), typeof(double), typeof(LifeBoard), new PropertyMetadata(1.0));



        public uint Columns {
            get { return (uint)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Columns.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register(nameof(Columns), typeof(uint), typeof(LifeBoard), new PropertyMetadata(0));



        public uint Rows {
            get { return (uint)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Rows.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register(nameof(Rows), typeof(uint), typeof(LifeBoard), new PropertyMetadata(0));




        #endregion Public Properties

        #region Public Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //_canvas = GetTemplateChild("_canvas") as Canvas;
            
            Update();
        }

        #endregion Public Methods

        #region Private Methods

        private static void OnLifeStatesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as LifeBoard)?.Update();
        }

        private void Update()
        {
            if (this.IsInitialized && LifeStates != null) {

                uint newColumns = (uint)LifeStates.GetLength(0);
                uint newRows = (uint)LifeStates.GetLength(1);

                if (newColumns != _columns || newRows != _rows) {
                    Columns = newColumns;
                    Rows = newRows;

                    //_canvas.Width = newColumns * FieldSize;
                    //_canvas.Height = newRows * FieldSize;
                    //_fields = new Rectangle[_columns, _rows];

                    //_canvas.Children.Clear();
                    //for (int y = 0; y < _rows; ++y) {
                    //    for (int x = 0; x < _columns; ++x) {
                    //        _fields[x, y] = new Rectangle()
                    //        {
                    //            //Style = FindResource("Field") as Style
                    //            //Height = FieldSize,
                    //            //Stroke = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                    //            //StrokeThickness = FieldSize / 10.0,
                    //            //Width = FieldSize
                    //        };

                    //        Canvas.SetLeft(_fields[x, y], x * FieldSize);
                    //        Canvas.SetTop(_fields[x, y], y * FieldSize);

                    //        _canvas.Children.Add(_fields[x, y]);
                    //    }
                    //}
                }

                //for (int y = 0; y < _rows; ++y) {
                //    for (int x = 0; x < _columns; ++x) {
                //        switch (LifeStates[x, y]) {
                //            case LifeState.NoLifePossible:
                //                //_fields[x, y].Fill = NoLifeBrush;
                //                break;

                //            case LifeState.Dead:
                //                //_fields[x, y].Fill = DeadBrush;
                //                break;

                //            case LifeState.Alive:
                //                //_fields[x, y].Fill = AliveBrush;
                //                break;
                //        }
                //    }
                //}
            }
        }

        #endregion Private Methods
    }
}