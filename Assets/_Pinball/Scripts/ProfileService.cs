using Assets._Pinball.Scripts.Models;
using Firebase.Crashlytics;
using SgLib;
using System;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class ProfileService : MonoBehaviour
{
    public Action OnUsernameUpdate = delegate { };
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
    private Text username;
    [SerializeField]
    private Text rank;
    [SerializeField]
    private Text highscore;
    [SerializeField]
    private Image image;
    [SerializeField]
    private GameObject usernameEditDialog;
    [SerializeField]
    private TMP_InputField usernameInputField;
    [SerializeField]
    private TextMeshProUGUI inputFieldError;

    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        GooglePlayGamesScript.Instance.OnGoogleUserLoggedIn += GoogleUserLoggedIn;
        AuthService.Instance.UserLoggedOut += GoogleUserLoggedOut;
    }

    private void GoogleUserLoggedOut()
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
        AuthService.Instance.UserLoggedOut -= GoogleUserLoggedOut;
    }

    public async void Load()
    {

        var userInfo = await AuthService.Instance.GetUserAsync();
        username.text = userInfo.Username;
        usernameInputField.text = userInfo.Username;
        rank.text = userInfo.Rank;
        highscore.text = userInfo.HighScore.ToString();
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

    }

    public void LoginAppleGameCenter()
    {

    }

    public void Logout()
    {
        AuthService.Instance.LogoutUser();
    }

    public async void UpdateEditDialog()
    {
        if(!Regex.Match(usernameInputField.text, @"^[a-zA-Z0-9]{3,32}").Success)
        {
            inputFieldError.text = "Please enter a valid username";
            return;
        }

        try
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(usernameInputField.text);
        }
        catch (Exception ex)
        {
            Crashlytics.LogException(ex);
            inputFieldError.text = "An error occured during updating the username.";
        }

        username.text = usernameInputField.text;
        inputFieldError.text = "";
        OnUsernameUpdate();
        usernameEditDialog.SetActive(false);
    }

    public void CloseEditDialog()
    {
        inputFieldError.text = "";
        usernameInputField.text = username.text;
        usernameEditDialog.SetActive(false);

    }
}
