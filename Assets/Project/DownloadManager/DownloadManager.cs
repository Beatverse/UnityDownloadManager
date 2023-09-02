using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class DownloadManager : MonoBehaviour
{
    private static DownloadManager _instance;

    public static DownloadManager Instance
    {
        get
        {
            return _instance;
        }
    }

    public string StoragePath = "Downloads";
    public int MaxConcurrencyJobs = 3;

    private DownloadItemQueue _downloadItemQueue;
    private DownloadJobList _downloadJobList;

    private bool _downloadItemQueueChanged;
    private Dictionary<string, DownloadOutput> _downloadOutputs;

    public void Awake()
    {
        StoragePath = Path.Join(Application.persistentDataPath, StoragePath);
        Debug.Log("DOWNLOAD PATH: " + StoragePath);

        _instance = this;

        _downloadItemQueue = new DownloadItemQueue();
        _downloadJobList = new DownloadJobList();
        _downloadOutputs = new Dictionary<string, DownloadOutput>();

        _downloadItemQueueChanged = false;
    }

    public DownloadOutput GetDownloadStatus (string fileName)
    {
        return _downloadOutputs[fileName];
    }

    public void Load()
    {
         // Check if the directory doesn't exist
        if (!Directory.Exists(StoragePath))
        {
            try
            {
                Directory.CreateDirectory(StoragePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }

        var downloadList = _downloadItemQueue.Load(StoragePath);
        foreach (var downloadItem in downloadList)
        {
            _downloadOutputs[downloadItem.FileName] = new DownloadOutput(downloadItem.FileURL, downloadItem.FileName);
        }

        _downloadJobList.Load(StoragePath, (list, item) => {
            DeserializeJob(list, item);
        });

        _downloadItemQueueChanged = false;
    }

    public void Update()
    {
        bool jobListChanged = _downloadJobList.Count > 0;

        for (int i = _downloadJobList.Count - 1; i >= 0; --i)
        {
            var job = _downloadJobList[i];

            if (!_downloadOutputs.ContainsKey(job.FileName))
            {
                _downloadOutputs.Add(job.FileName, new DownloadOutput(job.FileURL, job.FileName));
            }

            _downloadOutputs[job.FileName].DownloadedBytes = job.DownloadedBytes;
            _downloadOutputs[job.FileName].TotalBytes = job.TotalBytes;

            if (job.IsDone)
            {
                if (job.Success)
                {
                    _downloadOutputs[job.FileName].Status = DownloadOutput.DownloadStatus.Success;
                }
                else
                {
                    _downloadOutputs[job.FileName].Status = DownloadOutput.DownloadStatus.Error;
                }

                Destroy(job.gameObject);
                _downloadJobList.RemoveAt(i);
            }
            else if (!job.HasStarted)
            {
                _downloadOutputs[job.FileName].Status = DownloadOutput.DownloadStatus.InProgress;
                job.Download();
            }
        }

        while (_downloadJobList.Count < MaxConcurrencyJobs)
        {
            if (_downloadItemQueue.Count > 0)
            {
                var item = _downloadItemQueue.Dequeue();

                GameObject go = new GameObject("Download", typeof(DownloadJob));
                go.transform.parent = transform;

                var job = go.GetComponent<DownloadJob>();
                job.Init(item, StoragePath);

                _downloadJobList.Add(job);
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

    public void Download (string url, string fileName)
    {
        _downloadItemQueue.Enqueue(new DownloadItem(url, fileName));

        if (!_downloadOutputs.ContainsKey(fileName))
        {
            _downloadOutputs.Add(fileName, new DownloadOutput(url, fileName));
        }
        else
        {
            _downloadOutputs[fileName].Status = DownloadOutput.DownloadStatus.InProgress;
        }
    }

    public void DeserializeJob (List<DownloadJob> list, DownloadJobData data)
    {
        GameObject go = new GameObject("Download", typeof(DownloadJob));
        go.transform.parent = transform;

        var job = go.GetComponent<DownloadJob>();
        job.DeserializeData(data);

        list.Add(job);
    }
}
