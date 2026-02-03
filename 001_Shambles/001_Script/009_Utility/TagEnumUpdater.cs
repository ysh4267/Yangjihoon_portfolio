#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

[InitializeOnLoad]
public class TagEnumUpdater {
    static TagEnumUpdater() {
        CompilationPipeline.compilationStarted += OnCompilationStarted;
    }

    private static void OnCompilationStarted(object obj) {
        UpdateTagEnum();
    }

    private static void UpdateTagEnum() {
        string[] tags = UnityEditorInternal.InternalEditorUtility.tags;

        string assetPath = "Assets/Scripts/Utility/Enum/ENUM_TAG.cs";
        bool needsUpdate = false;

        if (File.Exists(assetPath)) {
            string[] fileLines = File.ReadAllLines(assetPath);
            foreach (var tag in tags) {
                string validName = tag.Replace(" ", "_");
                if (Array.FindIndex(fileLines, line => line.Contains(validName)) < 0) {
                    needsUpdate = true;
                    break;
                }
            }
        }
        else {
            needsUpdate = true;
        }

        if (needsUpdate) {
            CreateTagEnum();
        }
    }

    private static void CreateTagEnum() {
        string[] tags = UnityEditorInternal.InternalEditorUtility.tags;

        StringBuilder enumCode = new StringBuilder();
        enumCode.AppendLine("public enum ENUM_TAG");
        enumCode.AppendLine("{");

        foreach (string tag in tags) {
            string validName = tag.Replace(" ", "_");
            enumCode.AppendLine($"    {validName},");
        }

        enumCode.AppendLine("}");

        string assetPath = "Assets/Scripts/Utility/Enum/ENUM_TAG.cs";

        // 디렉토리가 존재하지 않으면 생성
        string directory = Path.GetDirectoryName(assetPath);
        if (!Directory.Exists(directory)) {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(assetPath, enumCode.ToString());

        AssetDatabase.Refresh();
        Debug.Log($"ENUM_TAG updated at {assetPath}");
    }
}
#endif
