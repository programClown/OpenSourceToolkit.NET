using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace OpenSourceToolkit.NET.Controls
{
    public partial class ZoomableImageControl : UserControl
    {
        private double _zoomLevel = 1.0;
        private Point _lastPanPoint;
        private bool _isPanning;
        private bool _isFitMode = true; // Track if we're in "fit to view" mode

        private const double MinZoom = 0.1;
        private const double MaxZoom = 10.0;
        private const double ZoomStepLarge = 0.25;  // 25% steps above 100%
        private const double ZoomStepSmall = 0.10;  // 10% steps below 100%

        /// <summary>Event raised when fullscreen button is clicked</summary>
        public event EventHandler FullscreenRequested;

        public static readonly StyledProperty<Bitmap> SourceProperty =
            AvaloniaProperty.Register<ZoomableImageControl, Bitmap>(nameof(Source));

        public Bitmap Source
        {
            get => GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly StyledProperty<Bitmap> ComparisonSourceProperty =
            AvaloniaProperty.Register<ZoomableImageControl, Bitmap>(nameof(ComparisonSource));

        public Bitmap ComparisonSource
        {
            get => GetValue(ComparisonSourceProperty);
            set => SetValue(ComparisonSourceProperty, value);
        }

        public static readonly StyledProperty<bool> IsComparisonModeProperty =
            AvaloniaProperty.Register<ZoomableImageControl, bool>(nameof(IsComparisonMode));

        public bool IsComparisonMode
        {
            get => GetValue(IsComparisonModeProperty);
            set => SetValue(IsComparisonModeProperty, value);
        }

        public static readonly StyledProperty<double> SliderPositionProperty =
            AvaloniaProperty.Register<ZoomableImageControl, double>(nameof(SliderPosition), 0.5);

        public double SliderPosition
        {
            get => GetValue(SliderPositionProperty);
            set => SetValue(SliderPositionProperty, value);
        }

        public static readonly StyledProperty<bool> IsVerticalComparisonProperty =
            AvaloniaProperty.Register<ZoomableImageControl, bool>(nameof(IsVerticalComparison));

        public bool IsVerticalComparison
        {
            get => GetValue(IsVerticalComparisonProperty);
            set => SetValue(IsVerticalComparisonProperty, value);
        }

        public static readonly StyledProperty<double> ZoomLevelProperty =
            AvaloniaProperty.Register<ZoomableImageControl, double>(nameof(ZoomLevel), 1.0);

        public double ZoomLevel
        {
            get => GetValue(ZoomLevelProperty);
            set => SetValue(ZoomLevelProperty, Math.Max(MinZoom, Math.Min(MaxZoom, value)));
        }

        /// <summary>
        /// When true, left-click drag will pan the image (no modifier key needed).
        /// Used when the "Pan/View Mode" tool is selected.
        /// </summary>
        public static readonly StyledProperty<bool> IsPanModeEnabledProperty =
            AvaloniaProperty.Register<ZoomableImageControl, bool>(nameof(IsPanModeEnabled), false);

        public bool IsPanModeEnabled
        {
            get => GetValue(IsPanModeEnabledProperty);
            set => SetValue(IsPanModeEnabledProperty, value);
        }

        public ZoomableImageControl()
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
            ZoomInButton.Click += (s, ev) => ZoomIn();
            ZoomOutButton.Click += (s, ev) => ZoomOut();
            ZoomFitButton.Click += (s, ev) => ZoomToFit();
            Zoom100Button.Click += (s, ev) => ZoomTo100();
            FullscreenButton.Click += (s, ev) => FullscreenRequested?.Invoke(this, EventArgs.Empty);

            // Attach wheel handler to ScrollContainer with tunneling to intercept before ScrollViewer handles it
            ScrollContainer.AddHandler(PointerWheelChangedEvent, OnPointerWheelChanged, global::Avalonia.Interactivity.RoutingStrategies.Tunnel);
            PointerPressed += OnPointerPressed;
            PointerMoved += OnPointerMoved;
            PointerReleased += OnPointerReleased;

            KeyDown += OnKeyDown;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == SourceProperty)
            {
                var oldBmp = change.OldValue as Bitmap;
                var newBmp = change.NewValue as Bitmap;
                bool sameDims = oldBmp != null && newBmp != null &&
                    Math.Abs(oldBmp.Size.Width - newBmp.Size.Width) < 1 &&
                    Math.Abs(oldBmp.Size.Height - newBmp.Size.Height) < 1;

                UpdateImage();
                if (!sameDims)
                {
                    _isFitMode = true;
                    global::Avalonia.Threading.Dispatcher.UIThread.Post(() => ZoomToFit(), global::Avalonia.Threading.DispatcherPriority.Loaded);
                }
            }
            else if (change.Property == ComparisonSourceProperty || change.Property == IsComparisonModeProperty)
            {
                UpdateImage();
            }
            else if (change.Property == ZoomLevelProperty)
            {
                _zoomLevel = (double)change.NewValue;
                ApplyZoom();
            }
            else if (change.Property == BoundsProperty && Source != null && _isFitMode)
            {
                // Re-fit when control is resized (only if we're in "fit" mode)
                ZoomToFit();
            }
            else if (change.Property == IsPanModeEnabledProperty)
            {
                // Update cursor to indicate pan mode is available
                UpdatePanModeCursor();
            }
        }

        private void UpdatePanModeCursor()
        {
            if (!_isPanning)
            {
                // Show grab cursor when pan mode is enabled and image is larger than viewport
                bool canPan = IsPanModeEnabled && CanPanImage();
                Cursor = canPan ? new Cursor(StandardCursorType.Hand) : Cursor.Default;
            }
        }

        /// <summary>Returns true if the image is larger than the viewport (panning is useful)</summary>
        private bool CanPanImage()
        {
            if (Source == null) return false;
            double imageWidth = Source.Size.Width * _zoomLevel;
            double imageHeight = Source.Size.Height * _zoomLevel;
            return imageWidth > ScrollContainer.Viewport.Width || imageHeight > ScrollContainer.Viewport.Height;
        }

        private void UpdateImage()
        {
            if (Source == null) return;
            ZoomableImage.Source = Source;
            ApplyZoom();
        }

        private void ApplyZoom()
        {
            // Always update zoom display and button states
            ZoomLevelText.Text = $"{_zoomLevel * 100:F0}%";

            UpdateZoomButtonsState();
            UpdatePanModeCursor();

            if (Source == null) return;

            double newWidth = Source.Size.Width * _zoomLevel;
            double newHeight = Source.Size.Height * _zoomLevel;

            ZoomableImage.Width = newWidth;
            ZoomableImage.Height = newHeight;

            ComparisonView.Width = newWidth;
            ComparisonView.Height = newHeight;

            ImageCanvas.Width = newWidth;
            ImageCanvas.Height = newHeight;
        }

        private void ZoomIn()
        {
            _isFitMode = false;
            double step = _zoomLevel < 1.0 ? ZoomStepSmall : ZoomStepLarge;
            SetZoom(_zoomLevel + step);
        }

        private void ZoomOut()
        {
            _isFitMode = false;
            // Use the step for the level we're zooming TO, not FROM
            double targetZoom = _zoomLevel - (_zoomLevel <= 1.0 ? ZoomStepSmall : ZoomStepLarge);
            double step = targetZoom < 1.0 ? ZoomStepSmall : ZoomStepLarge;
            SetZoom(_zoomLevel - step);
        }

        private void SetZoom(double newZoom, Point? centerPoint = null)
        {
            newZoom = Math.Max(MinZoom, Math.Min(MaxZoom, newZoom));
            if (Math.Abs(newZoom - _zoomLevel) < 0.001) return;

            Point scrollCenter;
            if (centerPoint.HasValue)
            {
                scrollCenter = centerPoint.Value;
            }
            else
            {
                scrollCenter = new Point(
                    ScrollContainer.Offset.X + ScrollContainer.Viewport.Width / 2,
                    ScrollContainer.Offset.Y + ScrollContainer.Viewport.Height / 2);
            }

            double relativeX = scrollCenter.X / (Source?.Size.Width * _zoomLevel ?? 1);
            double relativeY = scrollCenter.Y / (Source?.Size.Height * _zoomLevel ?? 1);

            _zoomLevel = newZoom;
            ZoomLevel = newZoom;
            ApplyZoom();

            if (Source != null)
            {
                double newScrollX = relativeX * Source.Size.Width * _zoomLevel - ScrollContainer.Viewport.Width / 2;
                double newScrollY = relativeY * Source.Size.Height * _zoomLevel - ScrollContainer.Viewport.Height / 2;

                ScrollContainer.Offset = new Vector(
                    Math.Max(0, newScrollX),
                    Math.Max(0, newScrollY));
            }
        }

        private void ZoomToFit()
        {
            _isFitMode = true; // Entering fit mode

            if (Source == null)
            {
                _zoomLevel = 1.0;
                ZoomLevel = 1.0;
                ApplyZoom();
                return;
            }

            // Use the scroll container's viewport for available space
            double availableWidth = 0;
            double availableHeight = 0;

            if (ScrollContainer.Viewport.Width > 0 && ScrollContainer.Viewport.Height > 0)
            {
                availableWidth = ScrollContainer.Viewport.Width;
                availableHeight = ScrollContainer.Viewport.Height;
            }
            else if (Bounds.Width > 0 && Bounds.Height > 0)
            {
                availableWidth = Bounds.Width;
                availableHeight = Bounds.Height;
            }
            else
            {
                // Not ready yet, set to 1:1 temporarily
                _zoomLevel = 1.0;
                ZoomLevel = 1.0;
                ApplyZoom();
                return;
            }

            // Subtract a small margin to prevent scrollbars from appearing
            const double scrollbarMargin = 4;
            availableWidth -= scrollbarMargin;
            availableHeight -= scrollbarMargin;

            // Calculate zoom to fit the image within the available space
            double scaleX = availableWidth / Source.Size.Width;
            double scaleY = availableHeight / Source.Size.Height;
            double fitZoom = Math.Min(scaleX, scaleY);

            // Clamp to valid zoom range
            _zoomLevel = Math.Max(MinZoom, Math.Min(MaxZoom, fitZoom));
            ZoomLevel = _zoomLevel;
            ApplyZoom();

            // Center the image
            ScrollContainer.Offset = new Vector(0, 0);
        }

        private void ZoomTo100()
        {
            _isFitMode = false; // Exiting fit mode
            _zoomLevel = 1.0;
            ZoomLevel = 1.0;
            ApplyZoom();

            if (Source != null)
            {
                double centerX = (Source.Size.Width - ScrollContainer.Viewport.Width) / 2;
                double centerY = (Source.Size.Height - ScrollContainer.Viewport.Height) / 2;
                ScrollContainer.Offset = new Vector(Math.Max(0, centerX), Math.Max(0, centerY));
            }
        }

        private void UpdateZoomButtonsState()
        {
            if (_isFitMode)
            {
                ZoomFitButton.Classes.Add("accent");
                Zoom100Button.Classes.Remove("accent");
            }
            else if (Math.Abs(_zoomLevel - 1.0) < 0.001)
            {
                ZoomFitButton.Classes.Remove("accent");
                Zoom100Button.Classes.Add("accent");
            }
            else
            {
                ZoomFitButton.Classes.Remove("accent");
                Zoom100Button.Classes.Remove("accent");
            }
        }

        private void OnPointerWheelChanged(object sender, PointerWheelEventArgs e)
        {
            if (Source == null) return;

            // Ctrl+wheel = scroll (Shift for horizontal)
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                double scrollAmount = 50;
                if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                {
                    // Horizontal scroll
                    ScrollContainer.Offset = new Vector(
                        ScrollContainer.Offset.X - e.Delta.Y * scrollAmount,
                        ScrollContainer.Offset.Y);
                }
                else
                {
                    // Vertical scroll
                    ScrollContainer.Offset = new Vector(
                        ScrollContainer.Offset.X,
                        ScrollContainer.Offset.Y - e.Delta.Y * scrollAmount);
                }
                e.Handled = true;
                return;
            }

            // Regular wheel = zoom
            _isFitMode = false;

            var position = e.GetPosition(ScrollContainer);
            var imagePoint = new Point(
                ScrollContainer.Offset.X + position.X,
                ScrollContainer.Offset.Y + position.Y);

            // Use smaller steps below 100%, normalize delta to ±1 to ignore Windows scroll settings
            double step = _zoomLevel < 1.0 ? ZoomStepSmall : ZoomStepLarge;
            double delta = e.Delta.Y > 0 ? step : -step;
            SetZoom(_zoomLevel + delta, imagePoint);

            e.Handled = true;
        }

        private void OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            var point = e.GetCurrentPoint(this);

            // Pan with: middle button, Shift+left button, or left button when pan mode is enabled
            bool shouldPan = point.Properties.IsMiddleButtonPressed ||
                (point.Properties.IsLeftButtonPressed && e.KeyModifiers.HasFlag(KeyModifiers.Shift)) ||
                (point.Properties.IsLeftButtonPressed && IsPanModeEnabled);

            if (shouldPan)
            {
                _isPanning = true;
                _lastPanPoint = e.GetPosition(ScrollContainer);
                Cursor = new Cursor(StandardCursorType.Hand);
                e.Handled = true;
            }
        }

        private void OnPointerMoved(object sender, PointerEventArgs e)
        {
            if (!_isPanning) return;

            var currentPoint = e.GetPosition(ScrollContainer);
            var delta = _lastPanPoint - currentPoint;

            ScrollContainer.Offset = new Vector(
                ScrollContainer.Offset.X + delta.X,
                ScrollContainer.Offset.Y + delta.Y);

            _lastPanPoint = currentPoint;
            e.Handled = true;
        }

        private void OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (_isPanning)
            {
                _isPanning = false;
                Cursor = Cursor.Default;
                e.Handled = true;
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                if (e.Key == Key.Add || e.Key == Key.OemPlus)
                {
                    ZoomIn();
                    e.Handled = true;
                }
                else if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
                {
                    ZoomOut();
                    e.Handled = true;
                }
                else if (e.Key == Key.D0 || e.Key == Key.NumPad0)
                {
                    ZoomToFit();
                    e.Handled = true;
                }
                else if (e.Key == Key.D1 || e.Key == Key.NumPad1)
                {
                    ZoomTo100();
                    e.Handled = true;
                }
            }
        }
    }
}
