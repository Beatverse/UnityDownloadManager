using System;

[Serializable]
public class DownloadItem
{
    public string FileURL { get; set; }
    public string FileName { get; set; }

    public DownloadItem(string url, string fileName)
    {
        FileURL = url;
        FileName = fileName;
    }
}