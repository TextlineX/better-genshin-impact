using BetterGenshinImpact.Core.Config;
using BetterGenshinImpact.Core.Simulator.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;
using Vanara.PInvoke;

namespace BetterGenshinImpact.Core.Input.Backends.Gamepad;

/// <summary>
/// Xbox/XInput 风格虚拟手柄后端。它只向 ViGEm 虚拟设备发送状态，不把键盘 VK 值当成手柄按钮。
/// </summary>
public sealed class GamepadInputBackend : IInputBackend
{
    private readonly GamepadChannel _channel;
    private readonly IGamepadDevice _device;

    public GamepadInputBackend(GamepadMappingConfig? mapping = null, KeyBindingsConfig? keyBindings = null)
        : this(new ViGEmXbox360Device(), mapping, keyBindings)
    {
    }

    public GamepadInputBackend(IGamepadDevice device, GamepadMappingConfig? mapping = null, KeyBindingsConfig? keyBindings = null)
    {
        _device = device ?? throw new ArgumentNullException(nameof(device));
        _channel = new GamepadChannel(_device, mapping ?? GamepadMappingConfig.CreateDefault(), keyBindings ?? new KeyBindingsConfig());
    }

    public InputBackendKind Kind => InputBackendKind.Gamepad;

    public IInputChannel Foreground => _channel;

    public IInputChannel Background => _channel;

    public void ReleaseAll() => _channel.ReleaseAll();

    public void Dispose()
    {
        _channel.ReleaseAll();
        _device.Dispose();
    }
}

internal sealed class GamepadChannel : InputChannelBase
{
    private readonly object _stateLock = new();
    private readonly IGamepadDevice _device;
    private readonly GamepadMappingConfig _mapping;
    private readonly Dictionary<User32.VK, GIActions> _keyActions;
    private readonly Dictionary<GIActions, GamepadControl> _activeActions = new();
    private short _rightThumbX;
    private short _rightThumbY;
    private long _rightThumbPulseId;

    public GamepadChannel(IGamepadDevice device, GamepadMappingConfig mapping, KeyBindingsConfig keyBindings)
    {
        _device = device;
        _mapping = mapping;
        _keyActions = BuildKeyActions(keyBindings);
    }

    public override IInputChannel SimulateAction(GIActions action, KeyType type = KeyType.KeyPress)
    {
        if (!_mapping.TryGet(action, out var control))
        {
            WarnUnsupported($"动作 {action} 没有可用的手柄映射，请到“设置 → 按键设置”选择手柄按钮");
            return this;
        }

        // “未映射”是用户对控制器不可用动作的明确选择，不是运行时异常。
        if (control.Kind == GamepadControlKind.Unmapped)
        {
            return this;
        }

        switch (type)
        {
            case KeyType.KeyDown:
                SetAction(action, true);
                break;
            case KeyType.KeyUp:
                SetAction(action, false);
                break;
            case KeyType.Hold:
                SetAction(action, true);
                Thread.Sleep(1000);
                SetAction(action, false);
                break;
            case KeyType.KeyPress:
                var wasPressed = IsActionPressed(action);
                SetAction(action, true);
                if (!wasPressed)
                {
                    SetAction(action, false);
                }
                break;
        }

        return this;
    }

    public override bool IsKeyDown(User32.VK key)
    {
        return TryResolveAction(key, out var action) && IsActionPressed(action);
    }

    public override bool IsActionDown(GIActions action) => IsActionPressed(action);

    protected override void OnKeyDown(User32.VK key)
    {
        if (TryResolveAction(key, out var action))
        {
            SetAction(action, true);
        }
        else
        {
            WarnUnsupported($"VK {key} 没有对应的手柄动作");
        }
    }

    protected override void OnKeyUp(User32.VK key)
    {
        if (TryResolveAction(key, out var action))
        {
            SetAction(action, false);
        }
    }

    protected override void OnKeyPress(User32.VK key)
    {
        if (TryResolveAction(key, out var action))
        {
            SimulateAction(action);
        }
        else
        {
            WarnUnsupported($"VK {key} 没有对应的手柄动作");
        }
    }

    protected override void OnMouseButton(InputMouseButton button, bool down)
    {
        // 旧任务大量使用 Region.Click()。在控制器界面中它表达的是“确认当前焦点”，
        // 而不是向游戏发送鼠标坐标。把这两个通用语义转换到虚拟手柄，避免每个任务
        // 都复制一套键鼠/手柄分支；真正需要鼠标坐标的路径仍会由 OnMoveTo 明确告警。
        var action = button switch
        {
            InputMouseButton.Left => GIActions.MenuConfirm,
            InputMouseButton.Right => GIActions.MenuCancel,
            _ => (GIActions?)null,
        };

        if (action is { } mappedAction)
        {
            SetAction(mappedAction, down);
            return;
        }

        WarnUnsupported($"鼠标按钮 {button} 在手柄模式下没有对应的控制器语义");
    }

    protected override void OnMoveBy(int dx, int dy)
    {
        var pulseId = Interlocked.Increment(ref _rightThumbPulseId);
        lock (_stateLock)
        {
            _rightThumbX = (short)Math.Clamp(dx * 512, short.MinValue, short.MaxValue);
            _rightThumbY = (short)Math.Clamp(-dy * 512, short.MinValue, short.MaxValue);
            RefreshDeviceState();
        }

        // Mouse movement is a delta, while XInput is a stateful axis. Keep the
        // virtual stick deflected only for one polling window, then return it to
        // center so camera control cannot remain stuck.
        Thread.Sleep(16);
        lock (_stateLock)
        {
            if (pulseId == _rightThumbPulseId)
            {
                _rightThumbX = 0;
                _rightThumbY = 0;
                RefreshDeviceState();
            }
        }
    }

    protected override void OnMoveTo(double absX, double absY) =>
        WarnUnsupported("绝对鼠标定位在手柄模式下不可用，请使用焦点导航");

    protected override void OnScroll(int clicks)
    {
        var action = clicks >= 0 ? GIActions.MoveForward : GIActions.MoveBackward;
        for (var i = 0; i < Math.Abs(clicks); i++)
        {
            SimulateAction(action);
        }
    }

    public override void ReleaseAll()
    {
        base.ReleaseAll();
        lock (_stateLock)
        {
            Interlocked.Increment(ref _rightThumbPulseId);
            _activeActions.Clear();
            _rightThumbX = 0;
            _rightThumbY = 0;
            _device.Reset();
        }
    }

    private bool IsActionPressed(GIActions action)
    {
        lock (_stateLock)
        {
            return _activeActions.ContainsKey(action);
        }
    }

    private void SetAction(GIActions action, bool pressed)
    {
        lock (_stateLock)
        {
            if (pressed)
            {
                if (_mapping.TryGet(action, out var control))
                {
                    _activeActions[action] = control;
                }
            }
            else
            {
                _activeActions.Remove(action);
            }

            RefreshDeviceState();
        }
    }

    private void RefreshDeviceState()
    {
        _device.Reset();
        foreach (var control in _activeActions.Values)
        {
            switch (control.Kind)
            {
                case GamepadControlKind.Button:
                    _device.SetButton(control.Button, true);
                    break;
                case GamepadControlKind.Axis:
                    _device.SetAxis(control.Axis, control.AxisValue);
                    break;
                case GamepadControlKind.Trigger:
                    _device.SetTrigger(control.Trigger, control.TriggerValue);
                    break;
                case GamepadControlKind.Unmapped:
                    break;
            }
        }

        if (_rightThumbX != 0 || _rightThumbY != 0)
        {
            _device.SetAxis(GamepadAxis.RightThumbX, _rightThumbX);
            _device.SetAxis(GamepadAxis.RightThumbY, _rightThumbY);
        }
    }

    private static Dictionary<User32.VK, GIActions> BuildKeyActions(KeyBindingsConfig config)
    {
        var result = new Dictionary<User32.VK, GIActions>();
        foreach (var action in Enum.GetValues<GIActions>())
        {
            var property = typeof(KeyBindingsConfig).GetProperty(action.ToString());
            if (property?.GetValue(config) is not KeyId keyId || keyId is KeyId.None or KeyId.Unknown)
            {
                continue;
            }

            try
            {
                result[keyId.ToVK()] = action;
            }
            catch (ArgumentOutOfRangeException)
            {
                // 未配置的动作不进入反查表。
            }
        }

        return result;
    }

    private bool TryResolveAction(User32.VK key, out GIActions action)
    {
        // 任务中直传的 Esc/Space/Enter 是菜单语义；显式 SimulateAction(OpenPaimonMenu)
        // 仍然使用配置的 Start 映射，避免把“关闭当前菜单”误发成“打开派蒙菜单”。
        action = key switch
        {
            User32.VK.VK_ESCAPE => GIActions.MenuCancel,
            User32.VK.VK_RETURN or User32.VK.VK_SPACE => GIActions.MenuConfirm,
            _ => default,
        };

        return action != default || _keyActions.TryGetValue(key, out action);
    }
}
