﻿namespace GameOfLife.Dtos
{
    public class NodeDto
    {
        public int Index { get; }
        public int Column { get; }
        public int Row { get; }
        public bool IsAlive { get; set; }

        public NodeDto(int index, int row, int column, bool isAlive)
        {
            Index = index;
            Column = column;
            Row = row;
            IsAlive = isAlive;
        }

        public override bool Equals(object obj)
        {
            var dto = obj as NodeDto;
            return dto != null &&
                   Column == dto.Column &&
                   Row == dto.Row;
        }

        public override int GetHashCode()
        {
            var hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + Column.GetHashCode();
            hashCode = hashCode * -1521134295 + Row.GetHashCode();
            return hashCode;
        }
    }
}