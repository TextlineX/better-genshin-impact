using System;
using System.IO;

namespace BetterGenshinImpact.Core.Recognition;

public enum RecognitionInputProfile
{
    KeyboardMouse,
    Gamepad,
}

/// <summary>
/// 识别素材 profile 的扩展点。没有手柄截图时默认回退到现有键鼠素材，不伪造手柄模板。
/// </summary>
public interface IRecognitionProfileResolver
{
    RecognitionInputProfile CurrentProfile { get; }

    string ResolveRecognitionFile(string taskName, string defaultPath);

    string ResolveTemplateFolder(string taskName, int captureWidth, int captureHeight, string defaultFolder);
}

public sealed class DefaultRecognitionProfileResolver(Func<RecognitionInputProfile> profileProvider) : IRecognitionProfileResolver
{
    public RecognitionInputProfile CurrentProfile => profileProvider();

    public string ResolveRecognitionFile(string taskName, string defaultPath)
    {
        if (CurrentProfile != RecognitionInputProfile.Gamepad)
        {
            return defaultPath;
        }

        var profilePath = Path.Combine(Path.GetDirectoryName(defaultPath)!, "Gamepad", "Recognition.json");
        return File.Exists(profilePath) ? profilePath : defaultPath;
    }

    public string ResolveTemplateFolder(string taskName, int captureWidth, int captureHeight, string defaultFolder)
    {
        if (CurrentProfile != RecognitionInputProfile.Gamepad)
        {
            return defaultFolder;
        }

        var profileFolder = Path.Combine(Path.GetDirectoryName(defaultFolder)!, "Gamepad", $"{captureWidth}x{captureHeight}");
        return Directory.Exists(profileFolder) ? profileFolder : defaultFolder;
    }
}
