using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Assets._Pinball.Scripts.Services;
using Unity.Services.Authentication;
using GooglePlayGames;

#if EASY_MOBILE
using EasyMobile;
#endif

public class UIManager : Singleton<UIManager>
{
    public static bool firstLoad = true;
    public Text score;
    public Text scoreInScoreBg;
    public Text bestScore;
    public GameObject buttons;
    public Button muteBtn;
    public Button unMuteBtn;

    [Header("Premium Buttons")]
    public GameObject leaderboardBtn;
    public GameObject achievementBtn;
    public GameObject removeAdsBtn;
    public GameObject restorePurchaseBtn;
    public GameObject shareBtn;

    private ScoreSO scoreSO;

    // Use this for initialization
    void Start()
    {
        scoreSO = ScriptableObject.FindObjectOfType<ScoreSO>() ?? ScriptableObject.CreateInstance<ScoreSO>();
        scoreSO.Init();
        score.gameObject.SetActive(false);
        scoreInScoreBg.text = scoreSO.Score.ToString();

        // Show or hide premium buttons
        bool enablePremium = PremiumFeaturesManager.Instance.enablePremiumFeatures;
        leaderboardBtn.SetActive(enablePremium);
        achievementBtn.SetActive(enablePremium);
        removeAdsBtn.SetActive(enablePremium);
        restorePurchaseBtn.SetActive(enablePremium);
        shareBtn.SetActive(false);  // share button only shows when game over
    }

    // Update is called once per frame
    void Update()
    {
        if(score != null)
            score.text = scoreSO.Score.ToString();
        bestScore.text = scoreSO.HighScore.ToString();
        UpdateMuteButtons();
    }

    public void HandlePlayButton()
    {
        SceneManager.LoadScene("Level1");
    }

    public void ShowButtons()
    {
        buttons.SetActive(true);
        score.gameObject.SetActive(false);
        scoreInScoreBg.text = scoreSO.Score.ToString();

        bool enablePremium = PremiumFeaturesManager.Instance.enablePremiumFeatures;
        leaderboardBtn.SetActive(enablePremium);
        achievementBtn.SetActive(enablePremium);
        removeAdsBtn.SetActive(enablePremium);
        restorePurchaseBtn.SetActive(enablePremium);
        shareBtn.SetActive(enablePremium);

    }

    void UpdateMuteButtons()
    {
        if (SoundManager.Instance.IsMuted())
        {
            unMuteBtn.gameObject.SetActive(false);
            muteBtn.gameObject.SetActive(true);
        }
        else
        {
            unMuteBtn.gameObject.SetActive(true);
            muteBtn.gameObject.SetActive(false);
        }
    }

    public async void ShowLeaderboardUI()
    {
        await LeaderboardService.Instance.LoadLeaderboard();
//#if EASY_MOBILE
//        if (GameServices.IsInitialized())
//        {
//            GameServices.ShowLeaderboardUI();
//        }
//        else
//        {
//#if UNITY_IOS
//            NativeUI.Alert("Service Unavailable", "The user is not logged in to Game Center.");
//#elif UNITY_ANDROID
//            GameServices.Init();
//#endif
//        }
//#endif
    }

    public void ShowAchievementUI()
    {
        #if EASY_MOBILE
        if (GameServices.IsInitialized())
        {
            GameServices.ShowAchievementsUI();
        }
        else
        {
        #if UNITY_IOS
            NativeUI.Alert("Service Unavailable", "The user is not logged in to Game Center.");
        #elif UNITY_ANDROID
            GameServices.Init();
        #endif
        }
        #endif
    }

    public void PurchaseRemoveAds()
    {
        #if EASY_MOBILE
        InAppPurchaser.Instance.Purchase(InAppPurchaser.Instance.removeAds);
        #endif
    }

    public void RestorePurchase()
    {
        #if EASY_MOBILE
        InAppPurchaser.Instance.RestorePurchase();
        #endif
    }

    public void ShareScreenshot()
    {
#if EASY_MOBILE
        ScreenshotSharer.Instance.ShareScreenshot();
#endif
    }

    public void ToggleSound()
    {
        SoundManager.Instance.ToggleMute();
    }

    public void RateApp()
    {
        GooglePlayGamesScript.Instance.LoginGooglePlayGames();
        DebugLogs();
        //Utilities.Instance.RateApp();
    }

    public void OpenTwitterPage()
    {
        //Utilities.Instance.OpenTwitterPage();
    }

    public async void OpenFacebookPage()
    {
        await AuthService.Instance.SignInAnonymouslyAsync();
        DebugLogs();
        //Utilities.Instance.OpenFacebookPage();
    }

    public void OpenProfilePage()
    {
        ProfileService.Instance.Open();
    }



    private static void DebugLogs()
    {
        Debug.Log($"DebugLogs UI Profile: {AuthenticationService.Instance.Profile}");
        Debug.Log($"DebugLogs UI PlayerId: {AuthenticationService.Instance.PlayerId}");
        Debug.Log($"DebugLogs UI IsSignedIn: {AuthenticationService.Instance.IsSignedIn}");
        Debug.Log($"DebugLogs UI SessionTokenExists: {AuthenticationService.Instance.SessionTokenExists}");
        Debug.Log($"DebugLogs UI IsExpired: {AuthenticationService.Instance.IsExpired}");
        Debug.Log($"DebugLogs UI AuthService.Instance.IsAuthenticated: {AuthService.Instance.IsAuthenticated}");
        Debug.Log($"DebugLogs UI Identities: {AuthenticationService.Instance.PlayerInfo?.Identities?.Count}");
        if (AuthenticationService.Instance.PlayerInfo?.Identities?.Count > 0)
        {
            foreach (var identity in AuthenticationService.Instance.PlayerInfo?.Identities)
            {
                Debug.Log($"DebugLogs Identities: {identity.UserId} - {identity.TypeId}");
            }
        }
#if UNITY_ANDROID
        Debug.Log($"DebugLogs UI GetUserDisplayName: {PlayGamesPlatform.Instance.GetUserDisplayName()}");
        Debug.Log($"DebugLogs UI GetUserImageUrl: {PlayGamesPlatform.Instance.GetUserImageUrl()}");
        Debug.Log($"DebugLogs UI IsAuthenticated: {PlayGamesPlatform.Instance.IsAuthenticated()}");
        Debug.Log($"DebugLogs UI localUser.userName: {PlayGamesPlatform.Instance.localUser.userName}");
        Debug.Log($"DebugLogs UI localUser.authenticated: {PlayGamesPlatform.Instance.localUser.authenticated}");
        Debug.Log($"DebugLogs UI localUser.state: {PlayGamesPlatform.Instance.localUser.state}");
#endif
    }
}
