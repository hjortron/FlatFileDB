using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FlatFileDB.Model
{
    [Serializable]
    class TableInfo
    {
        private readonly List<long> _recordsPositions;
        public readonly long StartOffset;
        private readonly string _fileName;

        [NonSerialized]
        private readonly MemoryStream _fileContent;

        public long Length
        {
            get { return _fileContent.Length; }
        }

        public TableInfo(string fileName, long startPos)
        {         
            if (File.Exists(fileName + ".fti"))
            {
                using (var fileStream = new FileStream(fileName + ".fti", FileMode.OpenOrCreate))
                {
                    var tableInf = (TableInfo) Tools.Deserialize(fileStream);
                    _recordsPositions = tableInf._recordsPositions;
                    StartOffset = tableInf.StartOffset;
                    _fileName = tableInf._fileName;
                }
            }
            else
            {
                _recordsPositions = new List<long>();
                StartOffset = startPos;
                _fileName = fileName;
            }
            _fileContent = new MemoryStream();
            new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite).CopyTo(_fileContent);
        }

        public int Write(Record record)
        {             
            _recordsPositions.Add(_fileContent.Position);
            var recordAsByteArr = record.ToByteArray();
            _fileContent.Write(recordAsByteArr, 0, recordAsByteArr.Length);
            return _recordsPositions.Count - 1;
        }

        public void SaveFile()
        {
            _fileContent.WriteTo(new FileStream(_fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite));
            _fileContent.Close();
            Tools.Serialize(_fileName + ".fti", this);
        }

        internal IEnumerable<string> GetRecords(IEnumerable<long> recordsPos)
        {
            var result = new List<string>();
            var b = new BinaryReader(_fileContent);

            foreach (var id in recordsPos)
            {
                var index = (int)(id - StartOffset);
                var recordPos = _recordsPositions.ElementAt(index);
                if (b.BaseStream.Length > recordPos)
                {
                    var bytesCount = _recordsPositions[index + 1] - recordPos;
                    _fileContent.Position = recordPos;
                    b.BaseStream.Seek(recordPos, SeekOrigin.Begin);
                    var v = b.ReadBytes((int)bytesCount);
                    result.Add(Tools.FromByteArray(v).ToString());
                }
            }
            return result;
        }
    }
}
