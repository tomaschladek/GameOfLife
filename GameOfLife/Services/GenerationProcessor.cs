using System.Collections;
using System.Collections.Concurrent;
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

        public (ConcurrentBag<CoordinateDto> ChangedPositions, int CounterDif) Execute(BitarrayWrapper nodes, int resolution, ConcurrentBag<CoordinateDto> setOfInterest)
        {
            var changedPositions = new ConcurrentBag<CoordinateDto>();
            Parallel.For(0, nodes.Length,
                index =>
                {
                    var coordinates = nodes.GetCoordinates(index);
                    _tempArray[coordinates.Row, coordinates.Column] = IsAlive(index, coordinates.Row, coordinates.Column, nodes); 
                });
            var counter = 0;
            Parallel.For(0, nodes.Length,
                index => 
                {
                    var coordinates = nodes.GetCoordinates(index);
                    if (_tempArray[coordinates.Row, coordinates.Column] != nodes[index])
                    {
                        nodes[index] =_tempArray[coordinates.Row, coordinates.Column];

                        if (_tempArray[coordinates.Row, coordinates.Column])
                        {
                            Interlocked.Increment(ref counter);
                        }
                        else
                        {
                            Interlocked.Decrement(ref counter);
                        }

                        changedPositions.Add(new CoordinateDto(coordinates.Row * resolution, coordinates.Column * resolution, _tempArray[coordinates.Row, coordinates.Column]));
                    }
            });
            return (changedPositions, counter);
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
            //for (var index = 0; index < nodes.Length; index++)
            //{
            //    bool node = nodes[index];
            //    var row = nodes.GetRow(index);
            //    var column = nodes.GetColumn(index);
            //    coordinateDtos.Add(new CoordinateDto(row,column,node));
            //}

            return coordinateDtos;
        }
    }
}