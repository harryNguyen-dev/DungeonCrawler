using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace WFC
{
    public static class Prim
    {
        public static List<(Tile from, Tile to)> BuildMST(List<Tile> rooms)
        {
            if (rooms == null || rooms.Count < 2)
                return new List<(Tile, Tile)>();
            List<(Tile from, Tile to)> mstEdges = new List<(Tile, Tile)>();

            // Tập các room đã nằm trong MST
            HashSet<Tile> inMST = new HashSet<Tile>();
            // Tập các room chưa vào MST
            HashSet<Tile> notInMST = new HashSet<Tile>(rooms);


            inMST.Add(rooms[0]);
            notInMST.Remove(rooms[0]);

            while(notInMST.Count > 0) 
            {
                Tile bestFrom = null;
                Tile bestTo = null;
                int bestDist = int.MaxValue;
            
                foreach(Tile from in inMST) 
                {
                    foreach(Tile to in notInMST)
                    {
                        int dist = MahattanDistance(from.GridPosition, to.GridPosition);
                        if(dist < bestDist)
                        {
                            bestDist = dist;
                            bestFrom = from;
                            bestTo = to;
                        }
                    }
                }
                // Thêm edge tốt nhất vào MST
                mstEdges.Add((bestFrom, bestTo));
                inMST.Add(bestTo);
                notInMST.Remove(bestTo);
            }

        
            return mstEdges;
        }

        private static int MahattanDistance(Vector2Int from, Vector2Int to)
        {
            return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
        }
        public static void AddExtraEdges(List<(Tile, Tile)> edges, List<Tile> rooms, System.Random rand, float extraRatio = 0.15f)
        {
            int extraCount = Mathf.RoundToInt(rooms.Count * extraRatio);

            for (int i = 0; i < extraCount; i++)
            {
                Tile a = rooms[rand.Next(0, rooms.Count)];
                Tile b = rooms[rand.Next(0, rooms.Count)];

                if (a != b && !edges.Contains((a, b)) && !edges.Contains((b, a)))
                    edges.Add((a, b));
            }
        }
    }
}