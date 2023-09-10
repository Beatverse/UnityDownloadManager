using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System;

public class DownloadOutputMap : Dictionary<string, DownloadOutput>
{
    public string DataFileName = "outputs.xml";

    public List<DownloadOutput> Load(string storagePath)
    {
        string filePath = Path.Join(storagePath, DataFileName);

        if (!File.Exists(filePath))
        {
            return null;
        }

        List<DownloadOutput> deserializedList = null;
        using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<DownloadOutput>));
                deserializedList = (List<DownloadOutput>)xmlSerializer.Deserialize(fileStream);

                foreach (var item in deserializedList)
                {
                    Add(item.FileName, item);
                }
            }
            catch (Exception)
            {
            }
        }

        return deserializedList;
    }

    public void Save(string storagePath)
    {
        List<DownloadOutput> downloadItemList = new List<DownloadOutput>(this.Values);

        string filePath = Path.Join(storagePath, DataFileName);
        using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<DownloadOutput>));
            xmlSerializer.Serialize(fileStream, downloadItemList);
        }
    }

    public void AddDownload (string fileName, string url)
    {
        if (!ContainsKey(fileName))
        {
            Add(fileName, new DownloadOutput(url, fileName));
        }
        else
        {
            this[fileName].Status = DownloadOutput.DownloadStatus.InProgress;
        }
    }
}
