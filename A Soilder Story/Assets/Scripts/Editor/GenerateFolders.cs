using UnityEngine;
using System.Collections;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GenerateFolders : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/创建默认文件夹 #&_b")]
    private static void CreateBasicFolder()
    {
        GenerateFolder();
        Debug.Log("Folders Created");
    }

    [MenuItem("Tools/创建全部文件夹")]
    private static void CreateAllFolder()
    {
        GenerateFolder(1);
        Debug.Log("Folders Created");
    }

    private static void GenerateFolder(int flag = 0)
    {
        // 文件路径
        string prjPath = Application.dataPath + "/";
        Directory.CreateDirectory(prjPath + "Resources");
        Directory.CreateDirectory(prjPath + "Scripts");
        Directory.CreateDirectory(prjPath + "Scenes");
        Directory.CreateDirectory(prjPath + "Plugins");
        Directory.CreateDirectory(prjPath + "Resources/Prefabs");
        Directory.CreateDirectory(prjPath + "Resources/Materials");
        Directory.CreateDirectory(prjPath + "Resources/Animator");

        if (1 == flag)
        {
            Directory.CreateDirectory(prjPath + "Meshes");
            Directory.CreateDirectory(prjPath + "Shaders");
            Directory.CreateDirectory(prjPath + "GUI");
        }

        AssetDatabase.Refresh();
    }
#endif
}