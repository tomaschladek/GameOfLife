namespace GameOfLife.Dtos
{
    public class CoordinateDto
    {
        public int Column { get; }
        public int Row { get; }
        public bool IsAlive { get; set; }

        public CoordinateDto(int row, int column, bool isAlive)
        {
            Column = column;
            Row = row;
            IsAlive = isAlive;
        }

        public override bool Equals(object obj)
        {
            var dto = obj as CoordinateDto;
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