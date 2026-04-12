using System;
using UnityEngine;

namespace Global
{
    public static class GlobalEvents
    {
        public static event Action<int> OnDungeonGeneratedSuccess;
        public static event Action<bool> OnStartGame;

        public static void TriggerDungeonGeneratedSuccess(int seed)
        {
            OnDungeonGeneratedSuccess?.Invoke(seed);
        }

        public static void TriggerStartGame(bool isStart)
        {
            OnStartGame?.Invoke(isStart);
        }
    }
}
