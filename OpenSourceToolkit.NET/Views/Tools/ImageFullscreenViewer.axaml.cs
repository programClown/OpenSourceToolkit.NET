using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System;

namespace OpenSourceToolkit.NET.Views.Tools
{
    public partial class ImageFullscreenViewer : Window
    {
        private ScaleTransform _scaleTransform;
        private TranslateTransform _translateTransform;
        private Point _lastPanPosition;
        private bool _isPanning;
        private double _currentZoom = 1.0;
        private const double MinZoom = 0.1;
        private const double MaxZoom = 10.0;
        private const double ZoomStep = 0.15;
        private DispatcherTimer _hintTimer;

        public ImageFullscreenViewer()
        {
            InitializeComponent();

            // Get transforms from the Image's RenderTransform (DisplayImage is source-generated)
            if (DisplayImage.RenderTransform is TransformGroup transformGroup)
            {
                foreach (var transform in transformGroup.Children)
                {
                    if (transform is ScaleTransform scale)
                        _scaleTransform = scale;
                    else if (transform is TranslateTransform translate)
                        _translateTransform = translate;
                }
            }

            // Handle keyboard input
            KeyDown += OnKeyDown;

            // Start timer to fade out hint text
            _hintTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _hintTimer.Tick += (s, e) =>
            {
                _hintTimer.Stop();
                HintText.Opacity = 0;
            };
            _hintTimer.Start();
        }

        /// <summary>
        /// Sets the image to display.
        /// </summary>
        public void SetImage(Bitmap image)
        {
            DisplayImage.Source = image;
        }

        /// <summary>
        /// Sets the image from raw bytes.
        /// </summary>
        public void SetImageFromBytes(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0) return;

            using (var ms = new System.IO.MemoryStream(imageBytes))
            {
                var bitmap = new Bitmap(ms);
                SetImage(bitmap);
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.Enter)
            {
                Close();
                e.Handled = true;
            }
            // Reset zoom with 0 or Home
            else if (e.Key == Key.D0 || e.Key == Key.NumPad0 || e.Key == Key.Home)
            {
                ResetZoom();
                e.Handled = true;
            }
            // Zoom in with +
            else if (e.Key == Key.Add || e.Key == Key.OemPlus)
            {
                ZoomIn();
                e.Handled = true;
            }
            // Zoom out with -
            else if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
            {
                ZoomOut();
                e.Handled = true;
            }
        }

        private void OnPointerWheelChanged(object sender, PointerWheelEventArgs e)
        {
            // Get mouse position relative to the container (ImageContainer is source-generated)
            var mousePos = e.GetPosition(ImageContainer);

            // Calculate new zoom
            double zoomDelta = e.Delta.Y > 0 ? ZoomStep : -ZoomStep;
            double newZoom = Math.Max(MinZoom, Math.Min(MaxZoom, _currentZoom + zoomDelta * _currentZoom));

            if (Math.Abs(newZoom - _currentZoom) < 0.001) return;

            // Calculate the point under the mouse before zoom
            double beforeX = (mousePos.X - _translateTransform.X) / _currentZoom;
            double beforeY = (mousePos.Y - _translateTransform.Y) / _currentZoom;

            // Apply new zoom
            _currentZoom = newZoom;
            _scaleTransform.ScaleX = _currentZoom;
            _scaleTransform.ScaleY = _currentZoom;

            // Calculate the point under the mouse after zoom and adjust translation
            double afterX = beforeX * _currentZoom;
            double afterY = beforeY * _currentZoom;

            _translateTransform.X = mousePos.X - afterX;
            _translateTransform.Y = mousePos.Y - afterY;

            e.Handled = true;
        }

        private void OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            var props = e.GetCurrentPoint(ImageContainer).Properties;
            if (props.IsLeftButtonPressed)
            {
                _isPanning = true;
                _lastPanPosition = e.GetPosition(ImageContainer);
                e.Pointer.Capture(ImageContainer);
                e.Handled = true;
            }
        }

        private void OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (_isPanning)
            {
                _isPanning = false;
                e.Pointer.Capture(null);
                e.Handled = true;
            }
        }

        private void OnPointerMoved(object sender, PointerEventArgs e)
        {
            if (!_isPanning) return;

            var currentPos = e.GetPosition(ImageContainer);
            var delta = currentPos - _lastPanPosition;

            _translateTransform.X += delta.X;
            _translateTransform.Y += delta.Y;

            _lastPanPosition = currentPos;
            e.Handled = true;
        }

        private void ResetZoom()
        {
            _currentZoom = 1.0;
            _scaleTransform.ScaleX = 1.0;
            _scaleTransform.ScaleY = 1.0;
            _translateTransform.X = 0;
            _translateTransform.Y = 0;
        }

        private void ZoomIn()
        {
            _currentZoom = Math.Min(MaxZoom, _currentZoom * (1 + ZoomStep));
            _scaleTransform.ScaleX = _currentZoom;
            _scaleTransform.ScaleY = _currentZoom;
        }

        private void ZoomOut()
        {
            _currentZoom = Math.Max(MinZoom, _currentZoom * (1 - ZoomStep));
            _scaleTransform.ScaleX = _currentZoom;
            _scaleTransform.ScaleY = _currentZoom;
        }

        protected override void OnClosed(EventArgs e)
        {
            _hintTimer?.Stop();
            base.OnClosed(e);
        }
    }
}
