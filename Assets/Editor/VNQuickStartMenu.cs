using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 剧情快速测试菜单：一键进入运行模式并自动开跑对话，无需在场景里挂任何物体。
/// </summary>
public static class VNQuickStartMenu
{
    private const string MENU = "VNovelizer/快速测试/";
    private const string SCENE = "Assets/Scenes/VNDebugScene.unity";

    [MenuItem(MENU + "▶ 第一幕 Chapter1（从头）", false, 10)]
    private static void RunCh1() => Launch("Chapter1", "");

    [MenuItem(MENU + "▶ 第二幕 Chapter2", false, 11)]
    private static void RunCh2() => Launch("Chapter2", "");

    [MenuItem(MENU + "▶ 第三幕 Chapter3", false, 12)]
    private static void RunCh3() => Launch("Chapter3", "");

    [MenuItem(MENU + "▶ 新手教程 Tutorial", false, 13)]
    private static void RunTutorial() => Launch("Tutorial", "");

    [MenuItem(MENU + "▶ 只看滑跪 Chapter1 @1015", false, 14)]
    private static void RunKneel() => Launch("Chapter1", "1015");

    [MenuItem(MENU + "■ 关闭自动启动", false, 40)]
    private static void Disable()
    {
        EditorPrefs.DeleteKey(VNQuickStart.KEY_AUTO);
        Debug.Log("[VNQuickStart] 已关闭自动启动，下次 Play 不会自动跑剧情。");
    }

    [MenuItem(MENU + "✓ 环境自检", false, 41)]
    private static void SelfCheck()
    {
        var cfg = Resources.Load<VNProjectConfig>("VNProjectConfig");
        bool ok = true;

        void Check(bool cond, string label, string fix)
        {
            Debug.Log($"{(cond ? "[OK]   " : "[FAIL] ")}{label}{(cond ? "" : "   → " + fix)}");
            if (!cond) ok = false;
        }

        Check(cfg != null, "VNProjectConfig.asset 可加载", "检查 Assets/Resources/VNProjectConfig.asset 是否存在");

        foreach (var s in new[] { "Chapter1", "Chapter2", "Chapter3", "Tutorial" })
        {
            var csv = Resources.Load<TextAsset>($"VNovelizerRes/VNScripts/{s}");
            Check(csv != null, $"剧本数据 VNScripts/{s}.csv", "剧本管理器里点「转换」生成 CSV");
        }

        foreach (var c in new[] { "Player", "Hypnos", "Mimi", "Dionysus" })
        {
            var prof = Resources.Load<CharacterProfile>($"VNovelizerRes/Characters/{c}");
            Check(prof != null, $"角色配置 Characters/{c}.asset", "在 Characters 目录创建 CharacterProfile");
        }

        var bg = Resources.Load<Texture2D>("VNovelizerRes/Backgrounds/BG_Athens_Day");
        Check(bg != null, "背景图 Backgrounds/BG_Athens_Day", "检查背景资源是否放在 VNovelizerRes/Backgrounds");

        var panel = Resources.Load<GameObject>("VNovelizerRes/VNPrefabs/UI/VNGamePlay/VNGameplayPanel");
        Check(panel != null, "UI 面板 VNGamePlay/VNGameplayPanel", "检查 VNPrefabs/UI/VNGamePlay 目录");

        // Build Settings 场景 GUID 校验
        foreach (var scene in EditorBuildSettings.scenes)
        {
            bool bad = string.IsNullOrEmpty(scene.guid.ToString()) ||
                       scene.guid.ToString() == "00000000000000000000000000000000";
            if (bad)
            {
                Check(false, $"Build Settings 场景 GUID 缺失: {scene.path}",
                      "File > Build Settings 里把该场景重新拖入列表");
            }
        }

        Debug.Log(ok
            ? "[VNQuickStart] 环境自检通过，可以跑剧情了。"
            : "[VNQuickStart] 环境自检发现问题，按上面提示修复后再试。");
    }

    private static void Launch(string scriptName, string startLineID)
    {
        if (EditorApplication.isPlaying)
            EditorApplication.isPlaying = false;

        EditorPrefs.SetString(VNQuickStart.KEY_SCRIPT, scriptName);
        EditorPrefs.SetString(VNQuickStart.KEY_LINE, startLineID);
        EditorPrefs.SetInt(VNQuickStart.KEY_AUTO, 1);

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EditorSceneManager.OpenScene(SCENE);
        EditorApplication.isPlaying = true;
    }
}
