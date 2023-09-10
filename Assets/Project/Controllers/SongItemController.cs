using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SongItemController : MonoBehaviour
{
    public string SongName;
    public string FileType;

    public TMP_Text TitleText;
    public TMP_Text StatusText;
    public TMP_Text ButtonText;

    // Start is called before the first frame update
    void Start()
    {
        TitleText.text = SongName;
    }

    // Update is called once per frame
    void Update()
    {
        string fileName = SongName + "." + FileType;
        var downloadStatus = DownloadManager.Instance.GetDownloadStatus(fileName);

        if (downloadStatus != null)
        {
            StatusText.text = downloadStatus.Status.ToString();
            if (downloadStatus.Status == DownloadOutput.DownloadStatus.Success)
            {
                ButtonText.text = "Play";
            }
            else if (downloadStatus.Status == DownloadOutput.DownloadStatus.InProgress)
            {
                ButtonText.text = "...";
            }
            else
            {
                ButtonText.text = "Download";
            }
        }
        else
        {
            StatusText.text = "";
            ButtonText.text = "Download";
        }
    }

    public void Action ()
    {
        string fileName = SongName + "." + FileType;
        var downloadStatus = DownloadManager.Instance.GetDownloadStatus(fileName);
        if (downloadStatus != null && downloadStatus.Status == DownloadOutput.DownloadStatus.InProgress)
        {
            return;
        }

        if (downloadStatus != null)
        {
            if (downloadStatus.Status == DownloadOutput.DownloadStatus.Success)
            {

            }
            else
            {
                DownloadManager.Instance.Download(SongName + "/" + fileName, fileName);
            }
        }
        else
        {
            DownloadManager.Instance.Download(SongName + "/" + fileName, fileName);
        }
    }
}
