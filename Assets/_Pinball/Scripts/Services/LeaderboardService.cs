using Assets._Pinball.Scripts.Models;
using Firebase.Crashlytics;
using SgLib;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Exceptions;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

namespace Assets._Pinball.Scripts.Services
{
    public class LeaderboardService: MonoBehaviour
    {
        public static LeaderboardService Instance { get; private set; }
        [SerializeField]
        private GameObject scrollView;
        [SerializeField]
        private List<TextMeshProUGUI> names;
        [SerializeField]
        private List<TextMeshProUGUI> scores;
        private string HIGHSCORE = "HIGHSCORE";



        private void Start()
        {
            GooglePlayGamesScript.Instance.OnGoogleUserLogIn += GoogleUserLoggedIn;
            FacebookScript.Instance.OnFacebookUserLogIn += FacebookUserLoggedIn;
            ScoreManager.Instance.OnHighscoreUpdated += HighScoreUpdated;
        }

        private async void HighScoreUpdated(int highScore)
        {
            if (AuthService.Instance.IsAuthenticated)
                await LeaderboardsService.Instance.AddPlayerScoreAsync(AppInfo.Instance.LeaderboardId.ToLower(), highScore);
        }

        private void OnDestroy()
        {
            GooglePlayGamesScript.Instance.OnGoogleUserLogIn -= GoogleUserLoggedIn;
            FacebookScript.Instance.OnFacebookUserLogIn -= FacebookUserLoggedIn;
            ScoreManager.Instance.OnHighscoreUpdated -= HighScoreUpdated;
        }
        void Awake()
        {
            Instance = this;
        }

        public void Close()
        {
            scrollView.SetActive(false);
        }

        private async void GoogleUserLoggedIn(UserInfo userInfo)
        {
            var highScore = PlayerPrefs.GetInt(HIGHSCORE);
            await LeaderboardsService.Instance.AddPlayerScoreAsync(AppInfo.Instance.LeaderboardId.ToLower(), highScore);
        }
        private async void FacebookUserLoggedIn(UserInfo userInfo)
        {
            var highScore = PlayerPrefs.GetInt(HIGHSCORE);
            await LeaderboardsService.Instance.AddPlayerScoreAsync(AppInfo.Instance.LeaderboardId.ToLower(), highScore);
        }

        public async Task LoadLeaderboard()
        {
            try
            {
                scrollView.SetActive(true);

                var options = new GetPlayerRangeOptions();
                options.RangeLimit = AppInfo.Instance.LeaderboardStep;

                var leaderboard = await LeaderboardsService.Instance.GetPlayerRangeAsync(AppInfo.Instance.LeaderboardId.ToLower(), options);

                for (var i = 0; i < leaderboard.Results.Count; i++)
                {
                    var result = leaderboard.Results[i];
                    names[i].text = result.PlayerName;
                    scores[i].text = result.Score.ToString();
                }
            }
            catch (LeaderboardsException ex)
            {
                if (ex.ErrorCode == 27001)
                {
                    ToastMessageScript.Instance.showToast("In order to display leaderboard, You need to login.", 3);
                }
                else
                {
                    ToastMessageScript.Instance.showToast("Leaderboard cannot be loaded at the moment. Please login if you are not logged in and try again.", 3);
                    Crashlytics.LogException(ex);
                }
            }
        }

        public async Task<LeaderboardEntry> GetScore()
        {
            try
            {
                return await LeaderboardsService.Instance.GetPlayerScoreAsync(AppInfo.Instance.LeaderboardId);
            }
            catch (LeaderboardsException ex)
            {
                //User has no entry
                if(ex.ErrorCode == 27009 ||ex.ErrorCode == 51)
                {
                    return null;
                }
                throw;
            }            
        }
    }
}
