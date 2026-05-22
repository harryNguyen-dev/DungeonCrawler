using System;
using UnityEngine;

namespace Core
{
    public class SpawnPlayerManager : MonoBehaviour
    {
        public void OnEnable()
        {
            Global.GlobalEvents.OnDungeonGenerated += SpawnPlayer;
        }

        public void SpawnPlayer(int seed)
        {
            Vector3 offset = Vector3.up * 2.5f;
            Global.GlobalEntities.Instance.SpawnPlayer();
            var spawnPoint = Global.GlobalVariable.PlayerSpawnPosition;
            Global.GlobalEntities.Instance.PlayerInstance.transform.position = spawnPoint + offset;
        }
    }
}