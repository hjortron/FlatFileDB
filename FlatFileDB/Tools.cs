using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using FlatFileDB.Model;
using AppSettings = FlatFileDB.Properties.Settings;

namespace FlatFileDB
{
    static class Tools
    {
        public static long GetApproximateSize(object obj)
        {
            using (Stream stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
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

        public static object Deserialize(FileStream stream)
        {
            try
            {
                var formatter = new BinaryFormatter();

                return formatter.Deserialize(stream);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                throw;
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

        public static string GetDataTableName(int fileN)
        {
            return string.Format(GetFileName(fileN) + AppSettings.Default.DataTableExtension);
        }

        public static string GetInfoTableName(int fileN)
        {
            return string.Format(GetFileName(fileN) + AppSettings.Default.InfoTableExtension);
        }

        public static string GetFileName(int fileN)
        {
            return string.Format(AppSettings.Default.TableName + fileN);
        }

        // MultiDictionary do not support serialization
        public static void InsertOrUpdate(this Dictionary<int, List<long>> dictionary, int key, long value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key].Add(value);
            }
            else
            {
                dictionary.Add(key, new List<long> { value });
            }
        }

        public static IEnumerable<string[]> ParseQuery(string query)
        {
            return
                query.ToLower()
                    .Split(new[] { " and " }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(element => element.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
