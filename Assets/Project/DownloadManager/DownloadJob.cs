using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class DownloadJobData
{
    public string FileURL;
    public string FileDestination;
    public long TotalBytes;
    public long DownloadedBytes;
}

public class DownloadJob : MonoBehaviour
{
    public enum Result
    {
        InProgress = 0,
        Success = 1,
        WebError = 2,
        FileWriteError = 3
    }

    public long DownloadChunkSize = 1024 * 1024;

    public string FileURL { get; set; }
    public string FileDestination { get; set; }
    public long TotalBytes { get; set; }
    public long DownloadedBytes { get; set; }

    public bool Success
    {
        get
        {
            return DownloadResult == Result.Success;
        }
    }

    public bool IsDone;
    public bool HasStarted;

    public Result DownloadResult;

    private FileStream _fileStream;

    public DownloadJob()
    {
        IsDone = false;
    }

    public void Init (DownloadItem item, string storagePath)
    {
        FileURL = item.FileURL;
        FileDestination = Path.Join(storagePath, item.FileName);
        TotalBytes = 0;
        DownloadedBytes = 0;

        IsDone = false;
    }

    public void Download()
    {
        HasStarted = true;
        StartCoroutine(DownloadFileCoroutine());
    }

    IEnumerator DownloadFileCoroutine()
    {
        if (File.Exists(FileDestination))
        {
            DownloadedBytes = new FileInfo(FileDestination).Length;
        }
        else
        {
            DownloadedBytes = 0;
        }

        UnityWebRequest headRequest = UnityWebRequest.Head(FileURL);
        yield return headRequest.SendWebRequest();

        Debug.Log(headRequest);

        TotalBytes = long.Parse(headRequest.GetResponseHeader("Content-Length"));

        if (DownloadedBytes < TotalBytes)
        {
            _fileStream = new FileStream(FileDestination, FileMode.OpenOrCreate, FileAccess.Write);
            _fileStream.Seek(DownloadedBytes, SeekOrigin.Begin);

            long byteIndex = DownloadedBytes;

            while (byteIndex < TotalBytes)
            {
                int endByte = (int)Mathf.Min(byteIndex + DownloadChunkSize - 1, TotalBytes - 1);

                UnityWebRequest chunkRequest = UnityWebRequest.Get(FileURL);
                chunkRequest.SetRequestHeader("Range", "bytes=" + byteIndex + "-" + endByte);

                yield return chunkRequest.SendWebRequest();

                if (chunkRequest.result == UnityWebRequest.Result.ConnectionError || chunkRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error downloading bytes " + byteIndex + ": " + chunkRequest.error);

                    DownloadResult = Result.WebError;
                    break;
                }
                else
                {
                    try
                    {
                        byte[] chunkData = chunkRequest.downloadHandler.data;
                        _fileStream.Write(chunkData, 0, chunkData.Length);
                        DownloadedBytes += chunkData.Length;

                        Debug.Log("Downloaded bytes " + chunkData.Length);
                        byteIndex += chunkData.Length;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Error saving file: " + ex.Message);
                        DownloadResult = Result.FileWriteError;
                        break;
                    }
                }
            }

            _fileStream.Close();
        }

        Debug.Log("Download complete!");
        IsDone = true;
    }

    public DownloadJobData SerializeData()
    {
        return new DownloadJobData
        {
            FileURL = this.FileURL,
            FileDestination = this.FileDestination,
            TotalBytes = this.TotalBytes,
            DownloadedBytes = this.DownloadedBytes
        };
    }

    public void DeserializeData(DownloadJobData data)
    {
        FileURL = data.FileURL;
        FileDestination = data.FileDestination;
        TotalBytes = data.TotalBytes;
        DownloadedBytes = data.DownloadedBytes;
        IsDone = false;
    }
}
