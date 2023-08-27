using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class DownloadManager
{
    private static DownloadManager _instance;

    public static DownloadManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new DownloadManager();
            }
            return _instance;
        }
    }

    public string StoragePath = Path.Join(Application.persistentDataPath, "Downloads");
    public int MaxConcurrencyJobs = 3;

    private DownloadItemQueue _downloadItemQueue;
    private DownloadJobList _downloadJobList;

    private bool _downloadItemQueueChanged;

    public DownloadManager()
    {
        _downloadItemQueue = new DownloadItemQueue();
        _downloadJobList = new DownloadJobList();

        _downloadItemQueueChanged = false;
    }

    public void Load()
    {
        _downloadItemQueue.Load(StoragePath);
        _downloadJobList.Load(StoragePath);

        _downloadItemQueueChanged = false;
    }

    public void Update()
    {
        bool jobListChanged = false;

        for (int i = _downloadJobList.Count - 1; i >= 0; --i)
        {
            if (_downloadJobList[i].IsDone)
            {
                _downloadJobList.RemoveAt(i);
                jobListChanged = true;
            }
        }

        while (_downloadJobList.Count < MaxConcurrencyJobs)
        {
            if (_downloadItemQueue.Count > 0)
            {
                var item = _downloadItemQueue.Dequeue();
                _downloadJobList.Add(new DownloadJob(item));
                jobListChanged = true;
            }
            else
            {
                break;
            }
        }

        if (jobListChanged)
        {
            _downloadJobList.Save(StoragePath);
        }

        if (_downloadItemQueueChanged)
        {
            _downloadItemQueue.Save(StoragePath);
        }
    }

    public void Download (string uri)
    {
        _downloadItemQueue.Enqueue(new DownloadItem(uri));
    }
}
