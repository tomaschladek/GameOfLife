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

        public (ConcurrentBag<CoordinateDto> ChangedPositions, int CounterDif) Execute(NodeDto[,] nodes, int resolution)
        {
            var bag = new ConcurrentBag<CoordinateDto>();
            Parallel.For(0, nodes.GetLength(0),
                row =>
                {
                    Parallel.For(0, nodes.GetLength(1), column => { _tempArray[row, column] = IsAlive(row, column, nodes); });
                });
            var counter = 0;
            Parallel.For(0, nodes.GetLength(0), row =>
            {
                Parallel.For(0, nodes.GetLength(1), column =>
                {
                    if (_tempArray[row, column] != (nodes[row, column]?.IsAlive ?? false))
                    {
                        if (nodes[row, column] == null)
                        {
                            nodes[row, column] = new NodeDto(_tempArray[row, column]);
                        }
                        else
                        {
                            nodes[row, column].IsAlive = _tempArray[row, column];
                        }

                        if (_tempArray[row, column])
                        {
                            Interlocked.Increment(ref counter);
                        }
                        else
                        {
                            Interlocked.Decrement(ref counter);
                        }

                        bag.Add(new CoordinateDto(row * resolution, column * resolution, _tempArray[row, column]));
                    }
                });
            });
            return (bag, counter);
        }

        private bool IsAlive(int row, int column, NodeDto[,] nodes)
        {
            var counter = 0;
            for (int rowShift = -1; rowShift < 2 && counter < 4; rowShift++)
            for (int colShift = -1; colShift < 2 && counter < 4; colShift++)
            {
                if (rowShift == 0 && colShift == 0) continue;
                if (row + rowShift >= 0
                    && row + rowShift < nodes.GetLength(0)
                    && column + colShift >= 0
                    && column + colShift < nodes.GetLength(1)
                    && nodes[row + rowShift, column + colShift] != null
                    && nodes[row + rowShift, column + colShift].IsAlive)
                {
                    counter++;
                }

            }

            if (nodes[row, column]?.IsAlive == true)
            {
                return counter == 2 || counter == 3;
            }

            return counter == 3;

        }
    }
}