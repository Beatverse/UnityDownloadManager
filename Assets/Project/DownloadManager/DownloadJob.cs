using System;

[Serializable]
public class DownloadJob
{
    public string FileUri { get; set; }

    public bool IsDone;

    public DownloadJob(DownloadItem item)
    {
        FileUri = item.FileUri;

        IsDone = false;
    }
}