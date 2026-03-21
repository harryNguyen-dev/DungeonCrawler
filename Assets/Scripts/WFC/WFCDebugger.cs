using UnityEngine;
using System.Collections.Generic;

namespace WFC 
{
    public class WFCDebugger : MonoBehaviour
    {
        [SerializeField] private WFCDungeonGenerator generator;

        private void Start()
        {
            generator.OnGenerated += PrintGrid;
        }

         private void PrintGrid()
        {
            int w = generator.Grid.Width;
            int h = generator.Grid.Height;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"=== WFC Grid {w}x{h} ===");

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    RoomTileData tile = generator.Grid.GetCollapsedTile(x, y);
                    string symbol = TileToSymbol(tile);
                    sb.Append(symbol.PadRight(4));
                }
                sb.AppendLine();
            }

            Debug.Log(sb.ToString());
        }

        private string TileToSymbol(RoomTileData tile)
        {
            if (tile == null)        return "?";
            if (tile.Weight <= 0f)   return GetSpecialSymbol(tile.tileName);

            return tile.tileName switch
            {
                var n when n.Contains("Wall")       => ".",
                var n when n.Contains("Start")      => "ST",
                var n when n.Contains("Hallway_NS") => "||",
                var n when n.Contains("Hallway_EW") => "==",
                var n when n.Contains("Normal")     => "[]",
                var n when n.Contains("Boss")       => "BS",
                var n when n.Contains("SuperChest") => "SC",
                var n when n.Contains("Exit")       => "EX",
                _                                   => "??"
            };
        }

        private string GetSpecialSymbol(string name)
        {
            if (name.Contains("Boss"))       return "BS";
            if (name.Contains("SuperChest")) return "SC";
            if (name.Contains("Exit"))       return "EX";
            if (name.Contains("Start"))      return "ST";
            return "??";
        }
    }
}