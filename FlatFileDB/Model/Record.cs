using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace FlatFileDB.Model
{
    public interface IRecord
    {
    }
    [Serializable]
    public class Record : IRecord
    {      
        public int sourceId;
        public int sourceType;
        public byte[] data;
        public string timestamp;

        public Record(int sourceId, int sourceType, byte[] data)
        {
            // TODO: Complete member initialization          
            this.timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            this.sourceId = sourceId;
            this.sourceType = sourceType;
            this.data = data;
        }

        override public string ToString()
        {
            return String.Format("{0}|{1}|{2}|\"{3}\"", timestamp, sourceId, sourceType, System.Text.Encoding.Default.GetString(data));
        }

        public byte[] ToByteArray()
        {
            var bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            using (var ms = new System.IO.MemoryStream())
            {
                bf.Serialize(ms, this);
                return ms.ToArray();
            }
        }        
    }
}
