using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System;

namespace OpenSourceToolkit.NET.Controls
{
    public partial class CropSelectionOverlay : UserControl
    {
        private bool _isDragging;
        private bool _isResizing;
        private string _resizeHandle;
        private Point _dragStart;
        private Rect _selectionStart;

        // Image dimensions (actual image size)
        public static readonly StyledProperty<int> ImageWidthProperty =
            AvaloniaProperty.Register<CropSelectionOverlay, int>(nameof(ImageWidth), defaultValue: 100);

        public int ImageWidth
        {
            get => GetValue(ImageWidthProperty);
            set => SetValue(ImageWidthProperty, value);
        }

        public static readonly StyledProperty<int> ImageHeightProperty =
            AvaloniaProperty.Register<CropSelectionOverlay, int>(nameof(ImageHeight), defaultValue: 100);

        public int ImageHeight
        {
            get => GetValue(ImageHeightProperty);
            set => SetValue(ImageHeightProperty, value);
        }

        // Selection in image coordinates
        public static readonly StyledProperty<int> SelectionXProperty =
            AvaloniaProperty.Register<CropSelectionOverlay, int>(nameof(SelectionX), defaultValue: 0);

        public int SelectionX
        {
            get => GetValue(SelectionXProperty);
            set => SetValue(SelectionXProperty, value);
        }

        public static readonly StyledProperty<int> SelectionYProperty =
            AvaloniaProperty.Register<CropSelectionOverlay, int>(nameof(SelectionY), defaultValue: 0);

        public int SelectionY
        {
            get => GetValue(SelectionYProperty);
            set => SetValue(SelectionYProperty, value);
        }

        public static readonly StyledProperty<int> SelectionWidthProperty =
            AvaloniaProperty.Register<CropSelectionOverlay, int>(nameof(SelectionWidth), defaultValue: 100);

        public int SelectionWidth
        {
            get => GetValue(SelectionWidthProperty);
            set => SetValue(SelectionWidthProperty, value);
        }

        public static readonly StyledProperty<int> SelectionHeightProperty =
            AvaloniaProperty.Register<CropSelectionOverlay, int>(nameof(SelectionHeight), defaultValue: 100);

        public int SelectionHeight
        {
            get => GetValue(SelectionHeightProperty);
            set => SetValue(SelectionHeightProperty, value);
        }

        public static readonly StyledProperty<bool> IsActiveProperty =
            AvaloniaProperty.Register<CropSelectionOverlay, bool>(nameof(IsActive), defaultValue: false);

        public bool IsActive
        {
            get => GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public CropSelectionOverlay()
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
            // Wire up handle events
            WireHandleEvents(HandleNW, "NW");
            WireHandleEvents(HandleN, "N");
            WireHandleEvents(HandleNE, "NE");
            WireHandleEvents(HandleW, "W");
            WireHandleEvents(HandleE, "E");
            WireHandleEvents(HandleSW, "SW");
            WireHandleEvents(HandleS, "S");
            WireHandleEvents(HandleSE, "SE");

            // Wire up selection border for move
            SelectionBorder.PointerPressed += OnSelectionPointerPressed;
            SelectionBorder.PointerMoved += OnSelectionPointerMoved;
            SelectionBorder.PointerReleased += OnSelectionPointerReleased;

            // Wire up canvas for new selection
            OverlayCanvas.PointerPressed += OnCanvasPointerPressed;
            OverlayCanvas.PointerMoved += OnCanvasPointerMoved;
            OverlayCanvas.PointerReleased += OnCanvasPointerReleased;

            UpdateVisuals();
        }

        private void WireHandleEvents(Border handle, string handleName)
        {

            handle.PointerPressed += (s, e) =>
            {
                try
                {
                    if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                    {
                        _isResizing = true;
                        _resizeHandle = handleName;
                        _dragStart = e.GetPosition(this);
                        _selectionStart = GetDisplayRect();
                        e.Pointer.Capture(handle);
                        e.Handled = true;
                    }
                }
                catch { }
            };

            handle.PointerMoved += (s, e) =>
            {
                try
                {
                    if (_isResizing && _resizeHandle == handleName)
                    {
                        HandleResize(e.GetPosition(this));
                        e.Handled = true;
                    }
                }
                catch { }
            };

            handle.PointerReleased += (s, e) =>
            {
                if (_isResizing && _resizeHandle == handleName)
                {
                    _isResizing = false;
                    _resizeHandle = null;
                    try { e.Pointer.Capture(null); } catch { }
                    e.Handled = true;
                }
            };
        }

        private void OnSelectionPointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && !_isResizing)
            {
                _isDragging = true;
                _dragStart = e.GetPosition(this);
                _selectionStart = GetDisplayRect();
                e.Pointer.Capture(SelectionBorder);
                e.Handled = true;
            }
        }

        private void OnSelectionPointerMoved(object sender, PointerEventArgs e)
        {
            if (!_isDragging) return;
            try
            {
                var current = e.GetPosition(this);
                var delta = current - _dragStart;

                var newRect = new Rect(
                    _selectionStart.X + delta.X,
                    _selectionStart.Y + delta.Y,
                    _selectionStart.Width,
                    _selectionStart.Height);

                SetDisplayRect(ClampRect(newRect));
                e.Handled = true;
            }
            catch { /* Prevent crash on edge cases */ }
        }

        private void OnSelectionPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                try { e.Pointer.Capture(null); } catch { }
                e.Handled = true;
            }
        }

        private void OnCanvasPointerPressed(object sender, PointerPressedEventArgs e)
        {
            // Only start new selection if clicking outside existing selection but within image bounds
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && !_isDragging && !_isResizing)
            {
                var pos = e.GetPosition(this);
                var displayRect = GetDisplayRect();

                // Check if click is within the image area
                var (scale, offsetX, offsetY) = GetUniformScaleAndOffset();
                double displayedWidth = ImageWidth * scale;
                double displayedHeight = ImageHeight * scale;
                var imageRect = new Rect(offsetX, offsetY, displayedWidth, displayedHeight);

                if (!displayRect.Contains(pos) && imageRect.Contains(pos))
                {
                    _isDragging = true;
                    _dragStart = pos;
                    _selectionStart = new Rect(pos.X, pos.Y, 0, 0);
                    e.Pointer.Capture(OverlayCanvas);
                    e.Handled = true;
                }
            }
        }

        private void OnCanvasPointerMoved(object sender, PointerEventArgs e)
        {
            if (!_isDragging || e.Pointer.Captured != OverlayCanvas) return;
            try
            {
                var current = e.GetPosition(this);

                double x = Math.Min(_dragStart.X, current.X);
                double y = Math.Min(_dragStart.Y, current.Y);
                double w = Math.Abs(current.X - _dragStart.X);
                double h = Math.Abs(current.Y - _dragStart.Y);

                SetDisplayRect(ClampRect(new Rect(x, y, w, h)));
                e.Handled = true;
            }
            catch { /* Prevent crash on edge cases */ }
        }

        private void OnCanvasPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (_isDragging && e.Pointer.Captured == OverlayCanvas)
            {
                _isDragging = false;
                try { e.Pointer.Capture(null); } catch { }
                e.Handled = true;
            }
        }

        private void HandleResize(Point current)
        {
            var delta = current - _dragStart;
            var newRect = _selectionStart;

            switch (_resizeHandle)
            {
                case "NW":
                    newRect = new Rect(
                        _selectionStart.X + delta.X,
                        _selectionStart.Y + delta.Y,
                        _selectionStart.Width - delta.X,
                        _selectionStart.Height - delta.Y);
                    break;
                case "N":
                    newRect = new Rect(
                        _selectionStart.X,
                        _selectionStart.Y + delta.Y,
                        _selectionStart.Width,
                        _selectionStart.Height - delta.Y);
                    break;
                case "NE":
                    newRect = new Rect(
                        _selectionStart.X,
                        _selectionStart.Y + delta.Y,
                        _selectionStart.Width + delta.X,
                        _selectionStart.Height - delta.Y);
                    break;
                case "W":
                    newRect = new Rect(
                        _selectionStart.X + delta.X,
                        _selectionStart.Y,
                        _selectionStart.Width - delta.X,
                        _selectionStart.Height);
                    break;
                case "E":
                    newRect = new Rect(
                        _selectionStart.X,
                        _selectionStart.Y,
                        _selectionStart.Width + delta.X,
                        _selectionStart.Height);
                    break;
                case "SW":
                    newRect = new Rect(
                        _selectionStart.X + delta.X,
                        _selectionStart.Y,
                        _selectionStart.Width - delta.X,
                        _selectionStart.Height + delta.Y);
                    break;
                case "S":
                    newRect = new Rect(
                        _selectionStart.X,
                        _selectionStart.Y,
                        _selectionStart.Width,
                        _selectionStart.Height + delta.Y);
                    break;
                case "SE":
                    newRect = new Rect(
                        _selectionStart.X,
                        _selectionStart.Y,
                        _selectionStart.Width + delta.X,
                        _selectionStart.Height + delta.Y);
                    break;
            }

            // Ensure minimum size
            if (newRect.Width >= 10 && newRect.Height >= 10)
            {
                SetDisplayRect(ClampRect(newRect));
            }
        }

        private Rect ClampRect(Rect rect)
        {
            var (scale, offsetX, offsetY) = GetUniformScaleAndOffset();
            double displayedWidth = ImageWidth * scale;
            double displayedHeight = ImageHeight * scale;

            // Clamp to the actual image area, not the full control bounds
            double minX = offsetX;
            double minY = offsetY;
            double maxX = offsetX + displayedWidth;
            double maxY = offsetY + displayedHeight;

            double x = Math.Max(minX, Math.Min(rect.X, maxX - 10));
            double y = Math.Max(minY, Math.Min(rect.Y, maxY - 10));
            double w = Math.Max(10, Math.Min(rect.Width, maxX - x));
            double h = Math.Max(10, Math.Min(rect.Height, maxY - y));
            return new Rect(x, y, w, h);
        }

        /// <summary>
        /// Gets the uniform scale factor and offset for the image displayed with Stretch="Uniform".
        /// Returns (scale, offsetX, offsetY) where the image is centered in the available space.
        /// </summary>
        private (double scale, double offsetX, double offsetY) GetUniformScaleAndOffset()
        {
            if (ImageWidth <= 0 || ImageHeight <= 0 || Bounds.Width <= 0 || Bounds.Height <= 0)
                return (1, 0, 0);

            double scaleX = Bounds.Width / ImageWidth;
            double scaleY = Bounds.Height / ImageHeight;
            double scale = Math.Min(scaleX, scaleY);

            // Calculate offset to center the image
            double displayedWidth = ImageWidth * scale;
            double displayedHeight = ImageHeight * scale;
            double offsetX = (Bounds.Width - displayedWidth) / 2;
            double offsetY = (Bounds.Height - displayedHeight) / 2;

            return (scale, offsetX, offsetY);
        }

        private Rect GetDisplayRect()
        {
            if (ImageWidth <= 0 || ImageHeight <= 0) return new Rect(0, 0, Bounds.Width, Bounds.Height);

            var (scale, offsetX, offsetY) = GetUniformScaleAndOffset();

            return new Rect(
                offsetX + SelectionX * scale,
                offsetY + SelectionY * scale,
                SelectionWidth * scale,
                SelectionHeight * scale);
        }

        private void SetDisplayRect(Rect displayRect)
        {
            if (ImageWidth <= 0 || ImageHeight <= 0) return;

            var (scale, offsetX, offsetY) = GetUniformScaleAndOffset();
            if (scale <= 0) return;

            SelectionX = (int)Math.Round((displayRect.X - offsetX) / scale);
            SelectionY = (int)Math.Round((displayRect.Y - offsetY) / scale);
            SelectionWidth = (int)Math.Round(displayRect.Width / scale);
            SelectionHeight = (int)Math.Round(displayRect.Height / scale);

            UpdateVisuals();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == SelectionXProperty ||
                change.Property == SelectionYProperty ||
                change.Property == SelectionWidthProperty ||
                change.Property == SelectionHeightProperty ||
                change.Property == ImageWidthProperty ||
                change.Property == ImageHeightProperty ||
                change.Property == IsActiveProperty ||
                change.Property == BoundsProperty)
            {
                UpdateVisuals();
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var result = base.ArrangeOverride(finalSize);
            UpdateVisuals();
            return result;
        }

        private void UpdateVisuals()
        {
            bool show = IsActive && Bounds.Width > 0 && Bounds.Height > 0;

            SelectionBorder.IsVisible = show;
            SelectionBorderDashed.IsVisible = show;
            TopDarkArea.IsVisible = show;
            BottomDarkArea.IsVisible = show;
            LeftDarkArea.IsVisible = show;
            RightDarkArea.IsVisible = show;
            HandleNW.IsVisible = show;
            HandleN.IsVisible = show;
            HandleNE.IsVisible = show;
            HandleW.IsVisible = show;
            HandleE.IsVisible = show;
            HandleSW.IsVisible = show;
            HandleS.IsVisible = show;
            HandleSE.IsVisible = show;
            SizeDisplay.IsVisible = show;

            if (!show) return;

            var rect = GetDisplayRect();

            // Selection border
            Canvas.SetLeft(SelectionBorder, rect.X);
            Canvas.SetTop(SelectionBorder, rect.Y);
            SelectionBorder.Width = rect.Width;
            SelectionBorder.Height = rect.Height;

            // Dashed border (inside main border)
            Canvas.SetLeft(SelectionBorderDashed, rect.X + 2);
            Canvas.SetTop(SelectionBorderDashed, rect.Y + 2);
            SelectionBorderDashed.Width = Math.Max(0, rect.Width - 4);
            SelectionBorderDashed.Height = Math.Max(0, rect.Height - 4);

            // Dark overlays
            Canvas.SetLeft(TopDarkArea, 0);
            Canvas.SetTop(TopDarkArea, 0);
            TopDarkArea.Width = Bounds.Width;
            TopDarkArea.Height = rect.Y;

            Canvas.SetLeft(BottomDarkArea, 0);
            Canvas.SetTop(BottomDarkArea, rect.Y + rect.Height);
            BottomDarkArea.Width = Bounds.Width;
            BottomDarkArea.Height = Bounds.Height - rect.Y - rect.Height;

            Canvas.SetLeft(LeftDarkArea, 0);
            Canvas.SetTop(LeftDarkArea, rect.Y);
            LeftDarkArea.Width = rect.X;
            LeftDarkArea.Height = rect.Height;

            Canvas.SetLeft(RightDarkArea, rect.X + rect.Width);
            Canvas.SetTop(RightDarkArea, rect.Y);
            RightDarkArea.Width = Bounds.Width - rect.X - rect.Width;
            RightDarkArea.Height = rect.Height;

            // Handles
            double hx = rect.X, hy = rect.Y, hw = rect.Width, hh = rect.Height;
            PositionHandle(HandleNW, hx - 5, hy - 5);
            PositionHandle(HandleN, hx + hw / 2 - 5, hy - 5);
            PositionHandle(HandleNE, hx + hw - 5, hy - 5);
            PositionHandle(HandleW, hx - 5, hy + hh / 2 - 5);
            PositionHandle(HandleE, hx + hw - 5, hy + hh / 2 - 5);
            PositionHandle(HandleSW, hx - 5, hy + hh - 5);
            PositionHandle(HandleS, hx + hw / 2 - 5, hy + hh - 5);
            PositionHandle(HandleSE, hx + hw - 5, hy + hh - 5);

            // Size display
            SizeText.Text = $"{SelectionWidth} × {SelectionHeight}";
            Canvas.SetLeft(SizeDisplay, rect.X + rect.Width / 2 - 40);
            Canvas.SetTop(SizeDisplay, rect.Y + rect.Height + 5);
        }

        private void PositionHandle(Border handle, double x, double y)
        {
            Canvas.SetLeft(handle, x);
            Canvas.SetTop(handle, y);
        }
    }
}
