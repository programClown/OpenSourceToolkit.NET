#!/usr/bin/env python3
"""Generate app icon for OpenSourceToolkit.NET"""

from PIL import Image, ImageDraw
import math
import os

def create_icon(size: int) -> Image.Image:
    """Create a toolbox icon with blue gradient."""
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Calculate dimensions
    padding = size * 0.1
    corner_radius = size * 0.2

    # Draw rounded rectangle background with gradient effect
    # Blue colors: #1D4ED8 to #3B82F6
    for y in range(size):
        # Gradient from top-left to bottom-right
        t = (y / size + (y / size)) / 2  # diagonal gradient
        t = min(1.0, max(0.0, t))

        r = int(29 + (59 - 29) * t)    # 1D4ED8 -> 3B82F6
        g = int(78 + (130 - 78) * t)
        b = int(216 + (246 - 216) * t)

        for x in range(size):
            # Check if inside rounded rectangle
            in_rect = True

            # Check corners
            if x < corner_radius and y < corner_radius:
                # Top-left corner
                if (x - corner_radius) ** 2 + (y - corner_radius) ** 2 > corner_radius ** 2:
                    in_rect = False
            elif x > size - corner_radius and y < corner_radius:
                # Top-right corner
                if (x - (size - corner_radius)) ** 2 + (y - corner_radius) ** 2 > corner_radius ** 2:
                    in_rect = False
            elif x < corner_radius and y > size - corner_radius:
                # Bottom-left corner
                if (x - corner_radius) ** 2 + (y - (size - corner_radius)) ** 2 > corner_radius ** 2:
                    in_rect = False
            elif x > size - corner_radius and y > size - corner_radius:
                # Bottom-right corner
                if (x - (size - corner_radius)) ** 2 + (y - (size - corner_radius)) ** 2 > corner_radius ** 2:
                    in_rect = False

            if in_rect:
                # Diagonal gradient
                diag_t = (x + y) / (2 * size)
                r = int(29 + (59 - 29) * diag_t)
                g = int(78 + (130 - 78) * diag_t)
                b = int(216 + (246 - 216) * diag_t)
                img.putpixel((x, y), (r, g, b, 255))

    # Draw toolbox icon (simplified)
    icon_padding = size * 0.2
    icon_size = size - 2 * icon_padding

    # Toolbox body
    box_top = int(icon_padding + icon_size * 0.25)
    box_bottom = int(size - icon_padding)
    box_left = int(icon_padding)
    box_right = int(size - icon_padding)

    # Draw toolbox outline
    line_width = max(2, int(size * 0.06))

    # Main box
    draw.rectangle([box_left, box_top, box_right, box_bottom], outline='white', width=line_width)

    # Handle on top
    handle_width = icon_size * 0.4
    handle_left = int(size / 2 - handle_width / 2)
    handle_right = int(size / 2 + handle_width / 2)
    handle_top = int(icon_padding)
    handle_bottom = int(box_top + line_width)

    draw.rectangle([handle_left, handle_top, handle_right, handle_bottom], outline='white', width=line_width)

    # Middle divider line
    mid_y = int((box_top + box_bottom) / 2)
    draw.line([box_left, mid_y, box_right, mid_y], fill='white', width=line_width)

    # Latch in center
    latch_size = int(icon_size * 0.15)
    latch_left = int(size / 2 - latch_size / 2)
    latch_right = int(size / 2 + latch_size / 2)
    latch_top = int(mid_y - latch_size / 2)
    latch_bottom = int(mid_y + latch_size / 2)
    draw.rectangle([latch_left, latch_top, latch_right, latch_bottom], fill='white')

    return img


def main():
    # Generate icons at multiple sizes
    sizes = [16, 24, 32, 48, 64, 128, 256]
    images = [create_icon(s) for s in sizes]

    # Save as ICO
    script_dir = os.path.dirname(os.path.abspath(__file__))
    ico_path = os.path.join(script_dir, 'toolkit-logo.ico')

    # Save with multiple sizes embedded
    images[0].save(
        ico_path,
        format='ICO',
        sizes=[(s, s) for s in sizes],
        append_images=images[1:]
    )

    print(f"Icon saved to: {ico_path}")

    # Also save a PNG for reference
    png_path = os.path.join(script_dir, 'toolkit-logo.png')
    create_icon(256).save(png_path, format='PNG')
    print(f"PNG saved to: {png_path}")


if __name__ == '__main__':
    main()
