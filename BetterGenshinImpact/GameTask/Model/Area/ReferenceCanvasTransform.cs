using BetterGenshinImpact.Core.Recognition;
using OpenCvSharp;
using System;

namespace BetterGenshinImpact.GameTask.Model.Area;

/// <summary>
/// 把 1920x1080 参考画布映射到当前截图。映射保持宽高比，剩余区域按锚点放置。
/// 这套规则同时用于识别 ROI 和需要点击的参考坐标，避免各任务各自处理 16:10 留白。
/// </summary>
public readonly struct ReferenceCanvasTransform
{
    public const int ReferenceWidth = 1920;
    public const int ReferenceHeight = 1080;

    public ReferenceCanvasTransform(int width, int height, SearchAnchorMode anchor = SearchAnchorMode.TopLeft)
    {
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "截图尺寸必须为正数。");
        }

        CaptureSize = new Size(width, height);
        Scale = Math.Min(width / (double)ReferenceWidth, height / (double)ReferenceHeight);
        var contentWidth = ReferenceWidth * Scale;
        var contentHeight = ReferenceHeight * Scale;
        (OffsetX, OffsetY) = anchor switch
        {
            SearchAnchorMode.TopRight => (width - contentWidth, 0d),
            SearchAnchorMode.BottomLeft => (0d, height - contentHeight),
            SearchAnchorMode.BottomRight => (width - contentWidth, height - contentHeight),
            SearchAnchorMode.Center => ((width - contentWidth) / 2d, (height - contentHeight) / 2d),
            _ => (0d, 0d),
        };
    }

    public Size CaptureSize { get; }

    public double Scale { get; }

    public double OffsetX { get; }

    public double OffsetY { get; }

    public Point MapPoint(double x, double y)
    {
        return new Point(
            (int)Math.Round(OffsetX + x * Scale),
            (int)Math.Round(OffsetY + y * Scale));
    }

    public Rect MapRect(Rect referenceRect, bool clamp = true)
    {
        var left = (int)Math.Round(OffsetX + referenceRect.Left * Scale);
        var top = (int)Math.Round(OffsetY + referenceRect.Top * Scale);
        var right = (int)Math.Round(OffsetX + referenceRect.Right * Scale);
        var bottom = (int)Math.Round(OffsetY + referenceRect.Bottom * Scale);
        var result = new Rect(left, top, Math.Max(1, right - left), Math.Max(1, bottom - top));
        return clamp ? Clamp(result, CaptureSize) : result;
    }

    public static Rect Clamp(Rect rect, Size bounds)
    {
        var left = Math.Clamp(rect.Left, 0, bounds.Width);
        var top = Math.Clamp(rect.Top, 0, bounds.Height);
        var right = Math.Clamp(rect.Right, 0, bounds.Width);
        var bottom = Math.Clamp(rect.Bottom, 0, bounds.Height);
        return new Rect(left, top, Math.Max(0, right - left), Math.Max(0, bottom - top));
    }
}
