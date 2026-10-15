using BetterGenshinImpact.GameTask;
using BetterGenshinImpact.GameTask.AutoSkip;
using BetterGenshinImpact.GameTask.Common.BgiVision;

namespace BetterGenshinImpact.UnitTest.GameTaskTests.AutoSkipTests;

public class AutoSkipSafetyTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void GamepadTalkMarker_RequiresDialogueSpecificMarker(
        bool hasDisabledUiButton,
        bool expected)
    {
        Assert.Equal(expected, Bv.HasReliableGamepadTalkMarker(hasDisabledUiButton));
    }

    [Theory]
    [InlineData(false, false, GamepadDialogueState.None)]
    [InlineData(false, true, GamepadDialogueState.None)]
    [InlineData(true, false, GamepadDialogueState.Advance)]
    [InlineData(true, true, GamepadDialogueState.Option)]
    public void GamepadDialogueState_SeparatesSceneAndOptionMarkers(
        bool hasDialogueMarker,
        bool hasOptionMarker,
        GamepadDialogueState expected)
    {
        Assert.Equal(expected, Bv.ResolveGamepadDialogueState(hasDialogueMarker, hasOptionMarker));
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
