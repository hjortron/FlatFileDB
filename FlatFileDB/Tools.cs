using FlatFileDB.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
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

        public static void Serialize(string fileName, object obj)
        {
            var binFormat = new BinaryFormatter();
            using (var fStream = new FileStream(fileName,
               FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                binFormat.Serialize(fStream, obj);
            }
        }

        public static object Deserialize(string filePath)
        {
            var fs = new FileStream(filePath, FileMode.Open);
            try
            {
                var formatter = new BinaryFormatter();

                
                return formatter.Deserialize(fs);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }           
        }

        public static Record FromByteArray(byte[] arrBytes)
        {
            var memStream = new MemoryStream();
            var binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            var record = (Record)binForm.Deserialize(memStream);
            return record;
        }
    }
}
