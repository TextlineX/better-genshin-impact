using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace BetterGenshinImpact.Core.Input.Backends.Gamepad;

/// <summary>
/// Reads physical XInput controllers for the mapping editor. This is input discovery only;
/// output is still sent through the ViGEm virtual controller.
/// </summary>
internal static class XInputGamepadReader
{
    private const uint ErrorSuccess = 0;
    private const ushort DPadUp = 0x0001;
    private const ushort DPadDown = 0x0002;
    private const ushort DPadLeft = 0x0004;
    private const ushort DPadRight = 0x0008;
    private const ushort Start = 0x0010;
    private const ushort Back = 0x0020;
    private const ushort LeftThumb = 0x0040;
    private const ushort RightThumb = 0x0080;
    private const ushort LeftBumper = 0x0100;
    private const ushort RightBumper = 0x0200;
    private const ushort A = 0x1000;
    private const ushort B = 0x2000;
    private const ushort X = 0x4000;
    private const ushort Y = 0x8000;
    private const short AxisThreshold = 12000;
    private const byte TriggerThreshold = 128;

    public static async Task<GamepadControl?> CaptureAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var baseline = ReadAll();
        if (baseline.Count == 0)
        {
            // A controller may be connected just after the window opens. The polling loop
            // below will still pick it up, while a missing baseline is treated as neutral.
            baseline = new Dictionary<int, XInputState>();
        }

        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var (index, current) in ReadAll())
            {
                baseline.TryGetValue(index, out var previous);
                if (TryDetectButton(previous.Buttons, current.Buttons, out var button))
                {
                    return GamepadControl.ButtonControl(button);
                }

                if (previous.LeftTrigger < TriggerThreshold && current.LeftTrigger >= TriggerThreshold)
                {
                    return GamepadControl.TriggerControl(GamepadTrigger.Left);
                }

                if (previous.RightTrigger < TriggerThreshold && current.RightTrigger >= TriggerThreshold)
                {
                    return GamepadControl.TriggerControl(GamepadTrigger.Right);
                }

                if (TryDetectAxis(previous.LeftThumbX, current.LeftThumbX, GamepadAxis.LeftThumbX, out var leftX)
                    || TryDetectAxis(previous.LeftThumbY, current.LeftThumbY, GamepadAxis.LeftThumbY, out leftX)
                    || TryDetectAxis(previous.RightThumbX, current.RightThumbX, GamepadAxis.RightThumbX, out leftX)
                    || TryDetectAxis(previous.RightThumbY, current.RightThumbY, GamepadAxis.RightThumbY, out leftX))
                {
                    return leftX;
                }
            }

            await Task.Delay(50, cancellationToken).ConfigureAwait(true);
        }

        return null;
    }

    private static bool TryDetectButton(ushort previous, ushort current, out GamepadButton button)
    {
        var pressed = (ushort)(current & ~previous);
        foreach (var candidate in ButtonMap)
        {
            if ((pressed & candidate.Mask) != 0)
            {
                button = candidate.Button;
                return true;
            }
        }

        button = default;
        return false;
    }

    private static bool TryDetectAxis(short previous, short current, GamepadAxis axis, out GamepadControl control)
    {
        if (Math.Abs(current - previous) < AxisThreshold || Math.Abs(current) < AxisThreshold)
        {
            control = null!;
            return false;
        }

        control = GamepadControl.AxisControl(axis, current >= 0 ? short.MaxValue : short.MinValue);
        return true;
    }

    private static Dictionary<int, XInputState> ReadAll()
    {
        var result = new Dictionary<int, XInputState>();
        for (var index = 0; index < 4; index++)
        {
            if (TryRead(index, out var state))
            {
                result[index] = state;
            }
        }

        return result;
    }

    private static bool TryRead(int index, out XInputState state)
    {
        state = default;
        try
        {
            if (XInputGetState((uint)index, out var nativeState) != ErrorSuccess)
            {
                return false;
            }

            state = new XInputState(
                nativeState.Gamepad.Buttons,
                nativeState.Gamepad.LeftTrigger,
                nativeState.Gamepad.RightTrigger,
                nativeState.Gamepad.LeftThumbX,
                nativeState.Gamepad.LeftThumbY,
                nativeState.Gamepad.RightThumbX,
                nativeState.Gamepad.RightThumbY);
            return true;
        }
        catch (DllNotFoundException)
        {
            return false;
        }
        catch (EntryPointNotFoundException)
        {
            return false;
        }
    }

    private static readonly ButtonCandidate[] ButtonMap =
    [
        new(DPadUp, GamepadButton.DPadUp),
        new(DPadRight, GamepadButton.DPadRight),
        new(DPadDown, GamepadButton.DPadDown),
        new(DPadLeft, GamepadButton.DPadLeft),
        new(Start, GamepadButton.Start),
        new(Back, GamepadButton.Back),
        new(LeftThumb, GamepadButton.LeftThumb),
        new(RightThumb, GamepadButton.RightThumb),
        new(LeftBumper, GamepadButton.LeftBumper),
        new(RightBumper, GamepadButton.RightBumper),
        new(A, GamepadButton.A),
        new(B, GamepadButton.B),
        new(X, GamepadButton.X),
        new(Y, GamepadButton.Y),
    ];

    private readonly record struct ButtonCandidate(ushort Mask, GamepadButton Button);

    private readonly record struct XInputState(
        ushort Buttons,
        byte LeftTrigger,
        byte RightTrigger,
        short LeftThumbX,
        short LeftThumbY,
        short RightThumbX,
        short RightThumbY);

    [DllImport("xinput1_4.dll", EntryPoint = "XInputGetState")]
    private static extern uint XInputGetState(uint userIndex, out NativeState state);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeState
    {
        public uint PacketNumber;
        public NativeGamepad Gamepad;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeGamepad
    {
        public ushort Buttons;
        public byte LeftTrigger;
        public byte RightTrigger;
        public short LeftThumbX;
        public short LeftThumbY;
        public short RightThumbX;
        public short RightThumbY;
    }
}
