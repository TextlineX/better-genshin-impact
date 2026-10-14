using BetterGenshinImpact.Core.Config;
using BetterGenshinImpact.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using BetterGenshinImpact.Core.Input.Backends.Gamepad;
using BetterGenshinImpact.Core.Simulator.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Vanara.PInvoke.User32;

namespace BetterGenshinImpact.Model;

public sealed record GamepadBindingOption(string DisplayName, GamepadControl Control)
{
    public static IReadOnlyList<GamepadBindingOption> All { get; } =
    [
        new("未映射", GamepadControl.UnmappedControl()),
        new("A", GamepadControl.ButtonControl(GamepadButton.A)),
        new("B", GamepadControl.ButtonControl(GamepadButton.B)),
        new("X", GamepadControl.ButtonControl(GamepadButton.X)),
        new("Y", GamepadControl.ButtonControl(GamepadButton.Y)),
        new("LB", GamepadControl.ButtonControl(GamepadButton.LeftBumper)),
        new("RB", GamepadControl.ButtonControl(GamepadButton.RightBumper)),
        new("Back", GamepadControl.ButtonControl(GamepadButton.Back)),
        new("Start", GamepadControl.ButtonControl(GamepadButton.Start)),
        new("十字键 上", GamepadControl.ButtonControl(GamepadButton.DPadUp)),
        new("十字键 右", GamepadControl.ButtonControl(GamepadButton.DPadRight)),
        new("十字键 下", GamepadControl.ButtonControl(GamepadButton.DPadDown)),
        new("十字键 左", GamepadControl.ButtonControl(GamepadButton.DPadLeft)),
        new("按下左摇杆", GamepadControl.ButtonControl(GamepadButton.LeftThumb)),
        new("按下右摇杆", GamepadControl.ButtonControl(GamepadButton.RightThumb)),
        new("LT", GamepadControl.TriggerControl(GamepadTrigger.Left)),
        new("RT", GamepadControl.TriggerControl(GamepadTrigger.Right)),
        new("左摇杆 上", GamepadControl.AxisControl(GamepadAxis.LeftThumbY, short.MaxValue)),
        new("左摇杆 下", GamepadControl.AxisControl(GamepadAxis.LeftThumbY, short.MinValue)),
        new("左摇杆 左", GamepadControl.AxisControl(GamepadAxis.LeftThumbX, short.MinValue)),
        new("左摇杆 右", GamepadControl.AxisControl(GamepadAxis.LeftThumbX, short.MaxValue)),
        new("右摇杆 上", GamepadControl.AxisControl(GamepadAxis.RightThumbY, short.MaxValue)),
        new("右摇杆 下", GamepadControl.AxisControl(GamepadAxis.RightThumbY, short.MinValue)),
        new("右摇杆 左", GamepadControl.AxisControl(GamepadAxis.RightThumbX, short.MinValue)),
        new("右摇杆 右", GamepadControl.AxisControl(GamepadAxis.RightThumbX, short.MaxValue)),
    ];

    public static GamepadBindingOption From(GamepadControl? control)
    {
        if (control is null)
        {
            return All[0];
        }

        return All.FirstOrDefault(option => SameControl(option.Control, control)) ?? All[0];
    }

    private static bool SameControl(GamepadControl left, GamepadControl right) =>
        left.Kind == right.Kind
        && left.Button == right.Button
        && left.Axis == right.Axis
        && left.AxisValue == right.AxisValue
        && left.Trigger == right.Trigger
        && left.TriggerValue == right.TriggerValue;
}

public partial class KeyBindingSettingModel:ObservableObject
{

    /// <summary>
    /// 按键绑定值
    /// </summary>
    [ObservableProperty]
    private KeyId _keyValue;

    [ObservableProperty]
    private ObservableCollection<KeyBindingSettingModel> _children = [];

    public string ActionName { get; set; }

    public bool IsExpanded => true;

    /// <summary>
    /// 界面上显示是文件夹而不是按键绑定
    /// </summary>
    [ObservableProperty]
    private bool _isDirectory;

    [ObservableProperty]
    private GamepadBindingOption _selectedGamepadBinding = GamepadBindingOption.All[0];

    public string ConfigPropertyName { get; set; }

    public GIActions? GamepadAction { get; }

    public IReadOnlyList<GamepadBindingOption> GamepadBindingOptions => GamepadBindingOption.All;

    public bool HasKeyboardBinding => !string.IsNullOrEmpty(ConfigPropertyName);

    public bool HasGamepadBinding => GamepadAction.HasValue;

    public KeyBindingSettingModel(string name)
    {
        IsDirectory = true;
        ActionName = name;
    }

    public KeyBindingSettingModel(string actionName,  string configPropertyName, KeyId keyValue)
    {
        ActionName = actionName;
        ConfigPropertyName = configPropertyName;
        KeyValue = keyValue;
        IsDirectory = false;
        GamepadAction = Enum.TryParse<GIActions>(configPropertyName, out var action) ? action : null;
    }

    public KeyBindingSettingModel(string actionName, GIActions gamepadAction)
    {
        ActionName = actionName;
        ConfigPropertyName = string.Empty;
        IsDirectory = false;
        GamepadAction = gamepadAction;
    }

}
