using GameOfLife;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GameOfLifeWPF.Controls
{
    /// <summary>
    /// A control to display <see cref="IField"/>s. It is used because the WPF visual tree is too slow for many elements (e. g. many <see cref="System.Windows.Shapes.Rectangle"/>s inside a <see cref="Canvas"/>).
    /// <para>
    /// It uses a <see cref="WriteableBitmap"/> to create an image on the fly and displaying it to the user. This is much more faster but can use a lot of memory because <see cref="WriteableBitmap"/> isn't disposed fast enough sometimes. This can lead to some kind of memory leaks.
    /// </para>
    /// </summary>
    /// <seealso cref="System.Windows.Controls.Control" />
    public class LifeBoard : Control
    {
        #region Public Fields

        // Using a DependencyProperty as the backing store for AliveColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AliveColorProperty =
            DependencyProperty.Register(nameof(AliveColor), typeof(Color), typeof(LifeBoard), new PropertyMetadata(Colors.GreenYellow, OnColorValueChanged));

        // Using a DependencyProperty as the backing store for DeadColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DeadColorProperty =
            DependencyProperty.Register(nameof(DeadColor), typeof(Color), typeof(LifeBoard), new PropertyMetadata(Colors.SlateGray, OnColorValueChanged));

        // Using a DependencyProperty as the backing store for FieldSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FieldSizeProperty =
            DependencyProperty.Register(nameof(FieldSize), typeof(int), typeof(LifeBoard), new PropertyMetadata(25, OnValueChanged));

        // Using a DependencyProperty as the backing store for Fields.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FieldsProperty =
            DependencyProperty.Register(nameof(Fields), typeof(IEnumerable<IField>), typeof(LifeBoard), new PropertyMetadata(null, OnValueChanged));

        // Using a DependencyProperty as the backing store for HighlightStroke.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HighlightStrokeProperty =
            DependencyProperty.Register(nameof(HighlightStroke), typeof(Color), typeof(LifeBoard), new PropertyMetadata(Colors.YellowGreen, OnColorValueChanged));

        // Using a DependencyProperty as the backing store for Stroke.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register(nameof(Stroke), typeof(Color), typeof(LifeBoard), new PropertyMetadata(Colors.Black, OnColorValueChanged));

        #endregion Public Fields

        #region Private Fields

        /// <summary>
        /// The thickness of the stroke of each rectangle.
        /// </summary>
        private const int _strokeThickness = 1;

        /// <summary>
        /// The fields which are given to the control.
        /// </summary>
        private IField[,] _fields;

        /// <summary>
        /// The image which is used to display the <see cref="_writeableBitmap"/>.
        /// </summary>
        private Image _image;

        /// <summary>
        /// The amount of columns of the life board.
        /// </summary>
        private int _nColumns;

        /// <summary>
        /// The amount of rows of the life board.
        /// </summary>
        private int _nRows;

        /// <summary>
        /// The old position which the mouse has hovered.
        /// </summary>
        private Point? _oldHoverPosition = null;

        /// <summary>
        /// The writeable bitmap to create the image of life board.
        /// </summary>
        private WriteableBitmap _writeableBitmap;

        #endregion Private Fields

        #region Public Constructors

        static LifeBoard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LifeBoard), new FrameworkPropertyMetadata(typeof(LifeBoard)));
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Gets or sets the color of alive fields.
        /// </summary>
        /// <value>
        /// The color of alive fields.
        /// </value>
        public Color AliveColor {
            get { return (Color)GetValue(AliveColorProperty); }
            set { SetValue(AliveColorProperty, value); }
        }

        /// <summary>
        /// Gets or sets the color of dead fields.
        /// </summary>
        /// <value>
        /// The color of dead fields.
        /// </value>
        public Color DeadColor {
            get { return (Color)GetValue(DeadColorProperty); }
            set { SetValue(DeadColorProperty, value); }
        }

        /// <summary>
        /// Gets or sets the fields.
        /// </summary>
        /// <value>
        /// The fields.
        /// </value>
        public IEnumerable<IField> Fields {
            get { return (IEnumerable<IField>)GetValue(FieldsProperty); }
            set { SetValue(FieldsProperty, value); }
        }

        /// <summary>
        /// Gets or sets the size of each field.
        /// </summary>
        /// <value>
        /// The size of each field.
        /// </value>
        public int FieldSize {
            get { return (int)GetValue(FieldSizeProperty); }
            set { SetValue(FieldSizeProperty, value); }
        }

        /// <summary>
        /// Gets or sets the color of the highlight stroke.
        /// </summary>
        /// <value>
        /// The color of the highlight stroke.
        /// </value>
        public Color HighlightStroke {
            get { return (Color)GetValue(HighlightStrokeProperty); }
            set { SetValue(HighlightStrokeProperty, value); }
        }

        /// <summary>
        /// Gets or sets the color of the stroke.
        /// </summary>
        /// <value>
        /// The stroke.
        /// </value>
        public Color Stroke {
            get { return (Color)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Draws the hover effect for a field at the given mouse position.
        /// </summary>
        /// <param name="mousePosition">The mouse position.</param>
        /// <param name="isMouseOver">if set to <c>true</c> the hover effect is drawn.</param>
        public void DrawHoverEffect(Point mousePosition, bool isMouseOver)
        {
            Position lifeBoardPosition = CalculateLifeBoardPosition(mousePosition);
            DrawField(lifeBoardPosition.X, lifeBoardPosition.Y, isMouseOver);
        }

        /// <summary>When overridden in a derived class, is invoked whenever application code or internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" />.</summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _image = (Image)Template.FindName("_image", this);

            _image.MouseMove += (s, e) => {
                if (_oldHoverPosition.HasValue) {
                    DrawHoverEffect(_oldHoverPosition.Value, false);
                }

                _oldHoverPosition = e.GetPosition(_image);
                DrawHoverEffect(_oldHoverPosition.Value, true);
            };

            _image.MouseLeave += (s, e) => {
                if (_oldHoverPosition.HasValue) {
                    DrawHoverEffect(_oldHoverPosition.Value, false);
                }

                _oldHoverPosition = null;
            };

            _image.MouseDown += (s, e) => {
                Point position = e.GetPosition(_image);
                int x = (int)position.X / FieldSize;
                int y = (int)position.Y / FieldSize;

                _fields[x, y].RequestLifeStateChangeCommand.Execute(null);
            };

            UpdateWholeBitmap();
        }

        #endregion Public Methods

        #region Private Methods

        private static void OnColorValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue) {
                (d as LifeBoard)?.RedrawFields();
            }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue) {
                (d as LifeBoard)?.UpdateWholeBitmap();
            }
        }

        /// <summary>
        /// Calculates the position of a field which is hovered by the mouse.
        /// </summary>
        /// <param name="mousePosition">The mouse position.</param>
        /// <returns></returns>
        private Position CalculateLifeBoardPosition(Point mousePosition)
        {
            return new Position(Math.Min((int)mousePosition.X / FieldSize, _nColumns - 1), Math.Min((int)mousePosition.Y / FieldSize, _nRows - 1));
        }

        /// <summary>
        /// Draws a field at the given position.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <param name="isMouseOver">if set to <c>true</c> the hover effect is drawn.</param>
        private void DrawField(int x, int y, bool isMouseOver)
        {
            IField field = _fields[x, y];
            DrawField(field, isMouseOver);
        }

        /// <summary>
        /// Draws the given field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="isMouseOver">if set to <c>true</c> the hover effect is drawn.</param>
        private void DrawField(IField field, bool isMouseOver)
        {
            if (field.IsAlive.HasValue) {
                Dispatcher.Invoke(new Action(() => {
                    int x1 = field.X * FieldSize;
                    int y1 = field.Y * FieldSize;

                    int x2 = (field.X + 1) * FieldSize;
                    int y2 = (field.Y + 1) * FieldSize;

                    Color strokeColor = isMouseOver ? HighlightStroke : Stroke;
                    Color fillColor = field.IsAlive.Value ? AliveColor : DeadColor;

                    _writeableBitmap.FillRectangle(x1, y1, x2, y2, strokeColor);
                    _writeableBitmap.FillRectangle(x1 + _strokeThickness, y1 + _strokeThickness, x2 - _strokeThickness, y2 - _strokeThickness, fillColor);
                }));
            }
        }

        /// <summary>
        /// Draws the whole life board.
        /// </summary>
        private void DrawWholeLifeBoard()
        {
            int pixelWidth = _nColumns * FieldSize;
            int pixelHeight = _nRows * FieldSize;

            DpiScale dpiScale = VisualTreeHelper.GetDpi(this);

            if (_writeableBitmap != null) {
                _writeableBitmap.Freeze(); // for disposing the old bitmap
                _image.Source = null;
            }

            _writeableBitmap = BitmapFactory.New(pixelWidth, pixelHeight);
            _image.Source = _writeableBitmap;

            RedrawFields();
        }

        private void OnFieldPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IField.IsAlive) && sender is IField field) {
                DrawField(field, false);
            }
        }

        /// <summary>
        /// Redraws all fields.
        /// </summary>
        private void RedrawFields()
        {
            for (int y = 0; y < _nRows; ++y) {
                for (int x = 0; x < _nColumns; ++x) {
                    DrawField(x, y, false);
                }
            }
        }

        /// <summary>
        /// Updates the whole bitmap by redrawing everything.
        /// </summary>
        private void UpdateWholeBitmap()
        {
            if (_image == null) {
                return;
            }

            if (_fields != null) {
                foreach (IField field in _fields) {
                    if (field is INotifyPropertyChanged notifyingField) {
                        notifyingField.PropertyChanged -= OnFieldPropertyChanged;
                    }
                }
            }

            if (Fields != null) {
                // Remark: Could be slow
                _nColumns = Fields.Max(f => f.X) + 1;
                _nRows = Fields.Max(f => f.Y) + 1;

                _fields = new IField[_nColumns, _nRows];

                foreach (IField field in Fields) {
                    _fields[field.X, field.Y] = field;

                    if (field is INotifyPropertyChanged notifyingField) {
                        notifyingField.PropertyChanged += OnFieldPropertyChanged;
                    }
                }

                DrawWholeLifeBoard();
            }
        }

        #endregion Private Methods
    }
}