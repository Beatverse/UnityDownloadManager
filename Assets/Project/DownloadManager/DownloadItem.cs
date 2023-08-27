using System;

[Serializable]
public class DownloadItem
{
    public string FileUri { get; set; }

    public DownloadItem(string uri)
    {
        FileUri = uri;
    }
}