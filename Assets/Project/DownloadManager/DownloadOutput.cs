using System;

[Serializable]
public class DownloadOutput
{
    public enum DownloadStatus
    {
        Pending = 0,
        InProgress = 1,
        Success = 2,
        Error = 3

    }

    public string FileURL { get; set; }
    public string FileName { get; set; }
    public long DownloadedBytes { get; set; }
    public long TotalBytes { get; set; }
    public DownloadStatus Status { get; set; }

    public DownloadOutput()
    {
        FileURL = "";
        FileName = "";
        DownloadedBytes = 0;
        TotalBytes = 0;
        Status = DownloadStatus.Pending;
    }

    public DownloadOutput(string url, string fileName)
    {
        FileURL = url;
        FileName = fileName;
        DownloadedBytes = 0;
        TotalBytes = 0;
        Status = DownloadStatus.Pending;
    }
}