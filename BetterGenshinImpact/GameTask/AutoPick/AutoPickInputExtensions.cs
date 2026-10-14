using BetterGenshinImpact.Core.Input;
using BetterGenshinImpact.Core.Simulator.Extensions;
using Vanara.PInvoke;

namespace BetterGenshinImpact.GameTask.AutoPick;

/// <summary>
/// Sends the configured pickup action without exposing keyboard VKs to callers.
/// The same task code can therefore operate with either the keyboard HUD or the
/// controller HUD.
/// </summary>
public static class AutoPickInputExtensions
{
    public static void PickDown(this IInputChannel input, User32.VK keyboardKey)
    {
        if (InputHub.Backend.Kind == InputBackendKind.Gamepad)
        {
            input.SimulateAction(GIActions.PickUpOrInteract, KeyType.KeyDown);
            return;
        }

        input.Keyboard.KeyDown(keyboardKey);
    }

    public static void PickUp(this IInputChannel input, User32.VK keyboardKey)
    {
        if (InputHub.Backend.Kind == InputBackendKind.Gamepad)
        {
            input.SimulateAction(GIActions.PickUpOrInteract, KeyType.KeyUp);
            return;
        }

        input.Keyboard.KeyUp(keyboardKey);
    }

    public static void PickPress(this IInputChannel input, User32.VK keyboardKey)
    {
        if (InputHub.Backend.Kind == InputBackendKind.Gamepad)
        {
            input.SimulateAction(GIActions.PickUpOrInteract);
            return;
        }

        input.Keyboard.KeyPress(keyboardKey);
    }
}
