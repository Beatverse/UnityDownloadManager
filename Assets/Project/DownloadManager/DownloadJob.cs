using System;
using System.Collections;
using System.IO;
using Unity.VisualScripting;
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
    public string FileName { get; set; }
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
        FileName= item.FileName;
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

    public void OnApplicationQuit ()
    {
        Debug.Log("QUIT JOB");
        StopAllCoroutines();
        if (_fileStream != null)
        {
            _fileStream.Close();
        }
        IsDone = true;
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

        TotalBytes = long.Parse(headRequest.GetResponseHeader("Content-Length"));

        if (DownloadedBytes < TotalBytes)
        {
            try
            {
                _fileStream = new FileStream(FileDestination, FileMode.OpenOrCreate, FileAccess.Write);
                _fileStream.Seek(DownloadedBytes, SeekOrigin.Begin);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
                DownloadResult = Result.FileWriteError;
                _fileStream = null;
            }

            if (_fileStream != null)
            {
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
                            long chunkLength = chunkData.Length;
                            _fileStream.Write(chunkData, 0, (int)chunkLength);
                            DownloadedBytes += chunkLength;

                            Debug.Log("Downloaded bytes " + chunkLength);
                            byteIndex += chunkLength;
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError("Error saving file: " + ex.Message);
                            DownloadResult = Result.FileWriteError;
                            break;
                        }
                    }

                    if (IsDone)
                    {
                        break;
                    }
                }

                _fileStream.Close();
                DownloadResult = Result.Success;
            }
        }
        else
        {
            DownloadResult = Result.Success;
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
