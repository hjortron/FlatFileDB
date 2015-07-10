using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace FlatFileDB
{
    static class Tools
    {
        public static long GetApproximateSize(object obj)
        {
            using (Stream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                return stream.Length;
            }
        }

        public static void Serialize<T>(string fileName, T obj)
        {
            var binFormat = new BinaryFormatter();
            using (var fStream = new FileStream(fileName,
               FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                binFormat.Serialize(fStream, obj);
            }
        }

        public static T Deserialize<T>(string filePath)
        {
            var binFormat = new BinaryFormatter();
            using (var fStream = new FileStream(filePath,
               FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return (T)binFormat.Deserialize(fStream);              
            }
        }
    }
}
