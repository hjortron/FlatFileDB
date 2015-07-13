using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace FlatFileDB.Model
{
    public interface IRecord
    {
    }

    [Serializable]
    public class Record : IRecord
    {      
        public readonly int SourceId;
        public readonly int SourceType;
        private readonly byte[] _data;
        private readonly string _timestamp;

        public Record(int sourceId, int sourceType, byte[] data)
        {              
            _timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            SourceId = sourceId;
            SourceType = sourceType;
            _data = data;
        }

        override public string ToString()
        {
            return string.Format("{0}|{1}|{2}|\"{3}\"", _timestamp, SourceId, SourceType, Convert.ToBase64String(_data));
        }

        public byte[] ToByteArray()
        {
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, this);
                return ms.ToArray();
            }
        }        
    }
}
