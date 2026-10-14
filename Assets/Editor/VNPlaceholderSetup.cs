using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 《替神放羊》VNovelizer 占位资源一键配置
/// 菜单：VNovelizer → 一键配置占位立绘与头像
/// 规则：
///   立绘  → Characters/{角色ID}_{差分名}.png
///   头像  → Characters/Head_{角色ID}_{差分名}.png
/// 美术出真货后同名替换图片即可，无需改脚本。
/// </summary>
public static class VNPlaceholderSetup
{
    private const string CharacterDir = "Assets/Resources/VNovelizerRes/Characters";

    [MenuItem("VNovelizer/一键配置占位立绘与头像")]
    public static void Setup()
    {
        var guids = AssetDatabase.FindAssets("t:CharacterProfile", new[] { CharacterDir });
        if (guids.Length == 0)
        {
            Debug.LogWarning("[VN] 没找到任何 CharacterProfile，确认目录：" + CharacterDir);
            return;
        }

        int total = 0;
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadMainAssetAtPath(path);
            if (asset == null) continue;

            var so = new SerializedObject(asset);
            var idProp = so.FindProperty("CharacterID");
            if (idProp == null || string.IsNullOrEmpty(idProp.stringValue))
            {
                Debug.LogWarning("[VN] 跳过（缺 CharacterID）：" + path);
                continue;
            }
            string id = idProp.stringValue;

            int body = Fill(so, "ElementSprites", id + "_", false);
            int head = Fill(so, "HeadSprites", "Head_" + id + "_", true);

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            total++;
            Debug.Log($"[VN] {id}：立绘 {body} 张 / 头像 {head} 张");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[VN] 配置完成，共处理 {total} 个角色。");
    }

    private static int Fill(SerializedObject so, string propertyName, string prefix, bool isHead)
    {
        var list = so.FindProperty(propertyName);
        if (list == null) return 0;

        list.ClearArray();
        int index = 0;

        foreach (string file in Directory.GetFiles(CharacterDir, "*.png"))
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            if (!fileName.StartsWith(prefix)) continue;

            string element = fileName.Substring(prefix.Length);
            if (string.IsNullOrEmpty(element)) continue;

            // 立绘列表里剔除 Head_ 开头的头像文件
            if (!isHead && fileName.StartsWith("Head_")) continue;

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(file);
            if (sprite == null) continue;

            list.InsertArrayElementAtIndex(index);
            var item = list.GetArrayElementAtIndex(index);
            item.FindPropertyRelative("Element").stringValue = element;
            item.FindPropertyRelative("Sprite").objectReferenceValue = sprite;
            index++;
        }
        return index;
    }
}
