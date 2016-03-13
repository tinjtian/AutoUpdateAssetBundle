using UnityEngine;
using System.Threading;
using System.Collections.Generic;
using System.Net;
using System.ComponentModel;
using GameEvent;

/*
 *	
 * by Liangjx
 *
 */

public class DownLoadFileData
{
    public string url = "";
    public string fileName = "";
    public DownLoadFileData(string url, string fileName)
    {
        this.url = url;
        this.fileName = fileName;
    }
}

public class UpdateWorkder : MonoBehaviour {
    private Thread thread;
    static readonly object m_lockObj = new object();
    private string currDownFile = string.Empty;
    private List<string> totalDownFile;
    private int totalFinishCount = 0;
    private Queue<DownLoadFileData> dataQueue = new Queue<DownLoadFileData>(); 

    public static UpdateWorkder Initialize()
    {
        var go = new GameObject("UpdateWorkder");
        DontDestroyOnLoad(go);
        UpdateWorkder worker = go.GetComponent<UpdateWorkder>() ??  go.AddComponent<UpdateWorkder>();
        return worker;
    }

    private void RegisterEvent()
    {
        EventDispatcher.AddEventListener<string, string>(EventConst.ADD_DOWNLOAD_FILE, AddDownLoadFile);
    }

    private void UnRegisterEvent()
    {
        EventDispatcher.RemoveEventListener<string, string>(EventConst.ADD_DOWNLOAD_FILE, AddDownLoadFile);
    }

    void Awake()
    {
        thread = new Thread(OnUpdate);
    }

	// Use this for initialization
	void Start () 
    {
        totalDownFile = new List<string>();
        RegisterEvent();
        thread.Start();
	}

    void OnUpdate()
    {
        while (true)
        {
            lock (m_lockObj)
            {
                if (dataQueue.Count > 0)
                {
                    DownLoadFileData data = dataQueue.Dequeue();
                    OnDownloadFile(data.url, data.fileName);
                }   
            }
            Thread.Sleep(1);
        }
    }

    /// <summary>
    /// 下载文件
    /// </summary>
    void OnDownloadFile(string url, string path)
    {
        currDownFile = path;
        using (WebClient client = new WebClient())
        {
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompleted);
            client.DownloadFileAsync(new System.Uri(url), currDownFile);
        }
    }

    /// <summary>
    /// 下载完成
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
        if (e.Error == null && e.Cancelled == false)
        {
            totalFinishCount += 1;
            Debug.Log("下载成功");
        }
        else
        {
            Debug.LogError("下载失败,原因:" + e.Error.Message);
        }
    }

    /// <summary>
    /// 下载进度改变
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
    }

    /// <summary>
    /// 应用程序退出
    /// </summary>
    void OnDestroy()
    {
        thread.Abort();
        UnRegisterEvent();
    }

    private void AddDownLoadFile(string url, string fileName)
    {
        lock (m_lockObj)
        {
            totalDownFile.Add(url);
            DownLoadFileData data = new DownLoadFileData(url, fileName);
            dataQueue.Enqueue(data);
        }
    }

    public bool IsDownFinish()
    {
        return totalDownFile.Count == totalFinishCount;
    }
}
