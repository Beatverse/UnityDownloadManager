using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;

public class DownloadJobList : List<DownloadJob>
{
    public string DataFileName = "parts.xml";

    public void Load(string storagePath)
    {
        string filePath = Path.Join(storagePath, DataFileName);
        using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(DownloadJobList));
            DownloadJobList deserializedQueue = (DownloadJobList)xmlSerializer.Deserialize(fileStream);

            foreach (var item in deserializedQueue)
            {
                Add(item);
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