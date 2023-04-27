using Assets._Pinball.Scripts.Models;
using Assets._Pinball.Scripts.Models.Enums;
using Assets._Pinball.Scripts.Services;
using GooglePlayGames;
using System;
using System.Collections.Generic;
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
            foreach( var identity in AuthenticationService.Instance.PlayerInfo?.Identities)
            {
                Debug.Log($"DebugLogs Identities: {identity.UserId} - {identity.TypeId}");
            }
        }
        
    }

    private async void Start()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
            await SignInAnonymouslyAsync();
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
    }

    public bool IsAuthenticated
    {
        get {
            var isGoogleAuthenticated = GooglePlayGamesScript.Instance.IsAuthenticated();
            var isFacebookAuthenticated = FacebookScript.Instance.IsAuthenticated();
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
            if (!IsAuthenticated)
                LoginExternal();
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
            Debug.LogError("Sign in anonymously succeeded!");
            OnUserLoggedIn();

            if (string.IsNullOrWhiteSpace(AuthenticationService.Instance.PlayerName))
                UpdateNameDialog.Instance.OpenDialog(true);
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
        var authTypes = GetAuthenticationTypes();
        foreach (var authType in authTypes) 
        {
            switch (authType)
            {
                case AuthenticationType.Anonymous:
                    break;
                case AuthenticationType.GooglePlayGames:
                    var googleLastLoginState = ExternalProviderStateHelper.LastLoginState(ExternalProviderLastLoginType.Google_State);
                    if(googleLastLoginState == ExternalProviderState.LoggedIn)
                        GooglePlayGamesScript.Instance.LoginGooglePlayGames(true);
                    break;
                case AuthenticationType.AppleGameCenter:
                    break;
                case AuthenticationType.Facebook:
                    var facebookLastLoginState = ExternalProviderStateHelper.LastLoginState(ExternalProviderLastLoginType.Facebook_State);
                    if (facebookLastLoginState == ExternalProviderState.LoggedIn)
                        FacebookScript.Instance.Login();
                    break;
                case AuthenticationType.Unknown:
                    break;
                default:
                    break;
            }
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

        var googlePlayId = Social.localUser.id;
        Debug.Log($"GooglePlayId: {googlePlayId}");
        if (!string.IsNullOrWhiteSpace(googlePlayId) && GooglePlayGamesScript.Instance.IsAuthenticated()) return AuthenticationType.GooglePlayGames;

        var facebookId = AuthenticationService.Instance.PlayerInfo.GetFacebookId();
        Debug.Log($"FacebookId: {facebookId}");
        if (!string.IsNullOrWhiteSpace(facebookId) && FacebookScript.Instance.IsAuthenticated()) return AuthenticationType.Facebook;

        return AuthenticationType.Anonymous;
    }
    private List<AuthenticationType> GetAuthenticationTypes() 
    {         
        var authenticationTypes = new List<AuthenticationType>();
        if (AuthenticationService.Instance.PlayerInfo?.Identities.Count == 0) return authenticationTypes;

        foreach (var identity in AuthenticationService.Instance.PlayerInfo?.Identities)
        {
            if(identity.TypeId.Contains("google"))
                authenticationTypes.Add(AuthenticationType.GooglePlayGames);
            else if (identity.TypeId.Contains("facebook"))
                authenticationTypes.Add(AuthenticationType.Facebook);
            else if (identity.TypeId.Contains("apple"))
                authenticationTypes.Add(AuthenticationType.AppleGameCenter);
        }

        return authenticationTypes;
    }
}
