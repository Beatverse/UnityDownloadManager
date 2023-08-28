using System.IO;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class DownloadJobList : List<DownloadJob>
{
    public string DataFileName = "parts.dat";

    public void Load(string storagePath, Action<List<DownloadJob>, DownloadJobData> createAction)
    {
        string filePath = Path.Join(storagePath, DataFileName);

        if (!File.Exists(filePath))
        {
            return;
        }

        using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
        {
            var formatter = new BinaryFormatter();
            var deserializedList = (List<DownloadJobData>)formatter.Deserialize(fileStream);

            foreach (var item in deserializedList)
            {
                createAction(this, item);
            }
        }
    }

    public void Save(string storagePath)
    {
        string filePath = Path.Join(storagePath, DataFileName);
        using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
        {
            var formatter = new BinaryFormatter();
            var serializedList = new List<DownloadJobData>();
            foreach (var item in this)
            {
                serializedList.Add(item.SerializeData());
            }
            formatter.Serialize(fileStream, serializedList);
        }
    }
}