using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public static class SpriteFixerTool
{
    struct SpriteAssignment
    {
        public string gameObjectPath;
        public string assetPath;
        public string subSpriteName;
    }

    static Sprite LoadSubSprite(string assetPath, string subName)
    {
        var allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        foreach (var asset in allAssets)
        {
            if (asset is Sprite sprite && sprite.name == subName)
                return sprite;
        }
        Debug.LogWarning($"Sub-sprite '{subName}' not found in '{assetPath}'. Available:");
        foreach (var asset in allAssets)
        {
            if (asset is Sprite s)
                Debug.LogWarning($"  - {s.name}");
        }
        return null;
    }

    static void ApplySprite(string goPath, string assetPath, string subSpriteName)
    {
        var go = GameObject.Find(goPath);
        if (go == null)
        {
            Debug.LogWarning($"GameObject not found: {goPath}");
            return;
        }
        var img = go.GetComponent<Image>();
        if (img == null)
        {
            Debug.LogWarning($"No Image component on: {goPath}");
            return;
        }
        var sprite = LoadSubSprite(assetPath, subSpriteName);
        if (sprite == null)
        {
            Debug.LogWarning($"Sprite null for {goPath}");
            return;
        }
        img.sprite = sprite;
        img.type = Image.Type.Simple;
        img.preserveAspect = true;
        img.color = Color.white;
        EditorUtility.SetDirty(img);
        Debug.Log($"Applied sprite '{subSpriteName}' to {goPath}");
    }

    [MenuItem("Tools/Diagnose Sprites")]
    public static void DiagnoseSprites()
    {
        var images = Resources.FindObjectsOfTypeAll<Image>();
        Debug.Log($"=== Sprite Diagnosis: {images.Length} Image components found ===");
        foreach (var img in images)
        {
            if (img.gameObject.scene.name == null) continue; // skip prefab assets
            string path = GetFullPath(img.transform);
            string spriteName = img.sprite != null ? img.sprite.name : "NULL";
            string spriteAssetPath = img.sprite != null ? AssetDatabase.GetAssetPath(img.sprite) : "N/A";
            Debug.Log($"  [{path}] sprite={spriteName} asset={spriteAssetPath} color={img.color}");
        }
    }

    [MenuItem("Tools/Fix MainMenu Sprites")]
    public static void FixMainMenuSprites()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "MainMenu")
        {
            Debug.LogError("Load MainMenu scene first!");
            return;
        }

        var assignments = new List<SpriteAssignment>
        {
            new SpriteAssignment { gameObjectPath = "MainMenuCanvas/Background", assetPath = "Assets/PNG/menu/bg.png", subSpriteName = "bg_0" },
            new SpriteAssignment { gameObjectPath = "MainMenuCanvas/LogoImage", assetPath = "Assets/PNG/menu/logo.png", subSpriteName = "logo_0" },
            new SpriteAssignment { gameObjectPath = "MainMenuCanvas/StartButton", assetPath = "Assets/PNG/menu/play.png", subSpriteName = "play_0" },
            new SpriteAssignment { gameObjectPath = "MainMenuCanvas/LeaderBoardButton", assetPath = "Assets/PNG/menu/leader.png", subSpriteName = "leader_0" },
            new SpriteAssignment { gameObjectPath = "MainMenuCanvas/AboutButton", assetPath = "Assets/PNG/menu/about.png", subSpriteName = "about_0" },
            new SpriteAssignment { gameObjectPath = "MainMenuCanvas/PrivacyPolicyButton", assetPath = "Assets/PNG/menu/prize.png", subSpriteName = "prize_0" },
            new SpriteAssignment { gameObjectPath = "MainMenuCanvas/SettingsButton", assetPath = "Assets/PNG/menu/setting.png", subSpriteName = "setting_0" },

            // LeaderBoard window
            new SpriteAssignment { gameObjectPath = "MainMenuCanvas/LeaderBoardWindow/PanelBG", assetPath = "Assets/PNG/you_win/bg.png", subSpriteName = "bg_0" },
            new SpriteAssignment { gameObjectPath = "MainMenuCanvas/LeaderBoardWindow/CloseButton", assetPath = "Assets/PNG/btn/close.png", subSpriteName = "close_0" },

            // About window
            new SpriteAssignment { gameObjectPath = "MainMenuCanvas/AboutWindow/PanelBG", assetPath = "Assets/PNG/you_win/bg.png", subSpriteName = "bg_0" },
            new SpriteAssignment { gameObjectPath = "MainMenuCanvas/AboutWindow/CloseButton", assetPath = "Assets/PNG/btn/close.png", subSpriteName = "close_0" },

            // PrivacyPolicy window
            new SpriteAssignment { gameObjectPath = "MainMenuCanvas/PrivacyPolicyWindow/PanelBG", assetPath = "Assets/PNG/you_win/bg.png", subSpriteName = "bg_0" },
            new SpriteAssignment { gameObjectPath = "MainMenuCanvas/PrivacyPolicyWindow/CloseButton", assetPath = "Assets/PNG/btn/close.png", subSpriteName = "close_0" },
        };

        foreach (var a in assignments)
            ApplySprite(a.gameObjectPath, a.assetPath, a.subSpriteName);

        // Background should stretch, no preserveAspect
        var bgGo = GameObject.Find("MainMenuCanvas/Background");
        if (bgGo != null)
        {
            var bgImg = bgGo.GetComponent<Image>();
            bgImg.preserveAspect = false;
            EditorUtility.SetDirty(bgImg);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("=== MainMenu sprites fixed! Save the scene. ===");
    }

    [MenuItem("Tools/Fix Game Sprites")]
    public static void FixGameSprites()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "Game")
        {
            Debug.LogError("Load Game scene first!");
            return;
        }

        var assignments = new List<SpriteAssignment>
        {
            // WinPanel
            new SpriteAssignment { gameObjectPath = "BaseUI/WinPanel/PanelBG", assetPath = "Assets/PNG/you_win/bg.png", subSpriteName = "bg_0" },
            new SpriteAssignment { gameObjectPath = "BaseUI/WinPanel/HeaderImage", assetPath = "Assets/PNG/you_win/header.png", subSpriteName = "header_0" },
            new SpriteAssignment { gameObjectPath = "BaseUI/WinPanel/StarsDecoration", assetPath = "Assets/PNG/you_win/star_1.png", subSpriteName = "star_1_0" },
            new SpriteAssignment { gameObjectPath = "BaseUI/WinPanel/TableBG", assetPath = "Assets/PNG/you_win/table.png", subSpriteName = "table_0" },
            new SpriteAssignment { gameObjectPath = "BaseUI/WinPanel/RestartButton", assetPath = "Assets/PNG/btn/restart.png", subSpriteName = "restart_0" },
            new SpriteAssignment { gameObjectPath = "BaseUI/WinPanel/MenuButton", assetPath = "Assets/PNG/btn/menu.png", subSpriteName = "menu_0" },
            new SpriteAssignment { gameObjectPath = "BaseUI/WinPanel/NextButton", assetPath = "Assets/PNG/btn/next.png", subSpriteName = "next_0" },

            // GameOverPanel
            new SpriteAssignment { gameObjectPath = "BaseUI/GameOverPanel/PanelBG", assetPath = "Assets/PNG/you_lose/bg.png", subSpriteName = "bg_0" },
            new SpriteAssignment { gameObjectPath = "BaseUI/GameOverPanel/HeaderImage", assetPath = "Assets/PNG/you_lose/header.png", subSpriteName = "header_0" },
            new SpriteAssignment { gameObjectPath = "BaseUI/GameOverPanel/TableBG", assetPath = "Assets/PNG/you_lose/table.png", subSpriteName = "table_0" },
            new SpriteAssignment { gameObjectPath = "BaseUI/GameOverPanel/RestartButton", assetPath = "Assets/PNG/btn/restart.png", subSpriteName = "restart_0" },
            new SpriteAssignment { gameObjectPath = "BaseUI/GameOverPanel/MenuButton", assetPath = "Assets/PNG/btn/menu.png", subSpriteName = "menu_0" },

            // HUD
            new SpriteAssignment { gameObjectPath = "HUDView/PauseButton", assetPath = "Assets/PNG/btn/pause.png", subSpriteName = "pause_0" },
        };

        foreach (var a in assignments)
            ApplySprite(a.gameObjectPath, a.assetPath, a.subSpriteName);

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("=== Game sprites fixed! Save the scene. ===");
    }

    static string GetFullPath(Transform t)
    {
        var parts = new List<string>();
        while (t != null)
        {
            parts.Insert(0, t.name);
            t = t.parent;
        }
        return string.Join("/", parts);
    }
}
