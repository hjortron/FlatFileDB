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

        public void AddRecord(IEnumerable<int> dateParts, long[] recordsPos)
        {
            _rootNode.AddRecord(dateParts, recordsPos);
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

            private IEnumerable<long> GetRecordsPositions()
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
