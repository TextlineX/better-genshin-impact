using BetterGenshinImpact.Core.Recognition;

namespace BetterGenshinImpact.UnitTest.CoreTests.RecognitionTests;

public class RecognitionProfileResolverTests
{
    [Theory]
    [InlineData(1920, 1200, true)]
    [InlineData(2560, 1600, true)]
    [InlineData(1920, 1080, false)]
    public void GamepadProfile_RequiresMatchingCaptureAssetFolder(int width, int height, bool expectedProfile)
    {
        var root = Path.Combine(Path.GetTempPath(), $"bgi-recognition-profile-{Guid.NewGuid():N}");
        var assets = Path.Combine(root, "Assets");
        var gamepad = Path.Combine(assets, "Gamepad");
        var defaultPath = Path.Combine(assets, "Recognition.json");
        var profilePath = Path.Combine(gamepad, "Recognition.json");

        try
        {
            Directory.CreateDirectory(gamepad);
            File.WriteAllText(defaultPath, "{}");
            File.WriteAllText(profilePath, "{}");
            Directory.CreateDirectory(Path.Combine(gamepad, "1920x1200"));

            var resolver = new DefaultRecognitionProfileResolver(() => RecognitionInputProfile.Gamepad);
            var resolved = resolver.ResolveRecognitionFile("Test", defaultPath, width, height);

            Assert.Equal(expectedProfile ? profilePath : defaultPath, resolved);

            var defaultFolder = Path.Combine(assets, "1920x1080");
            var templateFolder = resolver.ResolveTemplateFolder("Test", width, height, defaultFolder);
            Assert.Equal(expectedProfile ? Path.Combine(gamepad, "1920x1200") : defaultFolder, templateFolder);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}
