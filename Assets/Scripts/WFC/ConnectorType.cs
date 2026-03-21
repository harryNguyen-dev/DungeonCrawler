namespace WFC 
{
    public enum ConnectorType
    {
        None,
        Open,
    }
    public enum ConnectorDirection
    {
        North,
        East,
        South,
        West,
    }

    public static class ConnectorUtils 
    {
        public static ConnectorDirection Opposite(ConnectorDirection direction)
        {
            return direction switch 
            {
                ConnectorDirection.North => ConnectorDirection.South,
                ConnectorDirection.East => ConnectorDirection.West,
                ConnectorDirection.South => ConnectorDirection.North,
                ConnectorDirection.West => ConnectorDirection.East,
                _ => ConnectorDirection.North,
            };
        }

        public static (int dx, int dy) ToOffset(ConnectorDirection direction)
        {
            return direction switch 
            {
                ConnectorDirection.North => (0, 1),
                ConnectorDirection.East => (1, 0),
                ConnectorDirection.South => (0, -1),
                ConnectorDirection.West => (-1, 0),
                _ => (0, 0),
            };
        }
    }
}