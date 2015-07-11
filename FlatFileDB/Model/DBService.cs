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
        const int FILE_SIZE = 1024 * 1024 * 20;
        private long _recordsCount;
        private int _tablesCount;
        private Dictionary<int, List<long>> _sourceIds;
        private Dictionary<int, List<long>> _sourceTypes;
        private LinkedList<long> _tables;

        [NonSerialized]
        private TableInfo _currentFile;

        public DBService()
        {
            if (File.Exists("dbInf.dbi"))
            {
                var dbInf = (DBService)Tools.Deserialize("dbInf.dbi");
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

        public void CreateFile()
        {         
            _currentFile = new TableInfo(GetFileName(_tablesCount++), _recordsCount);
        }

        public void AddRecord(int sourceId, int sourceType, byte[] data)
        {
            var record = new Record(sourceId, sourceType, data);

            if (_currentFile.Length + record.ToByteArray().Length > FILE_SIZE)
            {            
                FlushData();
                CreateFile();
            }

            var id = _currentFile._startOffset + _currentFile.Write(record);
             ++_recordsCount;
            if (_sourceIds.ContainsKey(record.sourceId))
            {
                _sourceIds[record.sourceId].Add(id);
            }
            else
            {
                _sourceIds.Add(record.sourceId, new List<long> { id });
            }

            if (_sourceTypes.ContainsKey(record.sourceType))
            {
                _sourceTypes[record.sourceType].Add(id);
            }
            else
            {
                _sourceTypes.Add(record.sourceType, new List<long> { id });
            }           
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
                var offsetNext = _tables.ElementAtOrDefault(i+1) == default(long) ? _currentFile._startOffset :_tables.ElementAtOrDefault(i+1) ;
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

        private void AddResultValues(ref List<long> resultArray, List<long> values)
        {
            if (resultArray.Count() == 0)
            {
                resultArray.AddRange(values);
            }
            else
            {
                resultArray = resultArray.Intersect(values).ToList();
            }
        }

        public string GetFileName(int fileN)
        {
            return String.Format("table{0}.ftd", fileN);
        }

        public void FlushData()
        {
            _tables.AddLast(_currentFile._startOffset);            
            Tools.Serialize("dbInf.dbi", this);            
            _currentFile.SaveFile();           
        }
    }
}
