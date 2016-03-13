using UnityEngine;
using System.Collections;
using System.IO;
using GameEvent;
using AssetBundles;

/*
 *	
 * by Liangjx
 *
 */

public class AppStart : MonoBehaviour {
    private UpdateWorkder updateWorker;
    void Awake()
    {
        updateWorker = UpdateWorkder.Initialize();
        DontDestroyOnLoad(gameObject);  //防止销毁自己
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = AppConst.GameFrameRate;
    }

    IEnumerator OnUpdateResource()
    {
        string dataPath = Util.DataPath;  //数据目录
        string url = AppConst.WebUrl + AppConst.AppName + "/";
        string listUrl = url + "files.txt";
        WWW www = new WWW(listUrl); yield return www;
        if (www.error != null)
        {
            yield break;
        }
        if (!Directory.Exists(dataPath))
        {
            Directory.CreateDirectory(dataPath);
        }
        File.WriteAllBytes(dataPath + "files.txt", www.bytes);
        string filesText = www.text;
        string[] files = filesText.Split('\n');

        for (int i = 0; i < files.Length; i++)
        {
            if (string.IsNullOrEmpty(files[i]))
            {
                continue;
            }
            string[] keyValue = files[i].Split('|');
            string f = keyValue[0];
            string localfile = (dataPath + f).Trim();
            string path = Path.GetDirectoryName(localfile);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string fileUrl = url + f;
            bool canUpdate = !File.Exists(localfile);
            if (!canUpdate)
            {
                string remoteMd5 = keyValue[1].Trim();
                string localMd5 = Util.md5file(localfile);
                canUpdate = !remoteMd5.Equals(localMd5);
                if (canUpdate)
                {
                    File.Delete(localfile);
                }
            }
            if (canUpdate)
            {   
                //这里都是资源文件，用线程下载
                BeginDownload(fileUrl, localfile);
            }
        }

        while (!updateWorker.IsDownFinish())
        {
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForEndOfFrame();
        StartGame();
    }

    void BeginDownload(string url, string file)
    {     
        EventDispatcher.TriggerEvent<string, string>(EventConst.ADD_DOWNLOAD_FILE, url, file);
    }

	// Use this for initialization
	void Start () 
    {
        StartCoroutine(OnUpdateResource());
	}

    /// <summary>
    /// 所有资源下载完，可以开始游戏
    /// </summary>
    private void StartGame()
    {
        Debug.Log("所有资源下载完，可以开始游戏");
        StartCoroutine(Initialize());
    }

    protected IEnumerator Initialize()
    {
        var request = AssetBundleManager.Initialize();
        if (request != null)
        {
            yield return StartCoroutine(request);
        }
        yield return StartCoroutine(InstantiateGameObjectAsync("cube.assetbundle", "cube"));
        yield return StartCoroutine(InitializeLevelAsync("testscene.assetbundle", "testscene", true));
        yield return StartCoroutine(InstantiateGameObjectAsync("dragon.assetbundle", "death"));
    }

    protected IEnumerator InstantiateGameObjectAsync(string assetBundleName, string assetName)
    {
        float startTime = Time.realtimeSinceStartup;
        AssetBundleLoadAssetOperation request = AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(GameObject));
        if (request == null)
        {
            yield break;
        }
        yield return StartCoroutine(request);

        // Get the asset.
        GameObject prefab = request.GetAsset<GameObject>();

        if (prefab != null)
        {
            GameObject.Instantiate(prefab);
        }

        float elapsedTime = Time.realtimeSinceStartup - startTime;
        Debug.Log(assetName + (prefab == null ? " was not" : " was") + " loaded successfully in " + elapsedTime + " seconds");
    }

    protected IEnumerator InitializeLevelAsync(string sceneAssetBundle, string levelName, bool isAdditive)
    {
        float startTime = Time.realtimeSinceStartup;

        AssetBundleLoadOperation request = AssetBundleManager.LoadLevelAsync(sceneAssetBundle, levelName, isAdditive);
        if (request == null)
        {
            yield break;
        }
        yield return StartCoroutine(request);

        float elapsedTime = Time.realtimeSinceStartup - startTime;
        Debug.Log("Finished loading scene " + levelName + " in " + elapsedTime + " seconds");
    }
}
