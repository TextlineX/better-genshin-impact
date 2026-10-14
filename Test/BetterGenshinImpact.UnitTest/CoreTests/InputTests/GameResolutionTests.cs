using BetterGenshinImpact.Helpers;

namespace BetterGenshinImpact.UnitTest.CoreTests.InputTests;

public class GameResolutionTests
{
    [Theory]
    [InlineData(1920, 1080, true)]
    [InlineData(2560, 1440, true)]
    [InlineData(1920, 1200, true)]
    [InlineData(2560, 1600, true)]
    [InlineData(3440, 1440, false)]
    [InlineData(1280, 1024, false)]
    public void SupportedAspectRatio_Recognizes16By9And16By10(int width, int height, bool expected)
    {
        Assert.Equal(expected, AssertUtils.IsSupportedGameAspectRatio(width, height));
    }

    [Fact]
    public void Is16By10_RecognizesConfiguredResolutions()
    {
        Assert.True(AssertUtils.Is16By10(1920, 1200));
        Assert.True(AssertUtils.Is16By10(2560, 1600));
        Assert.False(AssertUtils.Is16By10(1920, 1080));
    }
}
