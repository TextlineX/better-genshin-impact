using System;
using System.Globalization;
using System.IO;
using System.Linq;

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

    string ResolveRecognitionFile(string taskName, string defaultPath, int captureWidth, int captureHeight);

    string ResolveTemplateFolder(string taskName, int captureWidth, int captureHeight, string defaultFolder);
}

public sealed class DefaultRecognitionProfileResolver(Func<RecognitionInputProfile> profileProvider) : IRecognitionProfileResolver
{
    public RecognitionInputProfile CurrentProfile => profileProvider();

    public string ResolveRecognitionFile(string taskName, string defaultPath, int captureWidth, int captureHeight)
    {
        if (CurrentProfile != RecognitionInputProfile.Gamepad)
        {
            return defaultPath;
        }

        var profileRoot = Path.Combine(Path.GetDirectoryName(defaultPath)!, "Gamepad");
        var profilePath = Path.Combine(profileRoot, "Recognition.json");
        return File.Exists(profilePath) && FindProfileFolder(profileRoot, captureWidth, captureHeight) is not null
            ? profilePath
            : defaultPath;
    }

    public string ResolveTemplateFolder(string taskName, int captureWidth, int captureHeight, string defaultFolder)
    {
        if (CurrentProfile != RecognitionInputProfile.Gamepad)
        {
            return defaultFolder;
        }

        var profileRoot = Path.Combine(Path.GetDirectoryName(defaultFolder)!, "Gamepad");
        return FindProfileFolder(profileRoot, captureWidth, captureHeight) ?? defaultFolder;
    }

    private static string? FindProfileFolder(string profileRoot, int captureWidth, int captureHeight)
    {
        if (!Directory.Exists(profileRoot))
        {
            return null;
        }

        var exact = Path.Combine(profileRoot, $"{captureWidth}x{captureHeight}");
        if (Directory.Exists(exact))
        {
            return exact;
        }

        var captureAspect = captureWidth / (double)captureHeight;
        return Directory.EnumerateDirectories(profileRoot)
            .Select(path => new { Path = path, Name = Path.GetFileName(path) })
            .Select(item => new
            {
                item.Path,
                Size = ParseProfileSize(item.Name),
            })
            .Where(item => item.Size is not null)
            .Select(item => new
            {
                item.Path,
                Width = item.Size!.Value.Width,
                Height = item.Size.Value.Height,
            })
            .Where(item => Math.Abs(item.Width / (double)item.Height - captureAspect) < 0.01)
            .OrderBy(item => Math.Abs(item.Width - captureWidth) + Math.Abs(item.Height - captureHeight))
            .Select(item => item.Path)
            .FirstOrDefault();
    }

    private static (int Width, int Height)? ParseProfileSize(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var parts = name.Split('x', 2, StringSplitOptions.TrimEntries);
        return parts.Length == 2
               && int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var width)
               && int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var height)
               && width > 0
               && height > 0
            ? (width, height)
            : null;
    }
}
