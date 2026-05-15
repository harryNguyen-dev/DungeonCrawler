using System;
using UnityEngine;

namespace Core
{
    public class SpawnPlayerManager : MonoBehaviour
    {
        public GameObject playerPrefab;
        
        public void OnEnable()
        {
            // Global.GlobalEvents.OnDungeonGeneratedSuccess += SpawnPlayer;
        }

        public void SpawnPlayer(int seed)
        {
            
        }
    }
}