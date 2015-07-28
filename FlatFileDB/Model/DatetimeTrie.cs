using System;
using System.Collections.Generic;
using System.Linq;

namespace FlatFileDB.Model
{
    public class DatetimeTrie
    {
        private TrieNode _rootNode;

        public DatetimeTrie()
        {
            _rootNode = new TrieNode(0);
        }

        public IEnumerable<long> GetRecordsByDate(DateTime dateStart, DateTime? dateEnd = null)
        {
            var dateStartArray = new[] { dateStart.Year, dateStart.Month, dateStart.Day, dateStart.Hour, dateStart.Minute };
            if (dateEnd != null)
            {
                var dateRange = new DateRange(dateStart, dateEnd.Value);
                return _rootNode.GetRecordsPositions(dateRange);
            }
            return _rootNode.GetRecordsPositions(dateStartArray);
        }

        public void AddRecord(DateTime date, long[] recordsPos)
        {
            _rootNode.AddRecord(new[] { date.Year, date.Month, date.Day, date.Hour, date.Minute }, recordsPos);
        }

        private class TrieNode
        {
            private int _datePart;
            private IEnumerable<long> _recordsPositions;
            private Dictionary<int, TrieNode> edges;

            internal TrieNode(int newDatePart)
            {
                _datePart = newDatePart;
                _recordsPositions = new List<long>();
                edges = new Dictionary<int, TrieNode>();
            }

            internal void AddRecord(IEnumerable<int> dateParts, long[] recordsPos)
            {
                var enumerable = dateParts as int[] ?? dateParts.ToArray();
                if (enumerable.Count() == 1)
                {
                    _recordsPositions = _recordsPositions.Concat(recordsPos);
                }
                else
                {
                    var nodeKey = enumerable.First();
                    TrieNode nextNode;
                    if (edges.TryGetValue(nodeKey, out nextNode))
                    {
                        nextNode.AddRecord(enumerable.Skip(1), recordsPos);
                    }
                    else
                    {
                        nextNode = new TrieNode(nodeKey);
                        nextNode.AddRecord(enumerable.Skip(1), recordsPos);
                        edges.Add(nodeKey, nextNode);
                    }
                }
            }

            private TrieNode GetTrieNode(IEnumerable<int> date)
            {
                var nodeKey = date.First();
                if (date.Count() == 1)
                {
                    return edges.ContainsKey(nodeKey) ? edges[nodeKey] : null;
                }
                return edges.ContainsKey(nodeKey) ? edges[nodeKey].GetTrieNode(date.Skip(1)) : null;

            }

            internal IEnumerable<long> GetRecordsPositions(IEnumerable<int> dateParts)
            {
                var node = GetTrieNode(dateParts);
                return node.GetRecordsPositions();
            }

            internal IEnumerable<long> GetRecordsPositions()
            {
                var resultList = new List<long>();
                if (!edges.Any())
                {
                    return _recordsPositions;
                }
                foreach (var edge in edges)
                {
                    resultList.AddRange(edge.Value.GetRecordsPositions());
                }
                return resultList;
            }

            internal IEnumerable<long> GetRecordsPositions(DateRange dateRange)
            {
                var lastCommonNode = GetTrieNode(dateRange.CommonParts);               
                var range = Enumerable.Range(dateRange.Range[0], dateRange.Range[1]);

                var result = new List<long>();
                foreach (var nodeKey in range)
                {
                    if (lastCommonNode.edges.ContainsKey(nodeKey))
                    {
                        result.AddRange(lastCommonNode.edges[nodeKey].GetRecordsPositions());
                    }
                }

                return result;
            }
        }

        private class DateRange
        {
            public int[] CommonParts;
            public int[] Range;

            public DateRange(DateTime date1, DateTime date2)
            {
                DateTime dateStart, dateEnd;
                if (date1 > date2)
                {
                    dateStart = date2;
                    dateEnd = date1;
                }
                else
                {
                    dateStart = date1;
                    dateEnd = date2;
                }

                var dateStartAsArray = dateStart.AsArrayOfIntegers().ToArray();
                var dateEndAsArray = dateEnd.AsArrayOfIntegers().ToArray();

                var commonPartsList = new List<int>();
                var range = new int[0];
                for (var i = 0; i < 5; i++)
                {
                    if (dateStartAsArray[i] == dateEndAsArray[i])
                    {
                        commonPartsList.Add(dateStartAsArray[i]);
                    }
                    else
                    {
                        range = new[] { dateStartAsArray[i], dateEndAsArray[i] };
                        break;
                    }
                }

                CommonParts = commonPartsList.ToArray();
                Range = range;
            }
        }
    }
}
