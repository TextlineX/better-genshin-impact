using BetterGenshinImpact.Core.Simulator.Extensions;
using System;
using System.Collections.Generic;

namespace BetterGenshinImpact.Core.Input.Backends.Gamepad;

public enum GamepadControlKind
{
    Button,
    Axis,
    Trigger,
    Unmapped,
}

public enum GamepadButton
{
    A,
    B,
    X,
    Y,
    LeftBumper,
    RightBumper,
    Back,
    Start,
    DPadUp,
    DPadRight,
    DPadDown,
    DPadLeft,
    LeftThumb,
    RightThumb,
}

public enum GamepadAxis
{
    LeftThumbX,
    LeftThumbY,
    RightThumbX,
    RightThumbY,
}

public enum GamepadTrigger
{
    Left,
    Right,
}

/// <summary>
/// 一个原神动作对应的虚拟 Xbox 控制器控件。AxisValue/TriggerValue 只在对应控件类型下使用。
/// </summary>
[Serializable]
public sealed class GamepadControl
{
    public GamepadControlKind Kind { get; set; }

    public GamepadButton Button { get; set; }

    public GamepadAxis Axis { get; set; }

    public GamepadTrigger Trigger { get; set; }

    public short AxisValue { get; set; } = short.MaxValue;

    public byte TriggerValue { get; set; } = byte.MaxValue;

    public static GamepadControl UnmappedControl() => new()
    {
        Kind = GamepadControlKind.Unmapped,
    };

    public static GamepadControl ButtonControl(GamepadButton button) => new()
    {
        Kind = GamepadControlKind.Button,
        Button = button,
    };

    public static GamepadControl AxisControl(GamepadAxis axis, short value) => new()
    {
        Kind = GamepadControlKind.Axis,
        Axis = axis,
        AxisValue = value,
    };

    public static GamepadControl TriggerControl(GamepadTrigger trigger, byte value = byte.MaxValue) => new()
    {
        Kind = GamepadControlKind.Trigger,
        Trigger = trigger,
        TriggerValue = value,
    };
}

/// <summary>
/// 原神动作到 Xbox/XInput 控件的配置。键鼠配置仍保留，手柄通道只读取它来反查直传 VK 对应的动作。
/// </summary>
[Serializable]
public sealed class GamepadMappingConfig
{
    public Dictionary<GIActions, GamepadControl> Actions { get; set; } = CreateDefaultActions();

    public static GamepadMappingConfig CreateDefault()
    {
        return new GamepadMappingConfig
        {
            Actions = CreateDefaultActions(),
        };
    }

    private static Dictionary<GIActions, GamepadControl> CreateDefaultActions()
    {
        return new Dictionary<GIActions, GamepadControl>
        {
                [GIActions.MoveForward] = GamepadControl.AxisControl(GamepadAxis.LeftThumbY, short.MaxValue),
                [GIActions.MoveBackward] = GamepadControl.AxisControl(GamepadAxis.LeftThumbY, short.MinValue),
                [GIActions.MoveLeft] = GamepadControl.AxisControl(GamepadAxis.LeftThumbX, short.MinValue),
                [GIActions.MoveRight] = GamepadControl.AxisControl(GamepadAxis.LeftThumbX, short.MaxValue),
                [GIActions.SwitchToWalkOrRun] = GamepadControl.ButtonControl(GamepadButton.LeftThumb),
                [GIActions.NormalAttack] = GamepadControl.TriggerControl(GamepadTrigger.Right),
                [GIActions.ElementalSkill] = GamepadControl.ButtonControl(GamepadButton.Y),
                [GIActions.ElementalBurst] = GamepadControl.ButtonControl(GamepadButton.B),
                [GIActions.SprintKeyboard] = GamepadControl.ButtonControl(GamepadButton.RightBumper),
                [GIActions.SprintMouse] = GamepadControl.ButtonControl(GamepadButton.RightBumper),
                [GIActions.SwitchAimingMode] = GamepadControl.TriggerControl(GamepadTrigger.Left),
                [GIActions.Jump] = GamepadControl.ButtonControl(GamepadButton.A),
                [GIActions.Drop] = GamepadControl.ButtonControl(GamepadButton.B),
                [GIActions.PickUpOrInteract] = GamepadControl.ButtonControl(GamepadButton.X),
                [GIActions.QuickUseGadget] = GamepadControl.ButtonControl(GamepadButton.DPadDown),
                [GIActions.InteractionInSomeMode] = GamepadControl.ButtonControl(GamepadButton.X),
                [GIActions.QuestNavigation] = GamepadControl.ButtonControl(GamepadButton.DPadUp),
                [GIActions.AbandonChallenge] = GamepadControl.ButtonControl(GamepadButton.B),
                [GIActions.SwitchMember1] = GamepadControl.ButtonControl(GamepadButton.DPadUp),
                [GIActions.SwitchMember2] = GamepadControl.ButtonControl(GamepadButton.DPadRight),
                [GIActions.SwitchMember3] = GamepadControl.ButtonControl(GamepadButton.DPadDown),
                [GIActions.SwitchMember4] = GamepadControl.ButtonControl(GamepadButton.DPadLeft),
                [GIActions.SwitchMember5] = GamepadControl.ButtonControl(GamepadButton.RightThumb),
                [GIActions.ShortcutWheel] = GamepadControl.ButtonControl(GamepadButton.LeftBumper),
                [GIActions.OpenInventory] = GamepadControl.ButtonControl(GamepadButton.Back),
                [GIActions.OpenCharacterScreen] = GamepadControl.ButtonControl(GamepadButton.DPadLeft),
                [GIActions.OpenMap] = GamepadControl.ButtonControl(GamepadButton.DPadUp),
                [GIActions.OpenPaimonMenu] = GamepadControl.ButtonControl(GamepadButton.Start),
                [GIActions.OpenPartySetupScreen] = GamepadControl.ButtonControl(GamepadButton.Back),
                [GIActions.MenuUp] = GamepadControl.ButtonControl(GamepadButton.DPadUp),
                [GIActions.MenuRight] = GamepadControl.ButtonControl(GamepadButton.DPadRight),
                [GIActions.MenuDown] = GamepadControl.ButtonControl(GamepadButton.DPadDown),
                [GIActions.MenuLeft] = GamepadControl.ButtonControl(GamepadButton.DPadLeft),
                [GIActions.MenuConfirm] = GamepadControl.ButtonControl(GamepadButton.A),
                [GIActions.MenuCancel] = GamepadControl.ButtonControl(GamepadButton.B),
                // Do not guess a controller button for PC-only shortcuts. The action
                // is still exposed in settings so the user can record a real mapping
                // for the controller UI they use.
                [GIActions.OpenAdventurerHandbook] = GamepadControl.UnmappedControl(),
                [GIActions.OpenCoOpScreen] = GamepadControl.UnmappedControl(),
                [GIActions.OpenWishScreen] = GamepadControl.UnmappedControl(),
                [GIActions.OpenBattlePassScreen] = GamepadControl.UnmappedControl(),
                [GIActions.OpenTheEventsMenu] = GamepadControl.UnmappedControl(),
                [GIActions.OpenTheSettingsMenu] = GamepadControl.UnmappedControl(),
                [GIActions.OpenTheFurnishingScreen] = GamepadControl.UnmappedControl(),
                [GIActions.OpenStellarReunion] = GamepadControl.UnmappedControl(),
                [GIActions.OpenQuestMenu] = GamepadControl.UnmappedControl(),
                [GIActions.OpenNotificationDetails] = GamepadControl.UnmappedControl(),
                [GIActions.OpenChatScreen] = GamepadControl.UnmappedControl(),
                [GIActions.OpenSpecialEnvironmentInformation] = GamepadControl.UnmappedControl(),
                [GIActions.CheckTutorialDetails] = GamepadControl.UnmappedControl(),
                [GIActions.ElementalSight] = GamepadControl.UnmappedControl(),
                [GIActions.ShowCursor] = GamepadControl.UnmappedControl(),
                [GIActions.OpenFriendsScreen] = GamepadControl.UnmappedControl(),
                [GIActions.HideUI] = GamepadControl.UnmappedControl(),
                [GIActions.FishingPrimaryAction] = GamepadControl.UnmappedControl(),
                [GIActions.MusicNote1] = GamepadControl.UnmappedControl(),
                [GIActions.MusicNote2] = GamepadControl.UnmappedControl(),
                [GIActions.MusicNote3] = GamepadControl.UnmappedControl(),
                [GIActions.MusicNote4] = GamepadControl.UnmappedControl(),
                [GIActions.MusicNote5] = GamepadControl.UnmappedControl(),
                [GIActions.MusicNote6] = GamepadControl.UnmappedControl(),
        };
    }

    public bool TryGet(GIActions action, out GamepadControl control) => Actions.TryGetValue(action, out control!);
}

public interface IGamepadDevice : IDisposable
{
    void SetButton(GamepadButton button, bool pressed);

    void SetAxis(GamepadAxis axis, short value);

    void SetTrigger(GamepadTrigger trigger, byte value);

    void Reset();
}
