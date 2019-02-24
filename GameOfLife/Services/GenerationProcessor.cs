using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameOfLife.Dtos;

namespace GameOfLife.Services
{
    public class GenerationProcessor
    {
        private readonly bool[,] _tempArray;

        public GenerationProcessor(int width, int height)
        {
            _tempArray = new bool[width,height];
        }

        public (ConcurrentBag<NodeDto> ChangedPositions, int CounterDif, ConcurrentBag<NodeDto> ConflictSet) Execute(BitarrayWrapper nodes, int resolution, ConcurrentBag<NodeDto> setOfInterest)
        {
            EvaluateSet(nodes, setOfInterest);
            var result = WriteChangesToState(nodes, resolution, setOfInterest);
            var newConflictSet = CreateConflictSet(nodes, result.ChangedPositions.Union(setOfInterest));
            return (result.ChangedPositions, result.Counter, newConflictSet);
        }

        private (int Counter, ConcurrentBag<NodeDto> ChangedPositions) WriteChangesToState(BitarrayWrapper nodes, int resolution, ConcurrentBag<NodeDto> setOfInterest)
        {
            var counter = 0;
            var changedPositions = new ConcurrentBag<NodeDto>();
            Parallel.ForEach(setOfInterest,
            coordinates =>
            {
                if (_tempArray[coordinates.Row, coordinates.Column] != nodes[coordinates.Index])
                {
                    nodes[coordinates.Index] = _tempArray[coordinates.Row, coordinates.Column];

                    if (_tempArray[coordinates.Row, coordinates.Column])
                    {
                        Interlocked.Increment(ref counter);
                    }
                    else
                    {
                        Interlocked.Decrement(ref counter);
                    }

                    changedPositions.Add(new NodeDto(coordinates.Index, coordinates.Row * resolution, coordinates.Column * resolution, _tempArray[coordinates.Row, coordinates.Column]));
                }
            });
            return (counter, changedPositions);
        }

        private void EvaluateSet(BitarrayWrapper nodes, ConcurrentBag<NodeDto> setOfInterest)
        {
            Parallel.ForEach(setOfInterest,
                coordinates =>
                {
                    _tempArray[coordinates.Row, coordinates.Column] = IsAlive(coordinates.Index, coordinates.Row, coordinates.Column, nodes);
                });
        }

        private bool IsAlive(int index, int row, int column, BitarrayWrapper nodes)
        {
            var counter = 0;
            for (int rowShift = -1; rowShift < 2 && counter < 4; rowShift++)
            {
                var newRow = index + rowShift * nodes.Width;
                if (newRow < 0 || row + rowShift >= nodes.Height) continue;

                for (int colShift = -1; colShift < 2 && counter < 4; colShift++)
                {
                    if (rowShift == 0 && colShift == 0 
                        || column + colShift <= 0 || column + colShift >= nodes.Width) continue;                   
                    if (nodes[newRow + colShift])
                    {
                        counter++;
                    }
                }
            }

            if (nodes[index])
            {
                return counter == 2 || counter == 3;
            }

            return counter == 3;

        }

        public ConcurrentBag<NodeDto> CreateConflictSet(BitarrayWrapper nodes)
        {
            var coordinateDtos = new ConcurrentBag<NodeDto>();
            for (var index = 0; index < nodes.Length; index++)
            {
                AddToCollection(nodes, coordinateDtos, index);
            }

            return coordinateDtos;
        }

        private ConcurrentBag<NodeDto> CreateConflictSet(BitarrayWrapper nodes, IEnumerable<NodeDto> originals)
        {
            var coordinateDtos = new ConcurrentBag<NodeDto>();
            foreach (var original in originals)
            {
                AddToCollection(nodes,coordinateDtos,original.Index);
            }

            return coordinateDtos;
        }

        private void AddToCollection(BitarrayWrapper nodes, ConcurrentBag<NodeDto> coordinateDtos, int index)
        {
            if (nodes[index])
            {
                AddSurroundingToCollection(nodes, coordinateDtos, index);
            }
        }

        public void AddSurroundingToCollection(BitarrayWrapper nodes, ConcurrentBag<NodeDto> coordinateDtos, int index)
        {
            for (int rowShift = -1; rowShift < 2; rowShift++)
                for (int columnShift = -1; columnShift < 2; columnShift++)
                {
                    var newIndex = index + rowShift * nodes.Width + columnShift;
                    if (newIndex < 0 || newIndex > nodes.Length) continue;

                    var coordinates = nodes.GetCoordinates(newIndex);
                    coordinateDtos.Add(new NodeDto(coordinates.Index, coordinates.Row, coordinates.Column, nodes[newIndex]));
                }
        }
    }
}