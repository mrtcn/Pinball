using Assets._Pinball.Scripts.Models;
using SgLib;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Pinball.Scripts.Services
{
    public class PlayerStatsService: MonoBehaviour
    {
        public ScoreManager ScoreManager;

        private void Start()
        {
            ScoreManager.Instance.OnHighscoreUpdated += HighScoreUpdate;
        }

        private void OnDestroy()
        {
            ScoreManager.Instance.OnHighscoreUpdated -= HighScoreUpdate;
        }

        private async void HighScoreUpdate(int highScore)
        {
            var playerStats = await StoreService.Instance.GetAsync<PlayerStats>(CloudSaveType.UserStats.ToString());
            if (playerStats == null) playerStats = new PlayerStats();
            playerStats.HighScore = highScore;
            await StoreService.Instance.SaveAsync(new Dictionary<string, object> { { CloudSaveType.UserStats.ToString(), playerStats } });
        }
    }
}
