using Assets._Pinball.Scripts.Models;
using Assets._Pinball.Scripts.Services;
using SgLib;
using System;
using Unity.Services.Authentication;
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
    private GameObject logoutSection;
    [SerializeField]
    private GameObject loginGooglePlayGames;
    [SerializeField]
    private GameObject loginIosGameCenter;
    [SerializeField]
    private GameObject loginFacebook;
    [SerializeField]
    private GameObject logoutGooglePlayGames;
    [SerializeField]
    private GameObject logoutIosGameCenter;
    [SerializeField]
    private GameObject logoutFacebook;
    [SerializeField]
    private Text usernameTextField;
    [SerializeField]
    private Text rank;
    [SerializeField]
    private Text highscore;
    [SerializeField]
    private Image image;
    [SerializeField]
    private Texture2D placeholderImage;

    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        GooglePlayGamesScript.Instance.OnGoogleUserLoggedIn += GoogleUserLoggedIn;
        UserCacheService.Instance.OnUserInfoUpdate += UserInfoUpdated;
        AuthService.Instance.UserLoggedOut += UserLoggedOut;
        UserService.Instance.OnUsernameUpdate += UsernameUpdated;
    }

    private void UsernameUpdated()
    {
        Load();
    }

    private void UserInfoUpdated(UserInfo userInfo)
    {
        Load();
    }

    private void UserLoggedOut()
    {
        Load();
    }

    private void GoogleUserLoggedIn()
    {
        Load();
    }

    // Update is called once per frame
    private void OnDestroy()
    {
        GooglePlayGamesScript.Instance.OnGoogleUserLoggedIn -= GoogleUserLoggedIn;
        UserCacheService.Instance.OnUserInfoUpdate -= UserInfoUpdated;
        AuthService.Instance.UserLoggedOut -= UserLoggedOut;
        UserService.Instance.OnUsernameUpdate -= UsernameUpdated;
    }

    public async void Load()
    {
        var userInfo = UserCacheService.Instance.GetUserInfo();
        userInfo = userInfo ?? new UserInfo(null, AuthenticationService.Instance.PlayerName, null, AuthenticationType.Anonymous, null);
        var score = await LeaderboardService.Instance.GetScore();
        score = score ?? new Unity.Services.Leaderboards.Models.LeaderboardEntry(AuthenticationService.Instance.PlayerId, AuthenticationService.Instance.PlayerName, 0, 0, "Rookie", DateTime.Now);
        usernameTextField.text = userInfo.Username;
        rank.text = score.Tier.ToString();
        highscore.text = score.Score.ToString();
        if(string.IsNullOrWhiteSpace(userInfo.ImagePath))
            Utilities.Instance.LoadImage(placeholderImage, image);
        else
            StartCoroutine(Utilities.Instance.LoadImage(userInfo.ImagePath, image));

        var authType = AuthService.Instance.GetAuthenticationType();
        authType = !AuthService.Instance.IsAuthenticated ? AuthenticationType.Anonymous : authType;
        switch (authType)
        {
            case AuthenticationType.Anonymous:
                //Activate Login Buttons
                EnableLoginSection();
                //Disable not supported Login Buttons
#if UNITY_ANDROID
                SetAndroidLoginButtons();
#elif UNITY_IOS
                SetIosLoginButtons();
#endif

                break;
            case AuthenticationType.GooglePlayGames:
                EnableLogoutSection();
                disableAllLogoutButtons();
                logoutGooglePlayGames.SetActive(true);
                break;
            case AuthenticationType.AppleGameCenter:
                EnableLogoutSection();
                disableAllLogoutButtons();
                logoutIosGameCenter.SetActive(true);
                break;
            case AuthenticationType.Facebook:
                EnableLogoutSection();
                disableAllLogoutButtons();
                logoutFacebook.SetActive(true);
                break;
            default:
                break;
        }

    }

    public void Open()
    {
        Load();
        profilePage.SetActive(true);
    }

    private void SetIosLoginButtons()
    {
        loginGooglePlayGames.SetActive(false);
        loginIosGameCenter.SetActive(true);
        loginFacebook.SetActive(true);
    }

    private void SetAndroidLoginButtons()
    {
        loginGooglePlayGames.SetActive(true);
        loginIosGameCenter.SetActive(false);
        loginFacebook.SetActive(true);
    }

    private void EnableLoginSection()
    {
        loginSection.SetActive(true);
        logoutSection.SetActive(false);
    }

    private void EnableLogoutSection()
    {
        loginSection.SetActive(false);
        logoutSection.SetActive(true);
    }

    private void disableAllLogoutButtons()
    {
        logoutFacebook.SetActive(false);
        logoutIosGameCenter.SetActive(false);
        logoutGooglePlayGames.SetActive(false);
    }

    public void Back()
    {
        profilePage.SetActive(false);
    }

    public void LoginGooglePlay()
    {
        GooglePlayGamesScript.Instance.LoginGooglePlayGames();
    }

    public void LoginFacebook()
    {
        FacebookScript.Instance.Login();
    }

    public void LoginAppleGameCenter()
    {

    }

    public void Logout()
    {
        AuthService.Instance.LogoutUser();
    }
}
