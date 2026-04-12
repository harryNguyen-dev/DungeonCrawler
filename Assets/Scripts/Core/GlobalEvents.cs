using System;
using UnityEngine;

namespace Core
{

    public static class GlobalEvents
    {
        public static event Action<int> OnDungeonGeneratedSuccess;

        public static void TriggerDungeonGeneratedSuccess(int seed)
        {
            OnDungeonGeneratedSuccess?.Invoke(seed);
        }
    }
}
