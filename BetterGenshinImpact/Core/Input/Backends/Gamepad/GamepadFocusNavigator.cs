using BetterGenshinImpact.Core.Simulator.Extensions;
using System;

namespace BetterGenshinImpact.Core.Input.Backends.Gamepad;

/// <summary>
/// 手柄 UI 的焦点导航入口。任务层通过动作导航，不再假设鼠标坐标点击在控制器模式下有效。
/// </summary>
public interface IGamepadFocusNavigator
{
    void Up();
    void Right();
    void Down();
    void Left();
    void Confirm();
    void Cancel();

    void SelectOption(int optionIndex, int optionCount);
}

public sealed class GamepadFocusNavigator(IInputChannel channel) : IGamepadFocusNavigator
{
    public void Up() => channel.SimulateAction(GIActions.MenuUp);
    public void Right() => channel.SimulateAction(GIActions.MenuRight);
    public void Down() => channel.SimulateAction(GIActions.MenuDown);
    public void Left() => channel.SimulateAction(GIActions.MenuLeft);
    public void Confirm() => channel.SimulateAction(GIActions.MenuConfirm);
    public void Cancel() => channel.SimulateAction(GIActions.MenuCancel);

    public void SelectOption(int optionIndex, int optionCount)
    {
        if (optionCount <= 0)
        {
            Confirm();
            return;
        }

        // 先回到列表顶部，再按识别结果的视觉顺序下移。具体焦点初始位置需用实机截图确认。
        for (var i = 0; i < optionCount; i++)
        {
            Up();
        }

        for (var i = 0; i < Math.Clamp(optionIndex, 0, optionCount - 1); i++)
        {
            Down();
        }

        Confirm();
    }
}
