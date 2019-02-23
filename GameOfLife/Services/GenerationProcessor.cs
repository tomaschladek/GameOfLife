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

        public GenerationProcessor(int size)
        {
            _tempArray = new bool[size,size];
        }

        public (ConcurrentBag<CoordinateDto> ChangedPositions, int CounterDif, ConcurrentBag<CoordinateDto> ConflictSet) Execute(BitarrayWrapper nodes, int resolution, ConcurrentBag<CoordinateDto> setOfInterest)
        {
            var changedPositions = new ConcurrentBag<CoordinateDto>();
            Parallel.ForEach(setOfInterest,
                coordinates =>
                {
                    _tempArray[coordinates.Row, coordinates.Column] = IsAlive(coordinates.Index, coordinates.Row, coordinates.Column, nodes); 
                });
            var counter = 0;
            Parallel.ForEach(setOfInterest,
                coordinates =>
                {
                    if (_tempArray[coordinates.Row, coordinates.Column] != nodes[coordinates.Index])
                    {
                        nodes[coordinates.Index] =_tempArray[coordinates.Row, coordinates.Column];

                        if (_tempArray[coordinates.Row, coordinates.Column])
                        {
                            Interlocked.Increment(ref counter);
                        }
                        else
                        {
                            Interlocked.Decrement(ref counter);
                        }

                        changedPositions.Add(new CoordinateDto(coordinates.Index,coordinates.Row * resolution, coordinates.Column * resolution, _tempArray[coordinates.Row, coordinates.Column]));
                    }
            });
            var newConflictSet = CreateConflictSet(nodes,changedPositions.Union(setOfInterest));
            return (changedPositions, counter, newConflictSet);
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
                    if (rowShift == 0 && colShift == 0) continue;
                    if (column + colShift <= 0 || column + colShift >= nodes.Width) continue;

                    var newIndex = newRow + colShift;
                    
                    if (nodes[newIndex])
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

        public ConcurrentBag<CoordinateDto> CreateConflictSet(BitarrayWrapper nodes)
        {
            var coordinateDtos = new ConcurrentBag<CoordinateDto>();
            for (var index = 0; index < nodes.Length; index++)
            {
                AddToCollection(nodes, coordinateDtos, index);
            }

            return coordinateDtos;
        }

        private ConcurrentBag<CoordinateDto> CreateConflictSet(BitarrayWrapper nodes, IEnumerable<CoordinateDto> originals)
        {
            var coordinateDtos = new ConcurrentBag<CoordinateDto>();
            foreach (var original in originals)
            {
                AddToCollection(nodes,coordinateDtos,original.Index);
            }
            for (var index = 0; index < nodes.Length; index++)
            {
                AddToCollection(nodes, coordinateDtos, index);
            }

            return coordinateDtos;
        }

        private static void AddToCollection(BitarrayWrapper nodes, ConcurrentBag<CoordinateDto> coordinateDtos, int index)
        {
            if (nodes[index])
            {
                for (int rowShift = -1; rowShift < 2; rowShift++)
                    for (int columnShift = -1; columnShift < 2; columnShift++)
                    {
                        var newIndex = index + rowShift * nodes.Width + columnShift;
                        if (newIndex < 0 || newIndex > nodes.Length) continue;
                        var coordinates = nodes.GetCoordinates(newIndex);
                        coordinateDtos.Add(new CoordinateDto(coordinates.Index, coordinates.Row, coordinates.Column, nodes[newIndex]));
                    }
            }
        }
    }
}