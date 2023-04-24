using Assets._Pinball.Scripts.Services;
using SgLib;
using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.UI;

public class ProfileService : MonoBehaviour
{
    public static ProfileService Instance { get; private set; }
    [SerializeField]
    private GameObject profilePage;
    [SerializeField]
    private GameObject loginSection;
    [SerializeField]
    private Text username;
    [SerializeField]
    private Text rank;
    [SerializeField]
    private Text highscore;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public async void Open()
    {

        var score = await GetScoreAsync();
        username.text = score.PlayerName;
        rank.text = score.Tier;
        highscore.text = ScoreManager.Instance.GetHighScore().ToString();
        if (AuthService.Instance.IsAuthenticated)
        {
            loginSection.SetActive(false);
        }
        else
        {
            loginSection.SetActive(true);
        }

        profilePage.SetActive(true);

    }

    private async Task<LeaderboardEntry> GetScoreAsync()
    {
        var score = await LeaderboardService.Instance.GetScore();
        if(score != null) return score;

        var auth = await AuthService.Instance.GetUserAsync();
        var highScore = ScoreManager.Instance.GetHighScore();
        return new LeaderboardEntry(auth.PlayerId, auth.Username, 0, highScore, "-", DateTime.Now);
    }

    public void Back()
    {
        profilePage.SetActive(false);
    }
}
