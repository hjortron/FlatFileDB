using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FlatFileDB.Model;
using AppSettings = FlatFileDB.Properties.Settings;

namespace FlatFileDB
{
    [Serializable]
    public class DataBaseManager
    {
        private long _recordsCount;
        private int _tablesCount;
        private readonly Dictionary<int, List<long>> _sourceIds;
        private readonly Dictionary<int, List<long>> _sourceTypes;
        private readonly DatetimeTrie _datetimeIndexer;
        private readonly LinkedList<long> _tables;

        [NonSerialized]
        private TableInfo _currentFile;

        public DataBaseManager()
        {
            if (File.Exists(AppSettings.Default.DbDataFile))
            {
                using (var fileStream = new FileStream(AppSettings.Default.DbDataFile, FileMode.Open))
                {
                    var dbInf = (DataBaseManager)Tools.Deserialize(fileStream);
                    _recordsCount = dbInf._recordsCount;
                    _sourceIds = dbInf._sourceIds;
                    _sourceTypes = dbInf._sourceTypes;
                    _datetimeIndexer = dbInf._datetimeIndexer;
                    _tables = dbInf._tables;
                    _tablesCount = dbInf._tablesCount;
                }
            }
            else
            {
                _sourceIds = new Dictionary<int, List<long>>();
                _sourceTypes = new Dictionary<int, List<long>>();
                _datetimeIndexer = new DatetimeTrie();
                _tables = new LinkedList<long>();
            }

            AddNewDataFile();
        }

        private void AddNewDataFile()
        {
            _currentFile = new TableInfo(Tools.GetFileName(_tablesCount++), _recordsCount);
        }

        public void AddRecord(int sourceId, int sourceType, byte[] data)
        {
            var record = new Record(sourceId, sourceType, data);

            if (_currentFile.Length + record.ToByteArray().Length > AppSettings.Default.DataTableSize)
            {
                SaveData();
                AddNewDataFile();
            }

            var id = _currentFile.StartOffset + _currentFile.Write(record);
            ++_recordsCount;

            _sourceIds.InsertOrUpdate(record.SourceId, id);
            _sourceTypes.InsertOrUpdate(record.SourceType, id);
            _datetimeIndexer.AddRecord(record.creationDate, new[] { id });
        }

        public List<string> GetRecords(string query)
        {
            var idsList = new List<long>();
            foreach (var fieldQ in Tools.ParseQuery(query))
            {
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

            var diction = GetRecordsPositions(idsList);

            var result = new List<string>();
            foreach (var file in diction)
            {
                result.AddRange(GetTable(file.Key).GetRecords(file.Value));
            }
            return result;
        }

        private TableInfo GetTable(int tableIndex)
        {
            if (tableIndex == -1)
            {
                return _currentFile;
            }
            return new TableInfo(Tools.GetInfoTableName(tableIndex), _tables.ElementAtOrDefault(tableIndex));
        }

        private Dictionary<int, List<long>> GetRecordsPositions(List<long> recordsIds)
        {
            Array.Sort(recordsIds.ToArray());
            var resultDictionary = new Dictionary<int, List<long>>();
            for (var i = 0; i < _tables.Count; i++)
            {
                var offset = _tables.ElementAt(i);
                var offsetNext = _tables.ElementAtOrDefault(i + 1) == default(long) ? _currentFile.StartOffset : _tables.ElementAtOrDefault(i + 1);
                var ids = recordsIds.Where(id => id >= offset && id < offsetNext);
                if (resultDictionary.ContainsKey(i))
                {
                    resultDictionary[i].AddRange(ids);
                }
                else
                {
                    resultDictionary.Add(i, new List<long>(ids));
                }
                recordsIds.RemoveAll(e => ids.Contains(e));
            }
            if (recordsIds.Count > 0)
            {
                resultDictionary.Add(-1, recordsIds);
            }

            return resultDictionary;
        }

        private static void AddResultValues(ref List<long> resultArray, IEnumerable<long> values)
        {
            if (!resultArray.Any())
            {
                resultArray.AddRange(values);
            }
            else
            {
                resultArray = resultArray.Intersect(values).ToList();
            }
        }

        public void SaveData()
        {
            _tables.AddLast(_currentFile.StartOffset);
            Tools.Serialize(AppSettings.Default.DbDataFile, this);
            _currentFile.SaveFile();
        }
    }
}
