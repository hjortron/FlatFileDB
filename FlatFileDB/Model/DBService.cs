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
    public class DBService
    {
        const int FILE_SIZE = 1024 * 1024 * 2;
        private int _recordsCount;
        private int _tablesCount;
        private Dictionary<int, List<long>> _sourceIds;
        private Dictionary<int, List<long>> _sourceTypes;
        private Dictionary<string, long[]> _tables;

        [NonSerialized]
        private TableInfo _currentFile;

        public DBService()
        {
            if (File.Exists("dbInf.dbi"))
            {
                var dbInf = Tools.Deserialize<DBService>("dbInf.dbi");
                _recordsCount = dbInf._recordsCount;
                _sourceIds = dbInf._sourceIds;
                _sourceTypes = dbInf._sourceTypes;
                _tables = dbInf._tables;
                _tablesCount = dbInf._tablesCount;
            }
            else
            {
                _sourceIds = new Dictionary<int, List<long>>();
                _sourceTypes = new Dictionary<int, List<long>>();
                _tables = new Dictionary<string, long[]>();
            }

            _currentFile = new TableInfo(String.Format("table{0}.ftd", _tablesCount), _recordsCount);
        }

        public void CreateFile()
        {
            _currentFile = new TableInfo(String.Format("table{0}.ftd", _tablesCount), _recordsCount);
        }

        public void AddRecord(int sourceId, int sourceType, byte[] data)
        {
            var record = new Record(_recordsCount++, sourceId, sourceType, data);

            if (_currentFile.Length + data.Length > FILE_SIZE)
            {
                Console.WriteLine("{0}, {1}", _currentFile.Length, FILE_SIZE);
                FlushData();
            }

            if (_sourceIds.ContainsKey(record.sourceId))
            {
                _sourceIds[record.sourceId].Add(record.recordId);
            }
            else
            {
                _sourceIds.Add(record.sourceId, new List<long> { record.recordId });
            }

            if (_sourceTypes.ContainsKey(record.sourceType))
            {
                _sourceTypes[record.sourceType].Add(record.recordId);
            }
            else
            {
                _sourceTypes.Add(record.sourceType, new List<long> { record.recordId });
            }

            _currentFile.Write(record);
        }

        public IEnumerable<IRecord> Read(string query)
        {
            var idsList = new List<long>();
            foreach (var element in query.ToLower().Split(new[] { " and " }, StringSplitOptions.RemoveEmptyEntries))
            {
                var fieldQ = element.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                switch (fieldQ[0].Trim())
                {
                    case "sourceid":
                        var key = Convert.ToInt32(fieldQ[1]);
                        if (_sourceIds.ContainsKey(key))
                        {
                            AddResultValues(ref idsList, _sourceIds[key]);
                        }
                        break;
                    case "sourcetype":
                        key = Convert.ToInt32(fieldQ[1]);
                        if (_sourceTypes.ContainsKey(key))
                        {
                            AddResultValues(ref idsList, _sourceTypes[key]);
                        }
                        break;
                }
            }

            var diction = GetRecords(idsList);
            var result = new List<Record>();
            foreach(var file in diction)
            {
                result.AddRange(GetRecordsFromFile(file.Key, file.Value));
            }
            return result;
        }

        private Dictionary<string, List<long>> GetRecords(IEnumerable<long> recordsIds)
        {
            Array.Sort(recordsIds.ToArray());
            var resultDictionary = new Dictionary<string, List<long>>();
            foreach (var id in recordsIds)
            {
                var fileName = _tables.FirstOrDefault(x => x.Value[0] >= id && x.Value[1] <= id).Key;
                if (resultDictionary.ContainsKey(fileName))
                {
                    resultDictionary[fileName].Add(id);
                }
                else
                {
                    resultDictionary.Add(fileName, new List<long> { id });
                }
            }
            return resultDictionary;
        }

        private List<Record> GetRecordsFromFile(string fileName, IEnumerable<long> recordsIds)
        {
            var idsRange = _tables[fileName];
            var file = new TableInfo(fileName, idsRange[0], idsRange[1]);
            file.GetRecords(recordsIds);
            return new List<Record>();
        }

        private void AddResultValues(ref List<long> resultArray, List<long> values)
        {
            if (resultArray.Count() == 0)
            {
                resultArray.AddRange(values);
            }
            else
            {
                resultArray.Intersect(values);
            }
        }

        public void FlushData()
        {
            _tables.Add(_currentFile._fileName, new[] { _currentFile._startPos, _currentFile._startPos });
            Tools.Serialize("dbInf.dbi", this);
            ++_tablesCount;
            //_currentFile.SaveFile();
            CreateFile();
        }
    }
}
