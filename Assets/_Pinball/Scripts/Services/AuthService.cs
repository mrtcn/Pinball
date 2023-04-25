using Assets._Pinball.Scripts.Models;
using Assets._Pinball.Scripts.Services;
using GooglePlayGames;
using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

public class AuthService : MonoBehaviour
{
    public static AuthService Instance { get; private set; }
    public Action UserLoggedOut = delegate { };
    public Action OnUserLoggedIn = delegate { };

    private static void DebugLogs()
    {
        Debug.Log($"DebugLogs Profile: {AuthenticationService.Instance.Profile}");
        Debug.Log($"DebugLogs PlayerId: {AuthenticationService.Instance.PlayerId}");
        Debug.Log($"DebugLogs IsSignedIn: {AuthenticationService.Instance.IsSignedIn}");
        Debug.Log($"DebugLogs SessionTokenExists: {AuthenticationService.Instance.SessionTokenExists}");
        Debug.Log($"DebugLogs IsExpired: {AuthenticationService.Instance.IsExpired}");
        Debug.Log($"DebugLogs GetUserDisplayName: {PlayGamesPlatform.Instance.GetUserDisplayName()}");
        Debug.Log($"DebugLogs GetUserImageUrl: {PlayGamesPlatform.Instance.GetUserImageUrl()}");
        Debug.Log($"DebugLogs IsAuthenticated: {PlayGamesPlatform.Instance.IsAuthenticated()}");
        Debug.Log($"DebugLogs AuthService.Instance.IsAuthenticated: {AuthService.Instance.IsAuthenticated}");
        Debug.Log($"DebugLogs localUser.userName: {PlayGamesPlatform.Instance.localUser.userName}");
        Debug.Log($"DebugLogs localUser.authenticated: {PlayGamesPlatform.Instance.localUser.authenticated}");
        Debug.Log($"DebugLogs localUser.state: {PlayGamesPlatform.Instance.localUser.state}");
        Debug.Log($"DebugLogs IdentitiesCount: {AuthenticationService.Instance.PlayerInfo?.Identities?.Count}");
        if(AuthenticationService.Instance.PlayerInfo?.Identities?.Count > 0)
        {
            Debug.Log($"DebugLogs Identities: {JsonUtility.ToJson(AuthenticationService.Instance.PlayerInfo?.Identities)}");
        }
        
    }

    async void Awake()
    {
        Instance = this;
        var initOpt = new InitializationOptions();
#if UNITY_EDITOR
        initOpt.SetEnvironmentName("test");
#else
        initOpt.SetEnvironmentName("production");
#endif
        await UnityServices.InitializeAsync(initOpt);

        SetupEvents();
        DebugLogs();

        await SignInAnonymouslyAsync();
    }

    public bool IsAuthenticated
    {
        get {
            var isGoogleAuthenticated = GooglePlayGamesScript.Instance.IsAuthenticated();
            var isFacebookAuthenticated = false;
            var isIosAuthenticated = false;
            return isGoogleAuthenticated || isFacebookAuthenticated || isIosAuthenticated; 
        }
        private set { }
    }

    public async Task SignInAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            
            Debug.Log("Sign in anonymously succeeded!");
            DebugLogs();

        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

    // Setup authentication event handlers if desired
    void SetupEvents()
    {
        AuthenticationService.Instance.SignedIn += async () => {
            OnUserLoggedIn();
            if(!IsAuthenticated)
                LoginExternal();
            
            // Shows how to get an access token
            Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");

        };

        AuthenticationService.Instance.SignInFailed += (err) => {
            Debug.LogError(err);
        };

        AuthenticationService.Instance.SignedOut += () => {
            Debug.Log("Player signed out.");
            UserLoggedOut();
        };

        AuthenticationService.Instance.Expired += () =>
        {
            Debug.Log("Player session could not be refreshed and expired.");
        };
    }

    private void LoginExternal()
    {
        var authType = GetAuthenticationType();
        switch (authType)
        {
            case AuthenticationType.Anonymous:
                break;
            case AuthenticationType.GooglePlayGames:
                GooglePlayGamesScript.Instance.LoginGooglePlayGames();
                break;
            case AuthenticationType.AppleGameCenter:
                break;
            case AuthenticationType.Facebook:
                break;
            case AuthenticationType.Unknown:
                break;
            default:
                break;
        }
    }

    public async Task<User> GetUserAsync()
    {
        var score = await LeaderboardService.Instance.GetScore();
        if (!IsAuthenticated)
        {
            var anonymousUsername = await AuthenticationService.Instance.GetPlayerNameAsync();
            anonymousUsername = string.IsNullOrWhiteSpace(anonymousUsername) ? "Anonymous" : anonymousUsername;
            return new User(AuthenticationService.Instance.PlayerId, anonymousUsername, "", (int)score.Score, score.Tier);
        }
            
        var image = GetUserImage();
        var username = string.IsNullOrWhiteSpace(Social.localUser.userName) ? "Anonymous" : Social.localUser.userName;
        return new User(AuthenticationService.Instance.PlayerId, username, image, (int)score.Score, score.Tier);
    }

    private string GetUserImage()
    {
        var authType = GetAuthenticationType();
        switch (authType)
        {
            case AuthenticationType.Anonymous:
                return "";
            case AuthenticationType.GooglePlayGames:
                return PlayGamesPlatform.Instance.GetUserImageUrl();
            case AuthenticationType.AppleGameCenter:
                return "";
            case AuthenticationType.Facebook:
                return "";
            case AuthenticationType.Unknown:
                return "";
            default:
                return "";
        }
        
    }

    public void LogoutUser()
    {
        AuthenticationService.Instance.SignOut();        
    }

    public AuthenticationType GetAuthenticationType() 
    {
        var appleId = AuthenticationService.Instance.PlayerInfo.GetAppleId();
        Debug.Log($"AppleId: {appleId}");
        if (!string.IsNullOrWhiteSpace(appleId)) return AuthenticationType.AppleGameCenter;

        var googlePlayId = AuthenticationService.Instance.PlayerInfo.GetGooglePlayGamesId();
        Debug.Log($"GooglePlayId: {googlePlayId}");
        if (!string.IsNullOrWhiteSpace(googlePlayId)) return AuthenticationType.GooglePlayGames;

        var facebookId = AuthenticationService.Instance.PlayerInfo.GetFacebookId();
        Debug.Log($"FacebookId: {facebookId}");
        if (!string.IsNullOrWhiteSpace(facebookId)) return AuthenticationType.Facebook;

        return AuthenticationType.Anonymous;
    }
}
