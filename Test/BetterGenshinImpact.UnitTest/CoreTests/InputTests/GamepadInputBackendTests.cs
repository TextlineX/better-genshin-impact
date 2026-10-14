using BetterGenshinImpact.Core.Config;
using BetterGenshinImpact.Core.Input.Backends.Gamepad;
using BetterGenshinImpact.Core.Simulator.Extensions;
using System.Collections.Generic;
using Vanara.PInvoke;

namespace BetterGenshinImpact.UnitTest.CoreTests.InputTests;

public class GamepadInputBackendTests
{
    [Fact]
    public void SimulateAction_TracksButtonPressAndRelease()
    {
        var device = new RecordingGamepadDevice();
        using var backend = new GamepadInputBackend(device);

        backend.Foreground.SimulateAction(GIActions.Jump, KeyType.KeyDown);
        Assert.True(device.Buttons[GamepadButton.A]);

        backend.Foreground.SimulateAction(GIActions.Jump, KeyType.KeyUp);
        Assert.False(device.Buttons[GamepadButton.A]);
    }

    [Fact]
    public void DirectKeyboardVk_UsesConfiguredActionInsteadOfTreatingVkAsButton()
    {
        var device = new RecordingGamepadDevice();
        using var backend = new GamepadInputBackend(device, keyBindings: new KeyBindingsConfig());

        backend.Foreground.Keyboard.KeyPress(User32.VK.VK_F);

        Assert.Equal(1, device.ButtonPresses[GamepadButton.X]);
        Assert.DoesNotContain(GamepadButton.A, device.ButtonPresses.Keys);
    }

    [Fact]
    public void ReleaseAll_ClearsButtonsAxesAndTriggers()
    {
        var device = new RecordingGamepadDevice();
        using var backend = new GamepadInputBackend(device);

        backend.Foreground.SimulateAction(GIActions.MoveForward, KeyType.KeyDown);
        backend.Foreground.SimulateAction(GIActions.NormalAttack, KeyType.KeyDown);
        backend.ReleaseAll();

        Assert.True(device.ResetCount > 0);
        Assert.Equal(0, device.Axes[GamepadAxis.LeftThumbY]);
        Assert.Equal(0, device.Triggers[GamepadTrigger.Right]);
    }

    [Fact]
    public void DefaultMapping_UsesXboxControlsForCoreGameActions()
    {
        var mapping = GamepadMappingConfig.CreateDefault();

        Assert.Equal(GamepadButton.A, mapping.Actions[GIActions.Jump].Button);
        Assert.Equal(GamepadButton.X, mapping.Actions[GIActions.PickUpOrInteract].Button);
        Assert.Equal(GamepadButton.A, mapping.Actions[GIActions.MenuConfirm].Button);
        Assert.Equal(GamepadButton.B, mapping.Actions[GIActions.MenuCancel].Button);
        Assert.Equal(GamepadTrigger.Right, mapping.Actions[GIActions.NormalAttack].Trigger);
        Assert.Equal(GamepadAxis.LeftThumbY, mapping.Actions[GIActions.MoveForward].Axis);
    }

    [Fact]
    public void CustomMapping_IsUsedForActionsWithoutTreatingItAsKeyboardInput()
    {
        var mapping = GamepadMappingConfig.CreateDefault();
        mapping.Actions[GIActions.NormalAttack] = GamepadControl.ButtonControl(GamepadButton.X);
        mapping.Actions[GIActions.PickUpOrInteract] = GamepadControl.TriggerControl(GamepadTrigger.Right);

        var device = new RecordingGamepadDevice();
        using var backend = new GamepadInputBackend(device, mapping);

        backend.Foreground.SimulateAction(GIActions.NormalAttack);
        Assert.Equal(1, device.ButtonPresses[GamepadButton.X]);
        Assert.DoesNotContain(GamepadButton.A, device.ButtonPresses.Keys);

        backend.Foreground.SimulateAction(GIActions.PickUpOrInteract, KeyType.KeyDown);
        Assert.Equal(byte.MaxValue, device.Triggers[GamepadTrigger.Right]);
    }

    private sealed class RecordingGamepadDevice : IGamepadDevice
    {
        public Dictionary<GamepadButton, bool> Buttons { get; } = new();
        public Dictionary<GamepadButton, int> ButtonPresses { get; } = new();
        public Dictionary<GamepadAxis, short> Axes { get; } = new();
        public Dictionary<GamepadTrigger, byte> Triggers { get; } = new();
        public int ResetCount { get; private set; }

        public void SetButton(GamepadButton button, bool pressed)
        {
            Buttons[button] = pressed;
            if (pressed)
            {
                ButtonPresses[button] = ButtonPresses.GetValueOrDefault(button) + 1;
            }
        }

        public void SetAxis(GamepadAxis axis, short value) => Axes[axis] = value;

        public void SetTrigger(GamepadTrigger trigger, byte value) => Triggers[trigger] = value;

        public void Reset()
        {
            ResetCount++;
            foreach (var button in Enum.GetValues<GamepadButton>())
            {
                Buttons[button] = false;
            }

            foreach (var axis in Enum.GetValues<GamepadAxis>())
            {
                Axes[axis] = 0;
            }

            foreach (var trigger in Enum.GetValues<GamepadTrigger>())
            {
                Triggers[trigger] = 0;
            }
        }

        public void Dispose() => Reset();
    }
}
