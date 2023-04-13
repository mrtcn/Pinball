using Assets._Pinball.Scripts.Services;
using Firebase.Crashlytics;
using System;

namespace Assets._Pinball.Scripts.Models
{
    public enum BackgroundType
    {
        Quit,
        Pause
    }
    [Serializable]
    public class ApplicationQuitInfo
    {
        public ApplicationQuitInfo(BackgroundType backgroundType, int highScore, int played, FixedSizeConcurrentQueue<LastSignificantGameState> lastSignificantEvents)
        {
            BackgroundType = backgroundType.ToString();
            HighScore = highScore;
            Played = played;
            try
            {
                string lastSignificantEventsText = "";
                while (lastSignificantEvents.Count > 0)
                {
                    if (lastSignificantEvents.TryDequeue(out LastSignificantGameState lastSignificantEvent))
                        lastSignificantEventsText += $"{lastSignificantEvent}, ";
                }
                if (lastSignificantEventsText.Length > 3)
                    LastSignificantEvents = lastSignificantEventsText.Substring(0, lastSignificantEventsText.Length - 2);
            }
            catch (Exception ex)
            {
                Crashlytics.LogException(ex);
                LastSignificantEvents = string.Empty;
            }
        }

        public string BackgroundType { get; set; }
        public int HighScore { get; set; }
        public int Played { get; set; }
        public string LastSignificantEvents { get; set; }
    }
}
