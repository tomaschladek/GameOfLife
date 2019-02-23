namespace GameOfLife.Dtos
{
    public class NodeDto
    {
        public bool IsAlive { get; set; }

        public NodeDto(bool isAlive)
        {
            IsAlive = isAlive;
        }
    }
}