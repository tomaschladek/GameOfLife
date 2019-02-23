namespace GameOfLife.Dtos
{
    public class CoordinateDto
    {
        public int X { get; }
        public int Y { get; }
        public bool IsAlive { get; set; }

        public CoordinateDto(int x, int y, bool isAlive)
        {
            X = x;
            Y = y;
            IsAlive = isAlive;
        }

        public override bool Equals(object obj)
        {
            var dto = obj as CoordinateDto;
            return dto != null &&
                   X == dto.X &&
                   Y == dto.Y;
        }

        public override int GetHashCode()
        {
            var hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }
    }
}