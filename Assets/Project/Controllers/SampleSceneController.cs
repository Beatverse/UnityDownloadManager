using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleSceneController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DownloadManager.Instance.Load();
    }

    // Update is called once per frame
    void Update()
    {
        DownloadManager.Instance.Update();
    }

    public void Download (string url)
    {
        Debug.Log(url);
        DownloadManager.Instance.Download(url, "test.mp3");
    }
}
