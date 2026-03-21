using UnityEngine;

namespace WFC 
{
    [CreateAssetMenu(
        fileName = "RoomTile_",
        menuName = "Dungeon/WFC/Room Tile Data"
    )]
    public class RoomTileData : ScriptableObject
    {
        [Header("Basic Information")]
        public string tileName;
        public GameObject prefab;

        [Header("Connector - N, E, S, W")]
        public ConnectorType north;
        public ConnectorType east;
        public ConnectorType south;
        public ConnectorType west;

        [Header("Weight")]
        [Range(0f, 1f)]
        public float Weight = 1f;
         
        public ConnectorType GetConnector(ConnectorDirection direction)
        {
            return direction switch 
            {
                ConnectorDirection.North => north,
                ConnectorDirection.East => east,
                ConnectorDirection.South => south,
                ConnectorDirection.West => west,
                _ => ConnectorType.None,
            };
        }

        public bool CanConnect(ConnectorDirection dir, RoomTileData other)
        {
            ConnectorType mine = GetConnector(dir);
            ConnectorType theirs = other.GetConnector(ConnectorUtils.Opposite(dir));
            return mine == theirs;
        }
    }
}