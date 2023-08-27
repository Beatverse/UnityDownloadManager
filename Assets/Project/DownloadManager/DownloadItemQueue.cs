using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;

public class DownloadItemQueue : Queue<DownloadItem>
{
    public string DataFileName = "downloads.xml";

    public void Load(string storagePath)
    {
        string filePath = Path.Join(storagePath, DataFileName);
        using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(DownloadItemQueue));
            DownloadItemQueue deserializedQueue = (DownloadItemQueue)xmlSerializer.Deserialize(fileStream);

            foreach (var item in deserializedQueue)
            {
                Enqueue(item);
            }
        }
    }

    public void Save(string storagePath)
    {
        string filePath = Path.Join(storagePath, DataFileName);
        using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(DownloadItemQueue));
            xmlSerializer.Serialize(fileStream, this);
        }
    }
}