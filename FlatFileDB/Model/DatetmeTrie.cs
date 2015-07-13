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

        public void AddRecord(int[] dateParts, long[] recordsPos)
        {

        }

        private class TrieNode
        {
            private int datePart;
            private IEnumerable<long> recordsPositions;
            private Dictionary<int, TrieNode> edges;

            private void AddRecord(IEnumerable<int> dateParts, long[] recordsPos)
            {
                if (dateParts.Count() == 1)
                {
                    recordsPositions.Concat(recordsPos);
                }
                else
                {
                    TrieNode nextNode;
                    if (edges.TryGetValue(dateParts.First(), out nextNode))
                    {
                        nextNode.AddRecord(dateParts.Skip(1), recordsPos);
                    }
                    else
                    {
                        nextNode.AddRecord(dateParts.Skip(1), recordsPos);
                        edges.Add(dateParts.First(), nextNode);
                    }
                        }
            }

            private IEnumerable<long> GetRecordsPositions()
            {
                var resultList = new List<long>();
                if (!edges.Any())
                {
                    return recordsPositions;
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
