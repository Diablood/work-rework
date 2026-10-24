using System;

namespace WorkRework;

internal readonly record struct ScrollViewport(bool Horizontal, bool Vertical, float Width, float Height)
{
    internal static ScrollViewport Calculate(float contentWidth, float contentHeight, float width, float height,
        float horizontalBarHeight, float verticalBarWidth, bool allowHorizontal = true)
    {
        bool horizontal = false;
        bool vertical = false;
        // Start with no bars to find the smallest stable layout. Reserving either
        // bar unconditionally can otherwise make both appear on exact-fit tables.
        for (int pass = 0; pass < 3; pass++)
        {
            bool nextHorizontal = allowHorizontal && contentWidth > Math.Max(0f, width - (vertical ? verticalBarWidth : 0f)) + 1f;
            bool nextVertical = contentHeight > Math.Max(0f, height - (nextHorizontal ? horizontalBarHeight : 0f)) + 1f;
            if (horizontal == nextHorizontal && vertical == nextVertical) break;
            horizontal = nextHorizontal;
            vertical = nextVertical;
        }
        return new ScrollViewport(horizontal, vertical, Math.Max(0f, width - (vertical ? verticalBarWidth : 0f)),
            Math.Max(0f, height - (horizontal ? horizontalBarHeight : 0f)));
    }
}
