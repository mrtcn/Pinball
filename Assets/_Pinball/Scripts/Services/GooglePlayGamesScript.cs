using Assets._Pinball.Scripts.Models;
using Assets._Pinball.Scripts.Models.Enums;
using Assets._Pinball.Scripts.Services;
#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif
using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;

public class GooglePlayGamesScript : MonoBehaviour
{
    public event Action<UserInfo> OnGoogleUserLogIn = delegate { };
    public event Action OnGoogleUserLogOut = delegate { };

    public static GooglePlayGamesScript Instance;
    public string Error;
    private void Start()
    {
#if UNITY_ANDROID
        Activate();
#endif
    }
    private void Awake()
    {
        Instance = this;
    }

    public void Activate()
    {
#if UNITY_ANDROID
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
            // enables saving game progress.
            .EnableSavedGames()
            // requests the email address of the player be available.
            // Will bring up a prompt for consent.
            .RequestEmail()
            // requests a server auth code be generated so it can be passed to an
            //  associated back end server application and exchanged for an OAuth token.
            .RequestServerAuthCode(false)
            // requests an ID token be generated.  This OAuth token can be used to
            //  identify the player to other services such as Firebase.
            .RequestIdToken()
            .Build();
        PlayGamesPlatform.InitializeInstance(config);        
        // recommended for debugging:
        PlayGamesPlatform.DebugLogEnabled = true;
        // Activate the Google Play Games platform
        PlayGamesPlatform.Activate();
#endif
    }

    public void LoginGooglePlayGames(bool silent = false)
    {

#if UNITY_ANDROID
        PlayGamesPlatform.Instance.Authenticate(async (success) =>
        {
            if (success)
            {
                var code = PlayGamesPlatform.Instance.GetServerAuthCode();
                try
                {
                    var name = await AuthenticationService.Instance.GetPlayerNameAsync();
                    // Shows how to get a playerID
                    Debug.Log($"DebugLogs PlayerName1: {name}");
                    Debug.Log($"DebugLogs Social.localUser.userName: {Social.localUser.userName}");
                    Debug.Log($"PlayGamesPlatform IsAuthenticated: {IsAuthenticated()}");
                    Debug.Log($"PlayGamesPlatform Social.localUser.authenticated: {Social.localUser.authenticated}");
                    await Link(code);
                    Debug.Log($"PlayGamesPlatform IsAuthenticated2: {IsAuthenticated()}");
                    Debug.Log($"PlayGamesPlatform Social.localUser.authenticated2: {Social.localUser.authenticated}");
                    OnGoogleUserLogIn(new UserInfo(Social.localUser.userName, name, "", AuthenticationType.GooglePlayGames, Social.localUser.id));

                    ExternalProviderStateHelper.SetLastLoginState(ExternalProviderLastLoginType.Google_State, ExternalProviderState.LoggedIn);
                }
                catch (AuthenticationException ex)
                {
                    if (ex.ErrorCode != 10003) throw;

                    Debug.Log($"AuthenticationException: ErrorCode: {ex.ErrorCode} - Message: {ex.Message}");
                }
                Debug.Log("Login with Google Play games successful.");

            }
            else
            {
                Error = "Failed to retrieve Google play games authorization code";
                Debug.Log("Login Unsuccessful");
            }
        }, silent);
#endif
    }

    public async Task Link(string code)
    {
        try
        {
            var linkOpt = new LinkOptions();
            linkOpt.ForceLink = true;
            await AuthenticationService.Instance.LinkWithGooglePlayGamesAsync(code, linkOpt);
        }
        catch (AuthenticationException ex)
        {
            Debug.Log($"Link AuthenticationException: ErrorCode: {ex.ErrorCode} - Message: {ex.Message}");
            await AuthenticationService.Instance.SignInWithGoogleAsync(code);
        }
    }

    public bool IsAuthenticated()
    {

#if UNITY_ANDROID
        return PlayGamesPlatform.Instance.IsAuthenticated();
#else
        return false;
#endif
    }

    public void Logout()
    {

#if UNITY_ANDROID
        PlayGamesPlatform.Instance.SignOut();
        ExternalProviderStateHelper.SetLastLoginState(ExternalProviderLastLoginType.Google_State, ExternalProviderState.LoggedOut);
        OnGoogleUserLogOut();
#endif
    }

}
