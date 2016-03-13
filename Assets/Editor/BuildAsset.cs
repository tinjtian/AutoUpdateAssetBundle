using System.IO;
using UnityEditor;
using System.Collections.Generic;
/*
 *	
 * by Liangjx
 *
 */
public class BuildAsset{
    static List<string> paths = new List<string>();
    static List<string> files = new List<string>();

    [MenuItem("GameTool/Build iPhone Resource", false, 11)]
    public static void BuildiPhoneResource()
    {
        BuildTarget target;
        target = BuildTarget.iOS;
        BuildAssetResource(target);
    }

    [MenuItem("GameTool/Build Android Resource", false, 12)]
    public static void BuildAndroidResource()
    {
        BuildAssetResource(BuildTarget.Android);
    }

    [MenuItem("GameTool/Build Windows Resource", false, 13)]
    public static void BuildWindowsResource()
    {
        BuildAssetResource(BuildTarget.StandaloneWindows);
    }

    public static void BuildAssetResource(BuildTarget target)
    {
        string dataPath = Util.DataPath;
        if (Directory.Exists(dataPath))
        {
            Directory.Delete(dataPath, true);
        }
        string resPath = Util.AppDataPath + "/" + AppConst.AssetDirname + "/";
        if (!Directory.Exists(resPath))
        {
            Directory.CreateDirectory(resPath);
        }

        //生成assetbundle
        BuildPipeline.BuildAssetBundles(resPath, BuildAssetBundleOptions.None, target);

        paths.Clear(); 
        files.Clear();
        EditorUtility.ClearProgressBar();

        ///----------------------创建文件列表-----------------------
        string newFilePath = resPath + "/files.txt";
        if (File.Exists(newFilePath)) File.Delete(newFilePath);

        paths.Clear(); 
        files.Clear();
        Recursive(resPath);

        FileStream fs = new FileStream(newFilePath, FileMode.CreateNew);
        StreamWriter sw = new StreamWriter(fs);
        for (int i = 0; i < files.Count; i++)
        {
            string file = files[i];
            if (file.EndsWith(".meta") || file.Contains(".DS_Store"))
            {
                continue;
            }

            string md5 = Util.md5file(file);
            string value = file.Replace(resPath, string.Empty);
            sw.WriteLine(value + "|" + md5);
        }
        sw.Close(); 
        fs.Close();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 遍历目录及其子目录
    /// </summary>
    static void Recursive(string path)
    {
        string[] names = Directory.GetFiles(path);
        string[] dirs = Directory.GetDirectories(path);
        foreach (string filename in names)
        {
            string ext = Path.GetExtension(filename);
            if (ext.Equals(".meta")) continue;
            files.Add(filename.Replace('\\', '/'));
        }
        foreach (string dir in dirs)
        {
            paths.Add(dir.Replace('\\', '/'));
            Recursive(dir);
        }
    }
}
