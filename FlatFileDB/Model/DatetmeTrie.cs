using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlatFileDB.Model
{
    public class DatetmeTrie
    {
        private TrieNode _rootNode;

        public DatetmeTrie()
        {
            _rootNode = new TrieNode(0);
        }

        public IEnumerable<long> GetRecordsByDate(DateTime dateStart, DateTime? dateEnd = null)
        {
            var dateStartArray = new[] { dateStart.Year, dateStart.Month, dateStart.Day, dateStart.Hour, dateStart.Minute };
            if (dateEnd != null)
            {
                var dateEndtArray = new[] { dateEnd.Value.Year, dateEnd.Value.Month, dateEnd.Value.Day, dateEnd.Value.Hour, dateEnd.Value.Minute };
                var date = dateEndtArray.Intersect(dateStartArray);
                var range = dateEndtArray.Except(dateStartArray).Intersect(dateStartArray.Except(dateEndtArray));
                Console.WriteLine("{0} {1}", date, range);
                return _rootNode.GetRecordsPositions(date, range);
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
                edges = new Dictionary<int, TrieNode>();
            }

            internal void AddRecord(IEnumerable<int> dateParts, long[] recordsPos)
            {
                if (dateParts.Count() == 1)
                {
                    _recordsPositions = _recordsPositions.Concat(recordsPos);
                }
                else
                {
                    var nodeKey = dateParts.First();
                    TrieNode nextNode;
                    if (edges.TryGetValue(nodeKey, out nextNode))
                    {
                        nextNode.AddRecord(dateParts.Skip(1), recordsPos);
                    }
                    else
                    {
                        nextNode = new TrieNode(nodeKey);
                        nextNode.AddRecord(dateParts.Skip(1), recordsPos);
                        edges.Add(nodeKey, nextNode);
                    }
                }
            }

            internal TrieNode GetTrieNode(IEnumerable<int> date)
            {
                var nodeKey = date.First();
                if (date.Count() == 1)
                {
                    return edges.ContainsKey(nodeKey) ? edges[nodeKey] : null;
                }
                return edges.ContainsKey(nodeKey) ? edges[nodeKey].GetTrieNode(date.Skip(1)) : null;

            }

            internal IEnumerable<long> GetRecordsPositions(IEnumerable<int> dateParts, IEnumerable<int> dateRange = null)
            {
                
                var node = GetTrieNode(dateParts);
                if (dateRange != null)
                {
                    var resultList = new List<long>();
                    foreach (var key in dateRange.Where(key => node.edges.ContainsKey(key)))
                    {
                        resultList.AddRange(node.edges[key].GetRecordsPositions());
                    }
                    return resultList;
                }
                else
                {
                    return node.GetRecordsPositions();
                }

               
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
        }
    }
}
