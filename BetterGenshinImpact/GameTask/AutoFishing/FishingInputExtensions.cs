using BetterGenshinImpact.Core.Input;
using BetterGenshinImpact.Core.Input.Backends.Gamepad;
using BetterGenshinImpact.Core.Simulator.Extensions;
using Vanara.PInvoke;

namespace BetterGenshinImpact.GameTask.AutoFishing;

internal static class FishingInputExtensions
{
    public static void FishingPrimaryDown(this IInputChannel input)
    {
        if (InputHub.Backend.Kind == InputBackendKind.Gamepad)
        {
            input.SimulateAction(GIActions.FishingPrimaryAction, KeyType.KeyDown);
            return;
        }

        input.Mouse.LeftButtonDown();
    }

    public static void FishingPrimaryUp(this IInputChannel input)
    {
        if (InputHub.Backend.Kind == InputBackendKind.Gamepad)
        {
            input.SimulateAction(GIActions.FishingPrimaryAction, KeyType.KeyUp);
            return;
        }

        input.Mouse.LeftButtonUp();
    }

    public static void FishingPrimaryClick(this IInputChannel input)
    {
        if (InputHub.Backend.Kind == InputBackendKind.Gamepad)
        {
            input.SimulateAction(GIActions.FishingPrimaryAction);
            return;
        }

        input.Mouse.LeftButtonClick();
    }

    public static void FishingSecondaryClick(this IInputChannel input)
    {
        if (InputHub.Backend.Kind == InputBackendKind.Gamepad)
        {
            input.SimulateAction(GIActions.InteractionInSomeMode);
            return;
        }

        input.Mouse.RightButtonClick();
    }

    public static void FishingCancel(this IInputChannel input)
    {
        if (InputHub.Backend.Kind == InputBackendKind.Gamepad)
        {
            input.SimulateAction(GIActions.MenuCancel);
            return;
        }

        input.Keyboard.KeyPress(User32.VK.VK_ESCAPE);
    }

    public static bool IsFishingPrimaryDown(this IInputChannel input) =>
        InputHub.Backend.Kind == InputBackendKind.Gamepad
            ? input.IsActionDown(GIActions.FishingPrimaryAction)
            : input.IsKeyDown(User32.VK.VK_LBUTTON);
}
