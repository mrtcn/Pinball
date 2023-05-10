using Assets._Pinball.Scripts.Services;
using System;
using UnityEngine;

[CreateAssetMenu()]
public class ScoreSO : ScriptableObject
{
    public int Score { get; private set; }

    public int HighScore { get; private set; }

    public bool HasNewHighScore { get; private set; }

    public event Action<int> OnScoreUpdate = delegate { };
    public event Action<int> OnHighscoreUpdated = delegate { };

    // key name to store high score in PlayerPrefs
    private const string HIGHSCORE = "HIGHSCORE";

    public void Init()
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
        OnScoreUpdate(amount);

        if (Score > HighScore)
        {
            UpdateHighScore(Score);
            SetHighScoreProperties();

            HasNewHighScore = true;
        }
        else
        {
            HasNewHighScore = false;
        }
    }

    private void SetHighScoreProperties()
    {
        if (Score > 9 && Score < 20)
            FirebaseAnalyticsManager.SetProperty("highscore_10+", "10");
        else if (Score > 19 && Score < 30)
            FirebaseAnalyticsManager.SetProperty("highscore_20+", "20");
    }

    public void UpdateHighScore(int newHighScore)
    {
        // Update highscore if player has made a new one

        HighScore = newHighScore;
        PlayerPrefs.SetInt(HIGHSCORE, HighScore);
        OnHighscoreUpdated(HighScore);

        FirebaseAnalyticsManager.SendNewRecordEvent(HighScore);
    }

    public int GetHighScore()
    {
        return PlayerPrefs.GetInt(HIGHSCORE, 0);
    }
}
