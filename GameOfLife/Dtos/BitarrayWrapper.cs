using System.Collections;

namespace GameOfLife.Dtos
{
    public class BitarrayWrapper
    {
        private BitArray Source { get; }
        public int Width { get; }
        public int Height { get; }
        public int Length => Source.Length;

        public BitarrayWrapper(int width, int height)
        {
            Source = new BitArray(width*height);
            Width = width;
            Height = height;
        }

        public NodeDto GetCoordinates(int index)
        {
            var row = index / Width;
            var column = index - row * Width;
            return new NodeDto(index, row, column, false);
        }

        private int GetCellIndex(int row, int column)
        {
            return row * Width + column;
        }

        public bool this[int index]
        {
            get => Source[index];
            set => Source[index] = value;
        }

        public bool this[int row, int column]
        {
            get => Source[GetCellIndex(row, column)];
            set => Source[GetCellIndex(row, column)] = value;
        }
    }
}