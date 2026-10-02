using System;
using UnityEngine;

namespace RunnerGame
{
    /// <summary>
    /// Central, loosely coupled event hub.
    /// Systems publish state changes here and other systems subscribe,
    /// which keeps the scripts decoupled from each other (no hard references).
    /// </summary>
    public static class GameEvents
    {
        /// <summary>Raised once the countdown finishes and gameplay starts.</summary>
        public static event Action GameStarted;

        /// <summary>Raised when the player collects a coin. Argument: coin world position.</summary>
        public static event Action<Vector3> CoinCollected;

        /// <summary>Raised when the player takes a hit (and survives).</summary>
        public static event Action PlayerHurt;

        /// <summary>Raised when the player runs out of health.</summary>
        public static event Action PlayerDied;

        /// <summary>Raised when the run ends. Argument: score of the finished run.</summary>
        public static event Action<int> GameOver;

        /// <summary>Raised when pause state changes. Argument: true when pausing.</summary>
        public static event Action<bool> PauseChanged;

        public static void RaiseGameStarted()
        {
            GameStarted?.Invoke();
        }

        public static void RaiseCoinCollected(Vector3 worldPosition)
        {
            CoinCollected?.Invoke(worldPosition);
        }

        public static void RaisePlayerHurt()
        {
            PlayerHurt?.Invoke();
        }

        public static void RaisePlayerDied()
        {
            PlayerDied?.Invoke();
        }

        public static void RaiseGameOver(int score)
        {
            GameOver?.Invoke(score);
        }

        public static void RaisePauseChanged(bool paused)
        {
            PauseChanged?.Invoke(paused);
        }
    }
}
