using System;
using BetterGenshinImpact.GameTask;
using BetterGenshinImpact.GameTask.Common;
using Microsoft.Extensions.Logging;

namespace BetterGenshinImpact.Helpers;

public class AssertUtils
{
    public static bool IsSupportedGameAspectRatio(int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            return false;
        }

        var widthValue = (long)width;
        var heightValue = (long)height;
        return widthValue * 9 == heightValue * 16 // 16:9
            || widthValue * 5 == heightValue * 8; // 16:10
    }

    public static bool Is16By10(int width, int height) =>
        width > 0 && height > 0 && (long)width * 5 == (long)height * 8;

    public static void IsTrue(bool b, string msg)
    {
        if (!b)
        {
            throw new System.Exception(msg);
        }
    }
    
    
    /// <summary>
    /// 强制要求游戏分辨率比例检查。支持 16:9 与 16:10。
    /// 要求功能必须处于启用状态
    /// </summary>
    public static void CheckGameResolution(string msg = "")
    {
        var gameScreenSize = SystemControl.GetGameScreenRect(TaskContext.Instance().GameHandle);
        if (!IsSupportedGameAspectRatio(gameScreenSize.Width, gameScreenSize.Height))
        {
            TaskControl.Logger.LogError("游戏窗口比例不受支持！当前分辨率为 {Width}x{Height}，目前支持 16:9 与 16:10，无法正常使用{Msg}功能！", gameScreenSize.Width, gameScreenSize.Height, msg);
            throw new Exception("游戏窗口比例不受支持，目前支持 16:9 与 16:10");
        }
    }
}
