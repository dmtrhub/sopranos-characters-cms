using System;
using System.IO;
using System.Xml.Serialization;

namespace SopranosCharactersCms.Services
{
    public class XmlDataService
    {
        public T Deserialize<T>(string filePath) where T : class
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (FileStream stream = new FileStream(filePath, FileMode.Open))
            {
                return serializer.Deserialize(stream) as T;
            }
        }

        public void Serialize<T>(T data, string filePath)
        {
            string directoryPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                serializer.Serialize(stream, data);
            }
        }
    }
}
