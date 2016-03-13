using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Text;
using System.Security.Cryptography;

/*
 *	
 * by Liangjx
 *
 */

public class Util
{
    /// <summary>
    /// 数据目录
    /// </summary>
    public static string AppDataPath
    {
        get { return Application.dataPath.ToLower(); }
    }
    /// <summary>
    /// 取得数据存放目录
    /// </summary>
    public static string DataPath
    {
        get
        {
            string game = AppConst.AppName.ToLower();
            if (Application.isMobilePlatform)
            {
                return Application.persistentDataPath + "/" + game + "/";
            }
            if (AppConst.IdDebugModel)
            {
                return Application.dataPath + "/" + AppConst.AssetDirname + "/";
            }
            return "c:/" + game + "/";
        }
    }

    public static string GetStreamingAssetsPath()
    {
        if (Application.isEditor)
        {
            return "file://" + System.Environment.CurrentDirectory.Replace("\\", "/"); // Use the build output folder directly.
        }
        else if (Application.isWebPlayer)
        {
            return System.IO.Path.GetDirectoryName(Application.absoluteURL).Replace("\\", "/") + "/StreamingAssets";
        }
        else if (Application.isMobilePlatform || Application.isConsolePlatform)
        {
            return Application.streamingAssetsPath;
        }
        else // For standalone player.
        {
            return "file://" + Application.streamingAssetsPath;
        }
    }

    public static string GetRelativePath()
    {
        if (Application.isEditor)
        {
            return "file://" + System.Environment.CurrentDirectory.Replace("\\", "/") + "/Assets/" + AppConst.AssetDirname + "/";
        }
        else if (Application.isMobilePlatform || Application.isConsolePlatform)
        {
            return "file:///" + DataPath;
        }
        else // For standalone player.
        {
            return "file://" + Application.streamingAssetsPath + "/";
        }
    }

    /// <summary>
    /// 计算文件的MD5值
    /// </summary>
    public static string md5file(string file)
    {
        try
        {
            FileStream fs = new FileStream(file, FileMode.Open);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(fs);
            fs.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("md5file() fail, error:" + ex.Message);
        }
    }
}
