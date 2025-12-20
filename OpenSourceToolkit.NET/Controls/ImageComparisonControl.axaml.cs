using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using System;

namespace OpenSourceToolkit.NET.Controls
{
    public partial class ImageComparisonControl : UserControl
    {
        private bool _isDragging;

        public static readonly StyledProperty<Bitmap> BeforeSourceProperty =
            AvaloniaProperty.Register<ImageComparisonControl, Bitmap>(nameof(BeforeSource));

        public Bitmap BeforeSource
        {
            get => GetValue(BeforeSourceProperty);
            set => SetValue(BeforeSourceProperty, value);
        }

        public static readonly StyledProperty<Bitmap> AfterSourceProperty =
            AvaloniaProperty.Register<ImageComparisonControl, Bitmap>(nameof(AfterSource));

        public Bitmap AfterSource
        {
            get => GetValue(AfterSourceProperty);
            set => SetValue(AfterSourceProperty, value);
        }

        public static readonly StyledProperty<double> SliderPositionProperty =
            AvaloniaProperty.Register<ImageComparisonControl, double>(nameof(SliderPosition), defaultValue: 0.5,
                coerce: (o, v) => Math.Max(0, Math.Min(1, v)));

        public double SliderPosition
        {
            get => GetValue(SliderPositionProperty);
            set => SetValue(SliderPositionProperty, value);
        }

        public static readonly StyledProperty<bool> ShowLabelsProperty =
            AvaloniaProperty.Register<ImageComparisonControl, bool>(nameof(ShowLabels), defaultValue: true);

        public bool ShowLabels
        {
            get => GetValue(ShowLabelsProperty);
            set => SetValue(ShowLabelsProperty, value);
        }

        public static readonly StyledProperty<bool> IsVerticalProperty =
            AvaloniaProperty.Register<ImageComparisonControl, bool>(nameof(IsVertical), defaultValue: false);

        public bool IsVertical
        {
            get => GetValue(IsVerticalProperty);
            set => SetValue(IsVerticalProperty, value);
        }

        public ImageComparisonControl()
        {
            InitializeComponent();
        }

        protected override void OnLoaded(global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            base.OnLoaded(e);
            SetupControls();
        }

        private void SetupControls()
        {
            // Controls are source-generated from x:Name in AXAML
            SliderCanvas.PointerPressed += OnPointerPressed;
            SliderCanvas.PointerMoved += OnPointerMoved;
            SliderCanvas.PointerReleased += OnPointerReleased;
            SliderCanvas.PointerCaptureLost += OnPointerCaptureLost;

            UpdateSliderPosition();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == SliderPositionProperty || change.Property == BoundsProperty || change.Property == IsVerticalProperty)
            {
                UpdateSliderPosition();
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var result = base.ArrangeOverride(finalSize);
            UpdateSliderPosition();
            return result;
        }

        private void UpdateSliderPosition()
        {
            double width = Bounds.Width;
            double height = Bounds.Height;

            if (width <= 0 || height <= 0)
                return;

            // Set cursor based on orientation
            var cursor = IsVertical
                ? new Cursor(StandardCursorType.SizeNorthSouth)
                : new Cursor(StandardCursorType.SizeWestEast);
            SliderHandle.Cursor = cursor;
            SliderGrip.Cursor = cursor;

            // Update label positions based on orientation
            if (IsVertical)
            {
                // Vertical: Before at top, After at bottom
                BeforeLabel.HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Left;
                BeforeLabel.VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Top;
                AfterLabel.HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Left;
                AfterLabel.VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Bottom;
            }
            else
            {
                // Horizontal: Before at left, After at right
                BeforeLabel.HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Left;
                BeforeLabel.VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Top;
                AfterLabel.HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Right;
                AfterLabel.VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Top;
            }

            if (IsVertical)
            {
                // Vertical mode: slider moves up/down
                double sliderY = height * SliderPosition;

                // Position the slider handle (horizontal line)
                Canvas.SetLeft(SliderHandle, 0);
                Canvas.SetTop(SliderHandle, sliderY - 2);
                SliderHandle.Width = width;
                SliderHandle.Height = 4;

                // Position the grip icon (centered on the slider)
                Canvas.SetLeft(SliderGrip, (width - 40) / 2);
                Canvas.SetTop(SliderGrip, sliderY - 12);
                SliderGrip.Width = 40;
                SliderGrip.Height = 24;

                // Clip the Before image
                BeforeClipBorder.Clip = new global::Avalonia.Media.RectangleGeometry(
                    new global::Avalonia.Rect(0, 0, width, sliderY));

                BeforeClipBorder.HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch;
                BeforeClipBorder.VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch;
                BeforeClipBorder.Width = double.NaN;
                BeforeClipBorder.Height = double.NaN;
            }
            else
            {
                // Horizontal mode: slider moves left/right
                double sliderX = width * SliderPosition;

                // Position the slider handle (vertical line)
                Canvas.SetLeft(SliderHandle, sliderX - 2);
                Canvas.SetTop(SliderHandle, 0);
                SliderHandle.Width = 4;
                SliderHandle.Height = height;

                // Position the grip icon (centered on the slider)
                Canvas.SetLeft(SliderGrip, sliderX - 12);
                Canvas.SetTop(SliderGrip, (height - 40) / 2);
                SliderGrip.Width = 24;
                SliderGrip.Height = 40;

                // Clip the Before image
                BeforeClipBorder.Clip = new global::Avalonia.Media.RectangleGeometry(
                    new global::Avalonia.Rect(0, 0, sliderX, height));

                BeforeClipBorder.HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch;
                BeforeClipBorder.VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch;
                BeforeClipBorder.Width = double.NaN;
                BeforeClipBorder.Height = double.NaN;
            }
        }

        private void OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                _isDragging = true;
                e.Pointer.Capture(SliderCanvas);
                UpdatePositionFromPointer(e);
                e.Handled = true;
            }
        }

        private void OnPointerMoved(object sender, PointerEventArgs e)
        {
            if (_isDragging)
            {
                UpdatePositionFromPointer(e);
                e.Handled = true;
            }
        }

        private void OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                e.Pointer.Capture(null);
                e.Handled = true;
            }
        }

        private void OnPointerCaptureLost(object sender, PointerCaptureLostEventArgs e)
        {
            _isDragging = false;
        }

        private void UpdatePositionFromPointer(PointerEventArgs e)
        {
            var position = e.GetPosition(this);

            if (IsVertical)
            {
                double height = Bounds.Height;
                if (height > 0)
                {
                    SliderPosition = position.Y / height;
                }
            }
            else
            {
                double width = Bounds.Width;
                if (width > 0)
                {
                    SliderPosition = position.X / width;
                }
            }
        }
    }
}
