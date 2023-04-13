using UnityEngine;
using System;
using Assets._Pinball.Scripts.Services;

namespace SgLib
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        public int Score { get; private set; }

        public int HighScore { get; private set; }

        public bool HasNewHighScore { get; private set; }

        public static event Action<int> ScoreUpdated = delegate {};
        public static event Action<int> HighscoreUpdated = delegate {};

        // key name to store high score in PlayerPrefs
        private const string HIGHSCORE = "HIGHSCORE";

        void Awake()
        {
            if (Instance)
            {
                DestroyImmediate(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        void Start()
        {
            Reset();
        }

        public void Reset()
        {
            // Initialize score
            Score = 0;

            // Initialize highscore
            HighScore = PlayerPrefs.GetInt(HIGHSCORE, 0);
            HasNewHighScore = false;
        }

        public void AddScore(int amount)
        {
            Score += amount;

            // Fire event
            ScoreUpdated(Score);

            if (Score > HighScore)
            {
                UpdateHighScore(Score);
                HasNewHighScore = true;
            }
            else
            {
                HasNewHighScore = false;
            }
        }

        public void UpdateHighScore(int newHighScore)
        {
            // Update highscore if player has made a new one
            if (newHighScore > HighScore)
            {
                HighScore = newHighScore;
                PlayerPrefs.SetInt(HIGHSCORE, HighScore);
                HighscoreUpdated(HighScore);

                FirebaseAnalyticsManager.SendNewRecordEvent(HighScore);
            }
        }

        public int GetHighScore()
        {
            return PlayerPrefs.GetInt(HIGHSCORE, 0);
        }
    }
}
