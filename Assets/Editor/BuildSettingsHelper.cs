using UnityEditor;

public static class BuildSettingsHelper
{
    [MenuItem("Tools/Setup Build Scenes")]
    public static void SetupBuildScenes()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Game.unity", true)
        };
        UnityEngine.Debug.Log("Build scenes set: MainMenu (0), Game (1)");
    }
}
