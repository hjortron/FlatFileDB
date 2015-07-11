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
        public List<long> recordsPositions;
        public long _startOffset;
        public string _fileName;

        [NonSerialized]
        private MemoryStream _fileContent;

        public long Length
        {
            get { return _fileContent.Length; }
        }

        public TableInfo(string fileName, long startPos)
        {
            if (File.Exists(fileName + ".fti"))
            {
                var tableInf = (TableInfo)Tools.Deserialize(fileName + ".fti");
                recordsPositions = tableInf.recordsPositions;
                _startOffset = tableInf._startOffset;
                _fileName = tableInf._fileName;
            }
            else
            {
                recordsPositions = new List<long>();
                _startOffset = startPos;
                _fileName = fileName;
            }
            _fileContent = new MemoryStream();
            new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite).CopyToAsync(_fileContent);
        }

        public int Write(Record record)
        {             
            recordsPositions.Add(_fileContent.Position);
            var recordAsByteArr = record.ToByteArray();
            _fileContent.Write(recordAsByteArr, 0, recordAsByteArr.Length);
            return recordsPositions.Count - 1;
        }

        public void SaveFile()
        {
            _fileContent.WriteTo(new FileStream(_fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite));
            _fileContent.Close();
            Tools.Serialize(_fileName + ".fti", this);
        }

        internal IEnumerable<Record> GetRecords(IEnumerable<long> recordsPos)
        {
            var result = new List<Record>();
            var b = new BinaryReader(_fileContent);

            foreach (var id in recordsPos)
            {
                var index = (int)(id - _startOffset);
                var recordPos = recordsPositions.ElementAt((int)index);
                if (b.BaseStream.Length > recordPos)
                {
                    var bytesCount = recordsPositions[index + 1] - recordPos;
                    _fileContent.Position = recordPos;
                    b.BaseStream.Seek(recordPos, SeekOrigin.Begin);
                    var v = b.ReadBytes((int)bytesCount);
                    Console.WriteLine(Tools.FromByteArray(v));
                }
            }
            return result;
        }
    }
}
