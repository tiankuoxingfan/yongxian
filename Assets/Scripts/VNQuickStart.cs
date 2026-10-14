#if UNITY_EDITOR
using UnityEngine;

/// <summary>
/// 剧情快速测试引导（仅编辑器）。
/// 无需在场景里挂任何物体：进入 Play 模式后自动调用
/// VNManager.StartGameOnScene 在当前场景直接开跑对话。
/// 由菜单 VNovelizer/快速测试/... 触发。
/// </summary>
public static class VNQuickStart
{
    public const string KEY_AUTO = "VNQuick_Auto";
    public const string KEY_SCRIPT = "VNQuick_Script";
    public const string KEY_LINE = "VNQuick_Line";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (!UnityEditor.EditorPrefs.HasKey(KEY_AUTO))
            return;

        string script = UnityEditor.EditorPrefs.GetString(KEY_SCRIPT, "");
        string line = UnityEditor.EditorPrefs.GetString(KEY_LINE, "");

        // 一次性标记：读完立刻清掉，避免下次手动 Play 也被自动接管
        UnityEditor.EditorPrefs.DeleteKey(KEY_AUTO);

        if (string.IsNullOrEmpty(script))
        {
            Debug.LogWarning("[VNQuickStart] 未指定剧本名，已跳过自动启动。请通过菜单 VNovelizer/快速测试 启动。");
            return;
        }

        var go = new GameObject("[VNQuickStart]");
        Object.DontDestroyOnLoad(go);
        var runner = go.AddComponent<VNQuickStartRunner>();
        runner.scriptName = script;
        runner.startLineID = line;
    }
}

internal class VNQuickStartRunner : MonoBehaviour
{
    public string scriptName;
    public string startLineID;

    private void Start()
    {
        // 等一帧，确保 UIManager / VNManager 单例与场景物体就绪
        StartCoroutine(Run());
    }

    private System.Collections.IEnumerator Run()
    {
        yield return null;

        Debug.Log($"[VNQuickStart] 启动剧本: {scriptName}  起始行: {(string.IsNullOrEmpty(startLineID) ? "(从头)" : startLineID)}");
        Debug.Log("[VNQuickStart] 操作：鼠标左键 / 空格 / 回车 推进对话，A 自动播放，H 隐藏UI，L 历史记录");

        try
        {
            VNManager.GetInstance().StartGameOnScene(scriptName, startLineID);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[VNQuickStart] 启动异常: {e.Message}\n" +
                           "请检查：1) VNScripts 下是否有对应 .csv  2) VNProjectConfig 路径是否填写  3) Console 中是否有 ScriptParser 报错");
        }

        Destroy(gameObject, 2f);
    }
}
#endif
