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

    public string remoteStorageUrl = "https://beatcube-demo-public.s3.ap-southeast-1.amazonaws.com/musics";
    public string StoragePath = "Downloads";
    public int MaxConcurrencyJobs = 3;

    private DownloadItemQueue _downloadItemQueue;
    private DownloadJobList _downloadJobList;

    private bool _downloadItemQueueChanged;
    private bool _downloadOutputsChanged;
    private DownloadOutputMap _downloadOutputs;

    public void Awake()
    {
        StoragePath = Path.Join(Application.persistentDataPath, StoragePath);
        Debug.Log("DOWNLOAD PATH: " + StoragePath);

        _instance = this;

        _downloadItemQueue = new DownloadItemQueue();
        _downloadJobList = new DownloadJobList();
        _downloadOutputs = new DownloadOutputMap();

        _downloadItemQueueChanged = false;
        _downloadOutputsChanged = false;
    }

    public DownloadOutput GetDownloadStatus (string fileName)
    {
        if (!_downloadOutputs.ContainsKey(fileName))
        {
            return null;
        }
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

        _downloadItemQueue.Load(StoragePath);

        _downloadJobList.Load(StoragePath, (list, item) => {
            DeserializeJob(list, item);
        });

        _downloadOutputs.Load(StoragePath);

        _downloadItemQueueChanged = false;
        _downloadOutputsChanged = false;
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
                _downloadOutputsChanged = true;
            }

            if (_downloadOutputs[job.FileName].TotalBytes != job.TotalBytes)
            {
                _downloadOutputsChanged = true;
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

                _downloadOutputsChanged = true;
            }
            else if (!job.HasStarted)
            {
                _downloadOutputs[job.FileName].Status = DownloadOutput.DownloadStatus.InProgress;
                job.Download();

                _downloadOutputsChanged = true;
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

        if (_downloadOutputsChanged)
        {
            _downloadOutputs.Save(StoragePath);
        }
    }

    public void Download (string filePath, string fileName)
    {
        string url = remoteStorageUrl + "/" + filePath;

        _downloadItemQueue.Enqueue(new DownloadItem(url, fileName));
        _downloadOutputs.AddDownload(fileName, url);

        _downloadItemQueueChanged = true;
        _downloadOutputsChanged = true;
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
