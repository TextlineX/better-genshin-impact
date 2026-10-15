using System;

namespace BetterGenshinImpact.Core.Input;

/// <summary>
/// 一组可以整体切换的输入输出：Win32 (SendInput + PostMessage)、WebSdk、Gamepad
/// </summary>
public interface IInputBackend : IDisposable
{
    InputBackendKind Kind { get; }

    /// <summary>
    /// 是否可以在游戏窗口失去前台后继续接收输入。
    /// 键鼠后台通道使用窗口消息；虚拟手柄没有按窗口定向的输入通道。
    /// </summary>
    bool SupportsBackground { get; }

    /// <summary>
    /// 前台通道
    /// </summary>
    IInputChannel Foreground { get; }

    /// <summary>
    /// 后台通道。WebSdk / Gamepad 没有前后台之分，与 <see cref="Foreground"/> 是同一实例
    /// </summary>
    IInputChannel Background { get; }

    /// <summary>
    /// 释放所有通道中处于按下状态的按键
    /// </summary>
    void ReleaseAll();
}
