using Assets._Pinball.Scripts.Models;
using Firebase.Analytics;
using UnityEngine;

namespace Assets._Pinball.Scripts.Services
{
    public static class FirebaseAnalyticsManager
    {
        public static void SendMuteEvent(bool isMuted)
        {
            if (isMuted)
                FirebaseAnalytics.LogEvent("muted");
            else
                FirebaseAnalytics.LogEvent("unmuted");
        }

        public static void SendNewRecordEvent(int record)
        {
            FirebaseAnalytics.LogEvent("highscore", "score", record.ToString());
        }

        public static void SendPlayedAmountEvent(int played)
        {
            FirebaseAnalytics.LogEvent("played_amount", "played", played.ToString());
        }

        public static void SendApplicationQuitInfoEvent(ApplicationQuitInfo applicationQuitInfo)
        {
            FirebaseAnalytics.LogEvent("application_quit_info", "application_quit_info_param", JsonUtility.ToJson(applicationQuitInfo));
        }

        public static void SetProperty(string propertyName, string value) 
        {
            FirebaseAnalytics.SetUserProperty(propertyName, value);
        }
    }
}
