using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FlatFileDB.Model
{
    [Serializable]
    public class DbService
    {
        const int FileSize = 1024 * 1024 * 20;
        private long _recordsCount;
        private int _tablesCount;
        private readonly Dictionary<int, List<long>> _sourceIds;
        private readonly Dictionary<int, List<long>> _sourceTypes;
        private readonly LinkedList<long> _tables;

        [NonSerialized]
        private TableInfo _currentFile;

        public DbService()
        {
            if (File.Exists("dbInf.dbi"))
            {
                var dbInf = (DbService)Tools.Deserialize("dbInf.dbi");
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
                _tables = new LinkedList<long>();
            }

            _currentFile = new TableInfo(GetFileName(_tablesCount++), _recordsCount); 
        }

        private void CreateFile()
        {         
            _currentFile = new TableInfo(GetFileName(_tablesCount++), _recordsCount);
        }

        public void AddRecord(int sourceId, int sourceType, byte[] data)
        {
            var record = new Record(sourceId, sourceType, data);

            if (_currentFile.Length + record.ToByteArray().Length > FileSize)
            {            
                FlushData();
                CreateFile();
            }

            var id = _currentFile.StartOffset + _currentFile.Write(record);
             ++_recordsCount;
            if (_sourceIds.ContainsKey(record.SourceId))
            {
                _sourceIds[record.SourceId].Add(id);
            }
            else
            {
                _sourceIds.Add(record.SourceId, new List<long> { id });
            }

            if (_sourceTypes.ContainsKey(record.SourceType))
            {
                _sourceTypes[record.SourceType].Add(id);
            }
            else
            {
                _sourceTypes.Add(record.SourceType, new List<long> { id });
            }           
        }

        public List<string> Read(string query)
        {
            var idsList = new List<long>();
            foreach (var fieldQ in query.ToLower().Split(new[] { " and " }, StringSplitOptions.RemoveEmptyEntries).Select(element => element.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries)))
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

            var diction = GetRecords(idsList);
            
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
            return new TableInfo(GetFileName(tableIndex), _tables.ElementAtOrDefault(tableIndex));
        }

        private Dictionary<int, List<long>> GetRecords(List<long> recordsIds)
        {
            Array.Sort(recordsIds.ToArray());          
            var resultDictionary = new Dictionary<int, List<long>>();
            for (var i = 0; i < _tables.Count; i++ )
            {
                var offset = _tables.ElementAt(i);
                var offsetNext = _tables.ElementAtOrDefault(i+1) == default(long) ? _currentFile.StartOffset :_tables.ElementAtOrDefault(i+1) ;
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

        private static string GetFileName(int fileN)
        {
            return string.Format("table{0}.ftd", fileN);
        }

        public void FlushData()
        {
            _tables.AddLast(_currentFile.StartOffset);            
            Tools.Serialize("dbInf.dbi", this);            
            _currentFile.SaveFile();           
        }
    }
}
