using BetterGenshinImpact.Core.Config;
using BetterGenshinImpact.Core.Input;
using BetterGenshinImpact.Core.Input.Backends.Gamepad;
using BetterGenshinImpact.Core.Simulator.Extensions;
using BetterGenshinImpact.Service.Interface;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BetterGenshinImpact.ViewModel.Windows;

public sealed record GamepadOption<T>(T Value, string DisplayName);

public sealed record InputBackendOption(InputBackendKind Value, string DisplayName);

public sealed partial class GamepadMappingSettingItem : ObservableObject
{
    private readonly GamepadMappingConfig _config;
    private readonly IConfigService _configService;

    public GamepadMappingSettingItem(
        GIActions action,
        string actionName,
        GamepadControl control,
        GamepadMappingConfig config,
        IConfigService configService)
    {
        Action = action;
        ActionName = actionName;
        _config = config;
        _configService = configService;

        _selectedKind = KindOptions.First(option => option.Value == control.Kind);
        _selectedButton = ButtonOptions.First(option => option.Value == control.Button);
        _selectedAxis = AxisOptions.First(option => option.Value == control.Axis);
        _selectedTrigger = TriggerOptions.First(option => option.Value == control.Trigger);
        _selectedAxisValue = AxisValueOptions.First(option => option.Value == control.AxisValue);
        _selectedTriggerValue = TriggerValueOptions.First(option => option.Value == control.TriggerValue);
        UpdateKindVisibility();
    }

    public GIActions Action { get; }

    public string ActionName { get; }

    public IReadOnlyList<GamepadOption<GamepadControlKind>> KindOptions { get; } =
    [
        new(GamepadControlKind.Button, "按钮"),
        new(GamepadControlKind.Axis, "摇杆轴"),
        new(GamepadControlKind.Trigger, "扳机"),
    ];

    public IReadOnlyList<GamepadOption<GamepadButton>> ButtonOptions { get; } =
    [
        new(GamepadButton.A, "A"),
        new(GamepadButton.B, "B"),
        new(GamepadButton.X, "X"),
        new(GamepadButton.Y, "Y"),
        new(GamepadButton.LeftBumper, "LB"),
        new(GamepadButton.RightBumper, "RB"),
        new(GamepadButton.Back, "Back"),
        new(GamepadButton.Start, "Start"),
        new(GamepadButton.DPadUp, "十字键 上"),
        new(GamepadButton.DPadRight, "十字键 右"),
        new(GamepadButton.DPadDown, "十字键 下"),
        new(GamepadButton.DPadLeft, "十字键 左"),
        new(GamepadButton.LeftThumb, "按下左摇杆"),
        new(GamepadButton.RightThumb, "按下右摇杆"),
    ];

    public IReadOnlyList<GamepadOption<GamepadAxis>> AxisOptions { get; } =
    [
        new(GamepadAxis.LeftThumbX, "左摇杆 X"),
        new(GamepadAxis.LeftThumbY, "左摇杆 Y"),
        new(GamepadAxis.RightThumbX, "右摇杆 X"),
        new(GamepadAxis.RightThumbY, "右摇杆 Y"),
    ];

    public IReadOnlyList<GamepadOption<GamepadTrigger>> TriggerOptions { get; } =
    [
        new(GamepadTrigger.Left, "LT"),
        new(GamepadTrigger.Right, "RT"),
    ];

    public IReadOnlyList<GamepadOption<short>> AxisValueOptions { get; } =
    [
        new(short.MinValue, "负向"),
        new((short)0, "中心"),
        new(short.MaxValue, "正向"),
    ];

    public IReadOnlyList<GamepadOption<byte>> TriggerValueOptions { get; } =
    [
        new((byte)128, "半程"),
        new(byte.MaxValue, "满程"),
    ];

    [ObservableProperty]
    private GamepadOption<GamepadControlKind> _selectedKind;

    [ObservableProperty]
    private GamepadOption<GamepadButton> _selectedButton;

    [ObservableProperty]
    private GamepadOption<GamepadAxis> _selectedAxis;

    [ObservableProperty]
    private GamepadOption<GamepadTrigger> _selectedTrigger;

    [ObservableProperty]
    private GamepadOption<short> _selectedAxisValue;

    [ObservableProperty]
    private GamepadOption<byte> _selectedTriggerValue;

    [ObservableProperty]
    private bool _isButton;

    [ObservableProperty]
    private bool _isAxis;

    [ObservableProperty]
    private bool _isTrigger;

    partial void OnSelectedKindChanged(GamepadOption<GamepadControlKind> value)
    {
        UpdateKindVisibility();
        SaveControl();
    }

    partial void OnSelectedButtonChanged(GamepadOption<GamepadButton> value) => SaveControl();

    partial void OnSelectedAxisChanged(GamepadOption<GamepadAxis> value) => SaveControl();

    partial void OnSelectedTriggerChanged(GamepadOption<GamepadTrigger> value) => SaveControl();

    partial void OnSelectedAxisValueChanged(GamepadOption<short> value) => SaveControl();

    partial void OnSelectedTriggerValueChanged(GamepadOption<byte> value) => SaveControl();

    private void UpdateKindVisibility()
    {
        IsButton = SelectedKind.Value == GamepadControlKind.Button;
        IsAxis = SelectedKind.Value == GamepadControlKind.Axis;
        IsTrigger = SelectedKind.Value == GamepadControlKind.Trigger;
    }

    private void SaveControl()
    {
        if (SelectedKind == null)
        {
            return;
        }

        _config.Actions[Action] = SelectedKind.Value switch
        {
            GamepadControlKind.Button => GamepadControl.ButtonControl(SelectedButton.Value),
            GamepadControlKind.Axis => GamepadControl.AxisControl(SelectedAxis.Value, SelectedAxisValue.Value),
            GamepadControlKind.Trigger => GamepadControl.TriggerControl(SelectedTrigger.Value, SelectedTriggerValue.Value),
            _ => throw new ArgumentOutOfRangeException(),
        };

        _configService.Save();
    }
}

public sealed partial class GamepadSettingsViewModel : ObservableObject
{
    private static readonly IReadOnlyDictionary<GIActions, string> ActionNames = new Dictionary<GIActions, string>
    {
        [GIActions.MoveForward] = "向前移动",
        [GIActions.MoveBackward] = "向后移动",
        [GIActions.MoveLeft] = "向左移动",
        [GIActions.MoveRight] = "向右移动",
        [GIActions.SwitchToWalkOrRun] = "切换走/跑",
        [GIActions.NormalAttack] = "普通攻击",
        [GIActions.ElementalSkill] = "元素战技",
        [GIActions.ElementalBurst] = "元素爆发",
        [GIActions.SprintKeyboard] = "冲刺",
        [GIActions.SprintMouse] = "冲刺（鼠标动作）",
        [GIActions.SwitchAimingMode] = "切换瞄准模式",
        [GIActions.Jump] = "跳跃",
        [GIActions.Drop] = "落下",
        [GIActions.PickUpOrInteract] = "拾取/交互",
        [GIActions.QuickUseGadget] = "快捷使用小道具",
        [GIActions.InteractionInSomeMode] = "特定玩法内交互",
        [GIActions.QuestNavigation] = "开启任务追踪",
        [GIActions.AbandonChallenge] = "中断挑战",
        [GIActions.SwitchMember1] = "切换小队角色 1",
        [GIActions.SwitchMember2] = "切换小队角色 2",
        [GIActions.SwitchMember3] = "切换小队角色 3",
        [GIActions.SwitchMember4] = "切换小队角色 4",
        [GIActions.SwitchMember5] = "切换小队角色 5",
        [GIActions.ShortcutWheel] = "呼出快捷轮盘",
        [GIActions.OpenInventory] = "打开背包",
        [GIActions.OpenCharacterScreen] = "打开角色界面",
        [GIActions.OpenMap] = "打开地图",
        [GIActions.OpenPaimonMenu] = "打开派蒙菜单",
        [GIActions.OpenPartySetupScreen] = "打开队伍配置",
        [GIActions.MenuUp] = "菜单焦点向上",
        [GIActions.MenuRight] = "菜单焦点向右",
        [GIActions.MenuDown] = "菜单焦点向下",
        [GIActions.MenuLeft] = "菜单焦点向左",
        [GIActions.MenuConfirm] = "菜单确认",
        [GIActions.MenuCancel] = "菜单取消",
    };

    private readonly IConfigService _configService;

    public GamepadSettingsViewModel(IConfigService configService)
    {
        _configService = configService;
        Config = configService.Get();
        Config.GamepadMappingConfig ??= GamepadMappingConfig.CreateDefault();

        InputBackends =
        [
            new(InputBackendKind.Win32, "键鼠（Win32）"),
            new(InputBackendKind.Gamepad, "手柄（ViGEm Xbox 360）"),
        ];

        var defaults = GamepadMappingConfig.CreateDefault().Actions;
        foreach (var action in Enum.GetValues<GIActions>())
        {
            if (!defaults.TryGetValue(action, out var defaultControl))
            {
                continue;
            }

            if (!Config.GamepadMappingConfig.Actions.TryGetValue(action, out var control))
            {
                control = Clone(defaultControl);
                Config.GamepadMappingConfig.Actions[action] = control;
            }

            Mappings.Add(new GamepadMappingSettingItem(
                action,
                ActionNames.TryGetValue(action, out var name) ? name : action.ToString(),
                control,
                Config.GamepadMappingConfig,
                _configService));
        }

        _selectedInputBackend = InputBackends.First(option => option.Value == Config.InputBackend);
    }

    public AllConfig Config { get; }

    public IReadOnlyList<InputBackendOption> InputBackends { get; }

    public ObservableCollection<GamepadMappingSettingItem> Mappings { get; } = [];

    [ObservableProperty]
    private InputBackendOption _selectedInputBackend;

    [ObservableProperty]
    private string _testStatus = "测试会向当前虚拟手柄发送一次 A（菜单确认）长按。";

    partial void OnSelectedInputBackendChanged(InputBackendOption value)
    {
        Config.InputBackend = value.Value;
        _configService.Save();
        TestStatus = value.Value == InputBackendKind.Gamepad
            ? "已选择手柄后端，重启 BetterGI 或重新绑定游戏后生效。"
            : "已选择键鼠后端，重启 BetterGI 或重新绑定游戏后生效。";
    }

    [RelayCommand]
    private async Task TestControllerAsync()
    {
        TestStatus = "正在发送 A 按键，观察 Windows 手柄测试或游戏内确认动作……";
        try
        {
            await Task.Run(() =>
            {
                if (InputHub.Backend.Kind == InputBackendKind.Gamepad)
                {
                    InputHub.Backend.Foreground.SimulateAction(GIActions.MenuConfirm, KeyType.Hold);
                    return;
                }

                using var backend = new GamepadInputBackend(Config.GamepadMappingConfig, Config.KeyBindingsConfig);
                backend.Foreground.SimulateAction(GIActions.MenuConfirm, KeyType.Hold);
            });

            TestStatus = "A 按键测试完成；成功代表虚拟手柄链路正常，游戏内仍需选择“控制器”模式。";
        }
        catch (Exception ex)
        {
            TestStatus = $"手柄测试失败：{ex.Message}";
        }
    }

    private static GamepadControl Clone(GamepadControl source) => source.Kind switch
    {
        GamepadControlKind.Button => GamepadControl.ButtonControl(source.Button),
        GamepadControlKind.Axis => GamepadControl.AxisControl(source.Axis, source.AxisValue),
        GamepadControlKind.Trigger => GamepadControl.TriggerControl(source.Trigger, source.TriggerValue),
        _ => throw new ArgumentOutOfRangeException(),
    };
}
