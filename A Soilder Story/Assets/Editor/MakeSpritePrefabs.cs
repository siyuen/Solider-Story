using System.IO;
using UnityEditor;
using UnityEngine;

public class MakeSpritePrefabs {

    //小图目录
    private const string ORIGIN_DIR = "\\Atlas";
    //预制体目录
    private const string TARGET_DIR = "\\Resources\\Sprites";
#if UNITY_EDITOR
    [MenuItem("Tools/MakeSpritePrefabs")]
    private static void MakePrefabs()
    {
        EditorUtility.DisplayProgressBar("Make Sprite Prefabs", "Please wait...", 1);

        string targetDir = Application.dataPath + TARGET_DIR;
        //删除目标目录
        if (Directory.Exists(targetDir))
            Directory.Delete(targetDir, true);
        if (File.Exists(targetDir + ".meta"))
            File.Delete(targetDir + ".meta");
        //创建空的目标目录
        if (!Directory.Exists(targetDir))
            Directory.CreateDirectory(targetDir);

        //获取源目录的所有图片资源并处理
        string originDir = Application.dataPath + ORIGIN_DIR;
        DirectoryInfo originDirInfo = new DirectoryInfo(originDir);
        //MakeSpritePrefabsProcess(originDirInfo.GetFiles("*.jpg", SearchOption.AllDirectories), targetDir);
        MakeSpritePrefabsProcess(originDirInfo.GetFiles("*.png", SearchOption.AllDirectories), targetDir);

        EditorUtility.ClearProgressBar();
    }

    static private void MakeSpritePrefabsProcess(FileInfo[] files, string targetDir)
    {
        foreach (FileInfo file in files)
        {
            string allPath = file.FullName;
            string assetPath = allPath.Substring(allPath.IndexOf("Assets"));
            //加载贴图
            Sprite sprite = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Sprite)) as Sprite;
            //创建绑定了贴图的 GameObject 对象
            GameObject go = new GameObject(sprite.name);
            go.AddComponent<SpriteRenderer>().sprite = sprite;
            //获取目标路径
            string targetPath = assetPath.Replace("Assets" + ORIGIN_DIR + "\\", "");
            //去掉后缀
            targetPath = targetPath.Substring(0, targetPath.IndexOf("."));
            //得到最终路径
            targetPath = targetDir + "\\" + targetPath + ".prefab";
            //得到应用当前目录的路径
            string prefabPath = targetPath.Substring(targetPath.IndexOf("Assets"));
            //创建目录
            Directory.CreateDirectory(prefabPath.Substring(0, prefabPath.LastIndexOf("\\")));
            //生成预制件
            PrefabUtility.CreatePrefab(prefabPath.Replace("\\", "/"), go);
            //销毁对象
            GameObject.DestroyImmediate(go);
        }
    }
#endif
}
