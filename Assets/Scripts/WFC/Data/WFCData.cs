using System.Collections.Generic;
using UnityEngine;

namespace WFC
{

    [CreateAssetMenu(fileName = "WFCData", menuName = "WFC/WFCData")]
    public class WFCData : ScriptableObject
    {
        public GameObject prefab;

        public TileType tileType;
        // public
        public ConnectorType north;
        public ConnectorType east;
        public ConnectorType south;
        public ConnectorType west;

        [Range(0, 1)]
        public float weight;

        public ConnectorType GetConnector(Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return north;
                case Direction.East:
                    return east;
                case Direction.South:
                    return south;
                case Direction.West:
                    return west;
            }
            return ConnectorType.None;
        }

        public List<Direction> GetOpenConnector()
        {
            List<Direction> openConnectors = new List<Direction>();
            if(north == ConnectorType.Open) openConnectors.Add(Direction.North);
            if(east == ConnectorType.Open) openConnectors.Add(Direction.East);
            if(south == ConnectorType.Open) openConnectors.Add(Direction.South);
            if(west == ConnectorType.Open) openConnectors.Add(Direction.West);
            return openConnectors;
        }
    }

    public enum ConnectorType
    {
        None,
        Open,
    }

    public enum TileType
    {
        Empty,
        Room,
        Corridor,
    }
 
    public enum Direction
    {
        North,
        East,
        South,
        West,
    }

}