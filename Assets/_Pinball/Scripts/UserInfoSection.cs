using Assets._Pinball.Scripts.Models;
using Assets._Pinball.Scripts.Services;
using SgLib;
using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class UserInfoSection : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI usernameTextField;
    [SerializeField]
    private TextMeshProUGUI scoreAndTier;
    [SerializeField]
    private Image profileImage;
    [SerializeField]
    private Texture2D placeholderImage;
    // Start is called before the first frame update
    void Start()
    {
        UserService.Instance.OnUsernameUpdate += UsernameUpdate;
        AuthService.Instance.OnUserLoggedIn += UserLoggedIn;
        GooglePlayGamesScript.Instance.OnGoogleUserLoggedIn += UserLoggedIn;
        UserCacheService.Instance.OnUserInfoUpdate += UserInfoUpdated;
    }

    private void OnDestroy()
    {
        UserService.Instance.OnUsernameUpdate -= UsernameUpdate;
        AuthService.Instance.OnUserLoggedIn -= UserLoggedIn;
        GooglePlayGamesScript.Instance.OnGoogleUserLoggedIn -= UserLoggedIn;
        UserCacheService.Instance.OnUserInfoUpdate -= UserInfoUpdated;
    }

    private async void UsernameUpdate()
    {
        await UpdateAllFields();
    }

    private async void UserLoggedIn()
    {
        await UpdateAllFields();
    }

    private async void UserInfoUpdated(UserInfo userInfo)
    {
        await UpdateAllFields();
    }

    private async Task UpdateAllFields()
    {
        var userInfo = UserCacheService.Instance.GetUserInfo();
        userInfo = userInfo ?? new UserInfo(null, AuthenticationService.Instance.PlayerName, null, AuthenticationType.Anonymous, null);
        var score = await LeaderboardService.Instance.GetScore();
        score = score ?? new Unity.Services.Leaderboards.Models.LeaderboardEntry(AuthenticationService.Instance.PlayerId, AuthenticationService.Instance.PlayerName, 0, 0, "Rookie", DateTime.Now);
        usernameTextField.text = userInfo.Username;
        scoreAndTier.text = $"{score.Tier} {score.Score}";
        if(string.IsNullOrWhiteSpace(userInfo.ImagePath))
            Utilities.Instance.LoadImage(placeholderImage, profileImage);
        else
            StartCoroutine(Utilities.Instance.LoadImage(userInfo.ImagePath, profileImage));
    }
}
