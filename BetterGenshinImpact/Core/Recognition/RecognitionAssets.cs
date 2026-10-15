using BetterGenshinImpact.Core.Config;
using BetterGenshinImpact.Core.Input;
using BetterGenshinImpact.Core.Recognition.OpenCv;
using BetterGenshinImpact.GameTask;
using BetterGenshinImpact.GameTask.Model.Area;
using Microsoft.Extensions.Logging;
using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace BetterGenshinImpact.Core.Recognition;

public static class RecognitionAssets
{
    private sealed class LoggerTag;

    private static readonly ConcurrentDictionary<RecognitionAssetCacheKey, RecognitionObject> Cache = new();
    private static readonly ConcurrentDictionary<string, byte> ProfileFallbackWarnings = new();
    private static readonly ILogger Logger = App.GetLogger<LoggerTag>();

    public static IRecognitionProfileResolver ProfileResolver { get; set; } =
        new DefaultRecognitionProfileResolver(() => InputHub.Backend.Kind == InputBackendKind.Gamepad
            ? RecognitionInputProfile.Gamepad
            : RecognitionInputProfile.KeyboardMouse);

    public static RecognitionObject Get(string taskName, string objectName, Region region)
    {
        ArgumentNullException.ThrowIfNull(region);
        return Get(taskName, objectName, region.Width, region.Height);
    }

    public static RecognitionObject Get(string taskName, string objectName)
    {
        var captureRect = TaskContext.Instance().SystemInfo.ScaleMax1080PCaptureRect;
        return Get(taskName, objectName, captureRect.Width, captureRect.Height);
    }

    public static RecognitionObject Get(string taskName, string objectName, int captureWidth, int captureHeight)
    {
        var profile = ProfileResolver.CurrentProfile;
        WarnIfGamepadProfileMissing(taskName, captureWidth, captureHeight, profile);
        var key = new RecognitionAssetCacheKey(taskName, objectName, captureWidth, captureHeight, profile);
        var recognitionObject = Cache.GetOrAdd(key, static cacheKey => Load(cacheKey));
        return recognitionObject;
    }

    public static void Clear(string taskName, string objectName)
    {
        foreach (var key in Cache.Keys)
        {
            if (string.Equals(key.TaskName, taskName, StringComparison.Ordinal)
                && string.Equals(key.ObjectName, objectName, StringComparison.Ordinal))
            {
                Cache.TryRemove(key, out _);
            }
        }
    }

    public static void ClearTask(string taskName)
    {
        foreach (var key in Cache.Keys)
        {
            if (string.Equals(key.TaskName, taskName, StringComparison.Ordinal))
            {
                Cache.TryRemove(key, out _);
            }
        }
    }

    public static void ClearAll()
    {
        Cache.Clear();
        ProfileFallbackWarnings.Clear();
    }

    private static void WarnIfGamepadProfileMissing(
        string taskName,
        int captureWidth,
        int captureHeight,
        RecognitionInputProfile profile)
    {
        if (profile != RecognitionInputProfile.Gamepad || HasProfile(taskName, captureWidth, captureHeight))
        {
            return;
        }

        var key = $"{taskName}:{captureWidth}x{captureHeight}";
        if (ProfileFallbackWarnings.TryAdd(key, 0))
        {
            Logger.LogWarning(
                "手柄模式：{TaskName} 在 {Width}x{Height} 没有手柄识别 Profile，当前会回退到键鼠素材；这不代表该功能已适配手柄界面",
                taskName,
                captureWidth,
                captureHeight);
        }
    }

    public static bool HasProfile(string taskName, int captureWidth, int captureHeight)
    {
        var defaultPath = Global.Absolute($@"GameTask\{taskName}\Assets\Recognition.json");
        return ProfileResolver.CurrentProfile == RecognitionInputProfile.Gamepad
            && !string.Equals(
                ProfileResolver.ResolveRecognitionFile(taskName, defaultPath, captureWidth, captureHeight),
                defaultPath,
                StringComparison.OrdinalIgnoreCase);
    }

    private static RecognitionObject Load(RecognitionAssetCacheKey key)
    {
        var defaultRecognitionPath = Global.Absolute($@"GameTask\{key.TaskName}\Assets\Recognition.json");
        return RecognitionObjectJsonLoader.LoadFromFile(
            ProfileResolver.ResolveRecognitionFile(key.TaskName, defaultRecognitionPath, key.CaptureWidth, key.CaptureHeight),
            key.ObjectName,
            new RecognitionObjectJsonLoadContext
            {
                CaptureHeight = key.CaptureHeight,
                CaptureWidth = key.CaptureWidth,
                TemplateLoader = (template, mode) => LoadTemplateImage(key.TaskName, template, key.CaptureWidth, key.CaptureHeight, mode),
            });
    }

    private static Mat LoadTemplateImage(string taskName, string assetName, int captureWidth, int captureHeight, ImreadModes flags)
    {
        var defaultFolder = Global.Absolute($@"GameTask\{taskName}\Assets\{captureWidth}x{captureHeight}");
        var assetsFolder = ProfileResolver.ResolveTemplateFolder(taskName, captureWidth, captureHeight, defaultFolder);
        if (!Directory.Exists(assetsFolder))
        {
            var defaultProfileFolder = Global.Absolute($@"GameTask\{taskName}\Assets\Gamepad\1920x1080");
            assetsFolder = Directory.Exists(defaultProfileFolder)
                ? defaultProfileFolder
                : Global.Absolute($@"GameTask\{taskName}\Assets\1920x1080");
        }

        if (!Directory.Exists(assetsFolder))
        {
            throw new FileNotFoundException($"未找到{taskName}的素材文件夹");
        }

        var filePath = Path.Combine(assetsFolder, assetName);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"未找到{taskName}中的{assetName}文件");
        }

        using var stream = File.OpenRead(filePath);
        var mat = Mat.FromStream(stream, flags);
        if (captureWidth < 1920)
        {
            using (mat)
            {
                return ResizeHelper.Resize(mat, captureWidth / 1920d);
            }
        }

        return mat;
    }

    private readonly record struct RecognitionAssetCacheKey(string TaskName, string ObjectName, int CaptureWidth, int CaptureHeight, RecognitionInputProfile Profile);
}
