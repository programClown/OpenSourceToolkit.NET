using System;

namespace OpenSourceToolkit.NET.Services
{
    public static class WindowGeometry
    {
        /// <summary>
        /// Checks if two axis-aligned rectangles overlap.
        /// </summary>
        public static bool RectanglesOverlap(int x1, int y1, int w1, int h1, int x2, int y2, int w2, int h2)
        {
            return x1 < x2 + w2 && x1 + w1 > x2 && y1 < y2 + h2 && y1 + h1 > y2;
        }

        /// <summary>
        /// Clamps inner rectangle position so it fits entirely within outer rectangle.
        /// Returns the adjusted (x, y) position.
        /// </summary>
        public static (int X, int Y) ClampRectangleInside(int x, int y, int w, int h, int sx, int sy, int sw, int sh)
        {
            var clampedX = Math.Max(sx, Math.Min(x, sx + sw - w));
            var clampedY = Math.Max(sy, Math.Min(y, sy + sh - h));
            return (clampedX, clampedY);
        }
    }
}
