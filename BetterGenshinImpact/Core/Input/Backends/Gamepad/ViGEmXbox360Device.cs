using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using System;

namespace BetterGenshinImpact.Core.Input.Backends.Gamepad;

/// <summary>
/// ViGEm.NET 的 Xbox 360 虚拟控制器适配器。ViGEmBus 必须由用户单独安装。
/// </summary>
public sealed class ViGEmXbox360Device : IGamepadDevice
{
    private readonly ViGEmClient _client;
    private readonly IXbox360Controller _controller;
    private bool _disposed;

    public ViGEmXbox360Device()
    {
        _client = new ViGEmClient();
        try
        {
            _controller = _client.CreateXbox360Controller();
            _controller.Connect();
        }
        catch
        {
            _client.Dispose();
            throw;
        }
    }

    public void SetButton(GamepadButton button, bool pressed) =>
        _controller.SetButtonState(ToViGEmButton(button), pressed);

    public void SetAxis(GamepadAxis axis, short value) =>
        _controller.SetAxisValue(ToViGEmAxis(axis), value);

    public void SetTrigger(GamepadTrigger trigger, byte value) =>
        _controller.SetSliderValue(ToViGEmSlider(trigger), value);

    public void Reset()
    {
        foreach (var button in Enum.GetValues<GamepadButton>())
        {
            _controller.SetButtonState(ToViGEmButton(button), false);
        }

        foreach (var axis in Enum.GetValues<GamepadAxis>())
        {
            _controller.SetAxisValue(ToViGEmAxis(axis), 0);
        }

        _controller.SetSliderValue(Xbox360Slider.LeftTrigger, 0);
        _controller.SetSliderValue(Xbox360Slider.RightTrigger, 0);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        try
        {
            Reset();
            _controller.Disconnect();
        }
        finally
        {
            _client.Dispose();
        }
    }

    private static Xbox360Button ToViGEmButton(GamepadButton button) => button switch
    {
        GamepadButton.A => Xbox360Button.A,
        GamepadButton.B => Xbox360Button.B,
        GamepadButton.X => Xbox360Button.X,
        GamepadButton.Y => Xbox360Button.Y,
        GamepadButton.LeftBumper => Xbox360Button.LeftShoulder,
        GamepadButton.RightBumper => Xbox360Button.RightShoulder,
        GamepadButton.Back => Xbox360Button.Back,
        GamepadButton.Start => Xbox360Button.Start,
        GamepadButton.DPadUp => Xbox360Button.Up,
        GamepadButton.DPadRight => Xbox360Button.Right,
        GamepadButton.DPadDown => Xbox360Button.Down,
        GamepadButton.DPadLeft => Xbox360Button.Left,
        GamepadButton.LeftThumb => Xbox360Button.LeftThumb,
        GamepadButton.RightThumb => Xbox360Button.RightThumb,
        _ => throw new ArgumentOutOfRangeException(nameof(button), button, null),
    };

    private static Xbox360Axis ToViGEmAxis(GamepadAxis axis) => axis switch
    {
        GamepadAxis.LeftThumbX => Xbox360Axis.LeftThumbX,
        GamepadAxis.LeftThumbY => Xbox360Axis.LeftThumbY,
        GamepadAxis.RightThumbX => Xbox360Axis.RightThumbX,
        GamepadAxis.RightThumbY => Xbox360Axis.RightThumbY,
        _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null),
    };

    private static Xbox360Slider ToViGEmSlider(GamepadTrigger trigger) => trigger switch
    {
        GamepadTrigger.Left => Xbox360Slider.LeftTrigger,
        GamepadTrigger.Right => Xbox360Slider.RightTrigger,
        _ => throw new ArgumentOutOfRangeException(nameof(trigger), trigger, null),
    };
}
