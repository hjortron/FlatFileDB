using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace FlatFileDB.Model
{
    [Serializable]
    class TableInfo
    {
        public long _startPos;
        public long _endPos;
        public string _fileName;

        [NonSerialized]
        private StreamWriter _fileContent;

        public long Length
        {
            get { return _fileContent.BaseStream.Length; }
        }

        public TableInfo(string fileName, long startPos, long? endPos = null)
        {
            var stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);            
            _fileContent = new StreamWriter(stream);            
            _startPos = startPos;
            _endPos = endPos == null ? startPos : (long)endPos;
            _fileName = fileName;
        }

        public void Write(Record record)
        {
            _fileContent.WriteLine(record);
            _endPos = record.recordId;
        }

        public void SaveFile()
        {
            //Tools.Serialize(_fileName + ".fti", this);
        }

        internal void GetRecords(IEnumerable<long> recordsIds)
        {
            var result = new List<Record>();         
            foreach(var id in recordsIds)
            {               
                using (var b = new BinaryReader(_fileContent.BaseStream))
                {
                    // 2.
                    // Position and length variables.
                    //_fileContent.BaseStream.Position = id - _startPos;                  
                    int pos = 0;
                    // 2A.
                    // Use BaseStream.
                    var length = b.BaseStream.Length;
                    while (pos < length)
                    {
                        // 3.
                        // Read integer.
                        var v = b.ReadString();
                        Console.WriteLine(v);

                        // 4.
                        // Advance our position variable.
                        pos += sizeof(int);
                    }
                }
               // Console.WriteLine(line);
            }
        }
    }
}
