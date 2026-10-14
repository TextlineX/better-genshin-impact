using BetterGenshinImpact.Core.Recognition;
using BetterGenshinImpact.GameTask.Model.Area;
using OpenCvSharp;

namespace BetterGenshinImpact.UnitTest.GameTaskTests;

public class ReferenceCanvasTransformTests
{
    public static TheoryData<int, int, SearchAnchorMode, Rect> RectCases => new()
    {
        { 1920, 1080, SearchAnchorMode.TopLeft, new Rect(100, 200, 300, 400) },
        { 2560, 1440, SearchAnchorMode.TopRight, new Rect(133, 267, 400, 533) },
        { 1920, 1200, SearchAnchorMode.TopLeft, new Rect(100, 200, 300, 400) },
        { 1920, 1200, SearchAnchorMode.Center, new Rect(100, 260, 300, 400) },
        { 2560, 1600, SearchAnchorMode.BottomRight, new Rect(133, 427, 400, 533) },
    };

    [Theory]
    [MemberData(nameof(RectCases))]
    public void MapRect_UsesUniformScaleAndAnchor(int width, int height, SearchAnchorMode anchor, Rect expected)
    {
        var transform = new ReferenceCanvasTransform(width, height, anchor);

        var actual = transform.MapRect(new Rect(100, 200, 300, 400));

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(1920, 1080)]
    [InlineData(2560, 1440)]
    [InlineData(1920, 1200)]
    [InlineData(2560, 1600)]
    public void MapRect_AlwaysStaysInsideCapture(int width, int height)
    {
        var transform = new ReferenceCanvasTransform(width, height, SearchAnchorMode.Center);

        var actual = transform.MapRect(new Rect(-300, -200, 2600, 1600));

        Assert.True(actual.X >= 0 && actual.Y >= 0);
        Assert.True(actual.Right <= width && actual.Bottom <= height);
    }

    [Fact]
    public void MapPoint_Maps16To10CenterWithoutStretching()
    {
        var transform = new ReferenceCanvasTransform(1920, 1200, SearchAnchorMode.Center);

        Assert.Equal(new Point(960, 600), transform.MapPoint(960, 540));
    }
}
