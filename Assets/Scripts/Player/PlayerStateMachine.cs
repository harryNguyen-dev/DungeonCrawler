using System;
using UnityEngine;

namespace Player
{
    public enum PlayerState {
        Idle, Moving, Sprinting,
        CombatIdle, Attacking, Blocking, Parrying, Dodging,
        HitReaction, Stunned, Dead, Interacting,
    }

    public class PlayerStateMachine : MonoBehaviour
    {
        public PlayerState Current {get; private set;}
        public event Action<PlayerState, PlayerState> OnStateChanged;

        public bool TryTransition(PlayerState next) 
        {
            if(!IsValidTransition(Current, next)) return false;

            var prev = Current;
            Current = next;
            OnStateChanged?.Invoke(prev, next);
            return true;
        }

        bool IsValidTransition(PlayerState from, PlayerState to) {
            return (from, to) switch {
                (_, PlayerState.Dead)        => true,   // Chết từ bất cứ đâu
                (_, PlayerState.HitReaction) => from != PlayerState.Dead,
                (PlayerState.Dead, _)        => false,  // Dead là terminal
                (PlayerState.Attacking, PlayerState.Attacking) => true, // combo
                (PlayerState.Stunned, _)     => to == PlayerState.Idle,
                _                            => true
            };
        }
    }
}