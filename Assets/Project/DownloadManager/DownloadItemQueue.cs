using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System;

public class DownloadItemQueue : Queue<DownloadItem>
{
    public string DataFileName = "downloads.xml";

    public List<DownloadItem> Load(string storagePath)
    {
        string filePath = Path.Join(storagePath, DataFileName);

        if (!File.Exists(filePath))
        {
            return null;
        }

        List<DownloadItem> deserializedList = null;
        using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<DownloadItem>));
            deserializedList = (List<DownloadItem>)xmlSerializer.Deserialize(fileStream);

            foreach (var item in deserializedList)
            {
                Enqueue(item);
            }
        }

        return deserializedList;
    }

    public void Save(string storagePath)
    {
        List<DownloadItem> downloadItemList = new List<DownloadItem>(this);

        string filePath = Path.Join(storagePath, DataFileName);
        using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<DownloadItem>));
            xmlSerializer.Serialize(fileStream, downloadItemList);
        }
    }
}