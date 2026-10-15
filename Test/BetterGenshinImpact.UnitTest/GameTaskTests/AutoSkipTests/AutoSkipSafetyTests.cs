using BetterGenshinImpact.GameTask;
using BetterGenshinImpact.GameTask.AutoSkip;
using BetterGenshinImpact.GameTask.Common.BgiVision;

namespace BetterGenshinImpact.UnitTest.GameTaskTests.AutoSkipTests;

public class AutoSkipSafetyTests
{
    [Theory]
    [InlineData(false, false, false, false)]
    [InlineData(true, false, false, false)]
    [InlineData(false, true, true, false)]
    [InlineData(false, false, true, false)]
    [InlineData(true, true, false, true)]
    [InlineData(true, false, true, true)]
    public void GamepadTalkMarker_RequiresDialogueSpecificMarker(
        bool hasDisabledUiButton,
        bool hasDialogueConfirm,
        bool hasOptionIcon,
        bool expected)
    {
        Assert.Equal(expected, Bv.HasReliableGamepadTalkMarker(
            hasDisabledUiButton,
            hasDialogueConfirm,
            hasOptionIcon));
    }

    [Fact]
    public void TriggerUiFilter_DoesNotRunTalkTriggerJustBecauseUiChanged()
    {
        Assert.False(TaskTriggerDispatcher.ShouldRunTrigger(
            GameUiCategory.Talk,
            runWhenUiUnknown: false,
            currentUi: GameUiCategory.Unknown));

        Assert.True(TaskTriggerDispatcher.ShouldRunTrigger(
            GameUiCategory.Talk,
            runWhenUiUnknown: true,
            currentUi: GameUiCategory.Unknown));
    }
}
